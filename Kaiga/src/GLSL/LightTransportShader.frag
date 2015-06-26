//-----------------------------------------------------------
// Outputs
//-----------------------------------------------------------
layout( location = 0 ) out vec4 out_bounceAndAO;
layout( location = 1 ) out vec4 out_reflections;

//-----------------------------------------------------------
// Inputs
//-----------------------------------------------------------
// TAU_LOOKUP[N-1] = optimal number of spiral turns for N samples
const int TAU_LOOKUP[ ] = 
{
	1, 1, 2, 3, 2, 5, 2, 3, 2, 3, 3, 5, 5, 3, 4, 7, 5, 5, 7, 9, 8, 5, 5, 7, 7, 7, 8, 5, 8, 11, 12, 7, 10, 13, 8, 11,
	8, 7, 14, 11, 11, 13, 12, 13, 19, 17, 13, 11, 18, 19, 11, 11, 14, 17, 21, 15, 16, 17, 18, 13, 17, 11, 17, 19,
	18, 25, 18, 19, 19, 29, 21, 19, 27, 31, 29, 21,	18, 17, 29, 31, 31, 23, 18, 25, 26, 25, 23, 19, 34, 19, 27,	21, 
	25, 39, 29, 17, 21, 27
};
const int NUM_SAMPLES = 48;
const int TAU = TAU_LOOKUP[NUM_SAMPLES];
const float NUM_SAMPLES_RCP = 1.0f / NUM_SAMPLES;
const float PI = 3.142f;
const float Q = 0.006f;			// The screen space radius as which we begin dropping mips
const float BIAS = 0.2f;		// The offset applied to minimise self-occlusion.
const float EPSILON = 0.003f;	// A small offset to avoid divide by zero

const int MAX_REFLECTION_STEPS = 32;
const int NUM_BINARY_SERACH_STEPS = 4;
const float rayStep = 0.01f;
const float MAX_RAY_LENGTH = rayStep * MAX_REFLECTION_STEPS;
const int NUM_REFLECTION_SAMPLES = 1;

uniform sampler2D s_positionBuffer;
uniform sampler2D s_normalBuffer;
uniform sampler2D s_directLightBuffer2D;
uniform sampler2D s_indirectLightBuffer2D;
uniform sampler2D s_randomTexture;
uniform sampler2DRect s_material;

uniform int u_maxMip;
uniform mat4 u_projectionMatrix;
uniform float u_roughnessJitter;
uniform float u_radius;
uniform float u_aoFalloffScalar;
uniform float u_bounceFalloffScalar;
uniform float u_aspectRatio;
uniform float u_radiosityScalar;
uniform float u_colorBleedingBoost;
uniform float u_lightTransportResolutionScalar;
uniform float u_maxReflectDepthDiff;
uniform bool u_flag;

// Inputs
in Varying
{
	in vec2 in_uv;
};

//-----------------------------------------------------------
// Functions
//-----------------------------------------------------------

void RayCast
(
	vec3 dir, 
	vec3 startPos, 
	out vec2 bestHitUV, 
	out vec3 bestPosition, 
	out float smallestDepthDiff
)
{

	vec3 pointAlongRay = startPos;

	vec3 direction = dir;
	float xyLength = length( direction.xy );
	direction /= xyLength;


    pointAlongRay += dir * rayStep;

    dir *= rayStep;

    bestHitUV = vec2(0.0f,0.0f);
    smallestDepthDiff = 99999.0f;
    bestPosition = startPos;

    for(int i = 0; i < MAX_REFLECTION_STEPS; i++)
    {
		pointAlongRay += dir;
		vec4 projectedCoord = u_projectionMatrix * vec4(pointAlongRay, 1.0);
		projectedCoord.xy /= projectedCoord.w;
		projectedCoord.xy = projectedCoord.xy * 0.5 + 0.5;

		float mipLevel = 1.0f + (float(i)/MAX_REFLECTION_STEPS) * 2.0f;

		float depthDiff = pointAlongRay.z - textureLod(s_positionBuffer, projectedCoord.xy, mipLevel).z;

		if ( depthDiff < 0.0f )
		{
			for(int j = 0; j < NUM_BINARY_SERACH_STEPS; j++)
		    {
		        projectedCoord = u_projectionMatrix * vec4(pointAlongRay, 2.0f);
		        projectedCoord.xy /= projectedCoord.w;
		        projectedCoord.xy = projectedCoord.xy * 0.5f + 0.5f;
		 
		        depthDiff = pointAlongRay.z - textureLod(s_positionBuffer, projectedCoord.xy, mipLevel).z;

		        if( depthDiff > 0.0f )
		            pointAlongRay += dir;
		 
		        dir *= -0.5f;
		        pointAlongRay -= dir; 

		        if ( depthDiff < smallestDepthDiff )
				{
					smallestDepthDiff = abs(depthDiff);
					bestHitUV = projectedCoord.xy;
					bestPosition = pointAlongRay;
				}
		    }
			return;
		}
	}
}

void main()
{
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Shared values
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	vec4 material = texture( s_material, gl_FragCoord.xy / u_lightTransportResolutionScalar );
	float roughness = material.x;
	float reflectivity = material.y;
	ivec2 materialTextureSize = textureSize( s_material, 0 );
	vec3 fragPos = texture( s_positionBuffer, in_uv ).xyz;
	vec3 fragNormal = texture( s_normalBuffer, in_uv ).xyz;
	ivec2 randomTextureSize = textureSize( s_randomTexture, 0 );
	vec4 randomSample = texture2D( s_randomTexture, (gl_FragCoord.xy / randomTextureSize) );
	float randomNumber = randomSample.w;
	vec3 viewDir = normalize( -fragPos );

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// AO and Bounce
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	float angle = randomNumber * PI * 2.0f;
	float aoTotal = 0.0f;
	vec3 bounceTotal = vec3(0.0f, 0.0f, 0.0f);
	int numContributingSamples = 1;
	vec3 prominantBounceDirection = vec3(0.0f);
	vec3 prominantAODirection = vec3(0.0f);
	for ( int i = 0; i < NUM_SAMPLES; i++ )
	{
		// The ratio along the spiral
		float theta = NUM_SAMPLES_RCP * (i + 0.5f);
		float currAngle = 2.0f * PI * theta * TAU + angle;

		// Distance away from frag UV
		float h = u_radius * theta;

		// Scale distance based on frag distance from camera.
		// Fragments that are further away have a smaller radius
		float distanceRadiusScale = max( 0.2f, 1.0f / pow(-fragPos.z+1.0f, 2.0f) );
		h *= distanceRadiusScale;

		// UV offset away from frag UV
		vec2 offset = vec2( cos( currAngle ), sin( currAngle ) );
		offset.y *= u_aspectRatio;
		vec2 sampleUV = in_uv + offset * h;

		float mipLevel = min( log2( h / Q ), u_maxMip );
		vec3 samplePosition = textureLod( s_positionBuffer, sampleUV, mipLevel ).xyz;
		vec3 sampleNormal = textureLod( s_normalBuffer, sampleUV, mipLevel ).xyz;

		vec3 sampleDirectColor = textureLod( s_directLightBuffer2D, sampleUV, mipLevel ).xyz;
		vec3 sampleIndirectColor = textureLod( s_indirectLightBuffer2D, sampleUV, mipLevel ).xyz;
		vec3 sampleMaterial = texture( s_material, sampleUV * materialTextureSize.xy  ).xyz;
		float sampleRoughness = sampleMaterial.x;
		vec3 sampleColor = sampleDirectColor + sampleIndirectColor;

		// Fade off samples as they near the edge of the screen
		vec2 edge = vec2( 0.05f, 0.05f * u_aspectRatio );
		vec2 edgeTopLeft = smoothstep( vec2(0.0f), edge, sampleUV.xy );
		vec2 edgeBottomRight = vec2(1.0f) - smoothstep( vec2(1.0f-edge), vec2(1.0f), sampleUV.xy );
		sampleColor *= (edgeTopLeft.x * edgeTopLeft.y * edgeBottomRight.x * edgeBottomRight.y);

		// Fade off bounce light from shiny surfaces, as their color is view dependant.
		sampleColor *= sampleRoughness;


		// Calculate distance and direction between fragment and sample pos
		vec3 v = samplePosition - fragPos;
		float vLength = length(v);
		v = normalize(v);

		// Some values used in modulating ao and bounce
		float denominator = vLength + EPSILON;
		float vDotFragNormalBiased = max( dot( v, fragNormal ) - BIAS, 0.0f );
		float vDotFragNormal = max( dot( v, fragNormal ), 0.0f );

		// Modulate ao
		float aoAmount = 1.0f / pow(vLength+u_aoFalloffScalar, 2.0f);
		aoAmount *= vDotFragNormalBiased > 0.0f ? 1.0f : 0.0f;
		aoTotal += aoAmount;

		// Modulate bounce
		float sampleNormalDotFragNormal = max( dot( -sampleNormal, fragNormal ) + 1.0f, 0.0f );
		float bounceStrength =  1.0f / pow(vLength+u_bounceFalloffScalar, 2.0f);
		bounceStrength *= vDotFragNormal;
		bounceTotal += sampleColor * bounceStrength;

		prominantBounceDirection -= v * max( max( sampleColor.r, sampleColor.g ), sampleColor.b );

		prominantAODirection -= v * aoAmount;

		numContributingSamples++;
	}

	float prominantBounceStrength = 1.0f / pow(length(prominantBounceDirection)+u_bounceFalloffScalar, 1.0f);
	float prominantAOStrength = 1.0f / pow(length(prominantAODirection)+u_radiosityScalar, 1.0f);

	// Normalise bounce
	bounceTotal /= numContributingSamples;
	float maxBounceChannel = max( max( bounceTotal.r, bounceTotal.g ), bounceTotal.b );
	float minBounceChannel = min( min( bounceTotal.r, bounceTotal.g ), bounceTotal.b );
	float colorBoost = max( 0.0f, (maxBounceChannel - minBounceChannel) / maxBounceChannel );
	colorBoost = mix( 1.0f, colorBoost, u_colorBleedingBoost );
	bounceTotal *= u_radiosityScalar * colorBoost;

	// Apply fresnel to bounce
	prominantBounceDirection = normalize(prominantBounceDirection);
	vec3 halfVector = normalize(viewDir + prominantBounceDirection);
	float dotLH = clamp(dot(prominantBounceDirection,halfVector), 0.0f, 1.0f);
	float F = Fresnel( dotLH, reflectivity );
	bounceTotal *= F;

	// Normalise ao
	aoTotal /= numContributingSamples;
	//aoTotal *= 1.0f + u_aoFalloffScalar * u_aoFalloffScalar;
	aoTotal *= roughness;

	bounceTotal *= roughness;

	// Module bounce light by ao
	prominantAODirection = normalize(prominantAODirection);
	float prominantDot = 1.0f - clamp( dot( prominantAODirection, prominantBounceDirection ), 0.0f, 1.0f );
	if ( u_flag )
	{
		bounceTotal -= prominantDot * aoTotal * prominantBounceStrength * prominantAOStrength;
		bounceTotal = max( vec3(0.0f), bounceTotal );
	}

	aoTotal = 1.0f - aoTotal;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Reflections
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	vec3 reflectionColor = vec3(0.0f,0.0f,0.0f);
	float reflectionAlpha = 0.0f;
	vec3 reflectionFragNormal = fragNormal;

	// Multiply by fresnel
	for ( int i = 0; i < NUM_REFLECTION_SAMPLES; i++ )
	{
		vec3 randomSampleReflect = texture2D( s_randomTexture, (gl_FragCoord.xy / randomTextureSize) + randomSample.xy * NUM_REFLECTION_SAMPLES * 10.0f ).xyz;
		vec3 randomDirection = randomSampleReflect.xyz;
		reflectionFragNormal += randomDirection * roughness * u_roughnessJitter;
		reflectionFragNormal = normalize(reflectionFragNormal);

		vec3 reflectVec = reflect( normalize( fragPos ), fragNormal ); 

		vec2 hitUV;					// Output
		vec3 hitPos;				// Output
		float hitDepthDiff;			// Output

		RayCast(reflectVec, fragPos, hitUV, hitPos, hitDepthDiff);
		reflectionColor += textureLod(s_directLightBuffer2D, hitUV, 1.0 ).rgb + textureLod(s_indirectLightBuffer2D, hitUV, 1.0 ).rgb;
		
		// Now we have a ray intersection, and we've got our color/normal sample.
		// Time to find all the things wrong with this sample, and modulate it to make its
		// shortcomings less obvious.
		float reflectionAlphaCurrent = 1.0f;

		// The RayCast function returns the point with the smallest depth difference it can find.
		// If the smallest depth difference it can find is still rather large, it's likely we're just missing
		// information, and shouldn't be reflecting this pixel.
		float confidence = 1.0f - min( hitDepthDiff / u_maxReflectDepthDiff, 1.0f );
		reflectionAlphaCurrent *= confidence;

		// The ray will eventually just stop, and we'd like to fade a bit before it does so
		// there's no hard edge past a certain ray distance.
		float distanceStrength = 1.0f - min( length(hitPos - fragPos) / MAX_RAY_LENGTH, 1.0f );
		reflectionAlphaCurrent *= distanceStrength;

		// Rays that are pointing towards the camera are suspect, because we have no 'back face'
		// information in the framebuffer. 
		float backFaceFalloff = max( 0.0f, -reflectVec.z );
		reflectionAlphaCurrent *= backFaceFalloff;

		// Multiply by fresnel
		vec3 halfVector = normalize(viewDir + reflectVec);
		float dotLH = clamp(dot(reflectVec,halfVector), 0.0f, 1.0f);
		float F = Fresnel( dotLH, reflectivity );
		reflectionAlphaCurrent *= F;

		reflectionAlpha += reflectionAlphaCurrent;
	}
	reflectionColor /= NUM_REFLECTION_SAMPLES;
	reflectionAlpha /= NUM_REFLECTION_SAMPLES;

	// Non-shiny things shouldn't reflect
	reflectionAlpha *= (1.0f-roughness);

	out_bounceAndAO = vec4( bounceTotal, aoTotal );
	out_reflections = vec4( reflectionColor, reflectionAlpha );
}
