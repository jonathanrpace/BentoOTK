#version 450 core

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
const int NUM_SAMPLES = 32;
const int TAU = TAU_LOOKUP[NUM_SAMPLES];
const float NUM_SAMPLES_RCP = 1.0f / NUM_SAMPLES;
const float PI = 3.142f;
const float Q = 0.006f;			// The screen space radius as which we begin dropping mips
const float BIAS = 0.2f;		// The offset applied to minimise self-occlusion.
const float EPSILON = 0.003f;	// A small offset to avoid divide by zero

const int MAX_REFLECTION_STEPS = 128;
const int NUM_BINARY_SERACH_STEPS = 6;
uniform float rayStep;// = 0.1f;
const float MAX_RAY_LENGTH = rayStep * MAX_REFLECTION_STEPS;

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
uniform float u_falloffScalar;
uniform float u_aspectRatio;
uniform float u_radiosityScalar;
uniform float u_colorBleedingBoost;
uniform float u_lightTransportResolutionScalar;
uniform float u_maxReflectDepthDiff;

// Inputs
in Varying
{
	in vec2 in_uv;
};

//-----------------------------------------------------------
// Functions
//-----------------------------------------------------------
void AOAndBounce
( 
	vec3 fragPos, 
	vec3 fragNormal, 
	vec3 samplePos, 
	vec3 sampleColor, 
	out float aoTotal, 
	out vec3 bounceTotal
)
{
	vec3 v = samplePos - fragPos;
	float vLength = length(v);
	v = normalize(v);

	float falloff = 1.0f - min(1.0f, vLength / u_falloffScalar );
	
	float vDotFragNormal = max( dot( v, fragNormal ) - BIAS, 0.0f );

	float denominator = vLength + EPSILON;
	aoTotal += falloff * min( 1.0f, ( vDotFragNormal / denominator ) );
	bounceTotal += sampleColor * falloff * min( 1.0f, ( vDotFragNormal  ) / denominator );
}

vec2 BinarySearch(vec3 dir, inout vec3 pointAlongRay, out float depthDiff)
{
    for(int i = 0; i < NUM_BINARY_SERACH_STEPS; i++)
    {
        vec4 projectedCoord = u_projectionMatrix * vec4(pointAlongRay, 1.0f);
        projectedCoord.xy /= projectedCoord.w;
        projectedCoord.xy = projectedCoord.xy * 0.5f + 0.5f;
 
        depthDiff = pointAlongRay.z - texture(s_positionBuffer, projectedCoord.xy).z;

        if( depthDiff > 0.0f )
            pointAlongRay += dir;
 
        dir *= 0.5f;
        pointAlongRay -= dir;  
    }

    vec4 projectedCoord = u_projectionMatrix * vec4(pointAlongRay, 1.0f);
    projectedCoord.xy /= projectedCoord.w;
    projectedCoord.xy = projectedCoord.xy * 0.5f + 0.5f;
 
    return projectedCoord.xy;
}

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

    float distanceAlongRay = 0.0f;
    for(int i = 0; i < MAX_REFLECTION_STEPS; i++)
    {
		pointAlongRay += dir;
		vec4 projectedCoord = u_projectionMatrix * vec4(pointAlongRay, 1.0);
		projectedCoord.xy /= projectedCoord.w;
		projectedCoord.xy = projectedCoord.xy * 0.5 + 0.5;

		float depthDiff = abs( pointAlongRay.z - texture(s_positionBuffer, projectedCoord.xy).z );

		depthDiff += distanceAlongRay * 0.05f;

		if ( depthDiff < smallestDepthDiff )
		{
			smallestDepthDiff = depthDiff;
			bestHitUV = projectedCoord.xy;
			bestPosition = pointAlongRay;
		}

		distanceAlongRay += rayStep;
	}
}

void main()
{
	vec4 material = texture( s_material, gl_FragCoord.xy / u_lightTransportResolutionScalar );
	float roughness = material.x;

	vec3 fragPos = texture( s_positionBuffer, in_uv ).xyz;
	vec3 fragNormal = texture( s_normalBuffer, in_uv ).xyz;
	
	ivec2 randomTextureSize = textureSize( s_randomTexture, 0 );
	vec4 randomSample = texture2D( s_randomTexture, (gl_FragCoord.xy / randomTextureSize) );
	//vec4 randomSample2 = texture2D( s_randomTexture, (gl_FragCoord.xy / randomTextureSize) * 0.5f );

	float randomNumber = randomSample.w;

	float angle = randomNumber * PI * 2.0f;
	float aoTotal = 0.0f;
	vec3 bounceTotal = vec3(0.0f, 0.0f, 0.0f);
	int numContributingSamples = 1;
	for ( int i = 0; i < NUM_SAMPLES; i++ )
	{
		// The ratio along the spiral
		float theta = NUM_SAMPLES_RCP * (i + 0.5f);
		float currAngle = 2.0f * PI * theta * TAU + angle;

		// Distance away from frag UV
		float h = u_radius * theta;

		// UV offset away from frag UV
		vec2 offset = vec2( cos( currAngle ), sin( currAngle ) );
		offset.y *= u_aspectRatio;
		vec2 sampleUV = in_uv + offset * h;

		float mipLevel = min( log2( h / Q ), u_maxMip );
		vec3 samplePosition = textureLod( s_positionBuffer, sampleUV, mipLevel ).xyz;

		if ( abs(samplePosition.x) < 0.0001f )
		{
			continue;
		}

		vec3 sampleDirectColor = textureLod( s_directLightBuffer2D, sampleUV, mipLevel ).xyz;
		vec3 sampleIndirectColor = textureLod( s_indirectLightBuffer2D, sampleUV, mipLevel ).xyz;
		vec3 sampleColor = sampleDirectColor + sampleIndirectColor;

		// Fade off samples as they near the edge of the screen
		vec2 edge = vec2( 0.05f, 0.05f * u_aspectRatio );
		vec2 edgeTopLeft = smoothstep( vec2(0.0f), edge, sampleUV.xy );
		vec2 edgeBottomRight = vec2(1.0f) - smoothstep( vec2(1.0f-edge), vec2(1.0f), sampleUV.xy );
		sampleColor *= (edgeTopLeft.x * edgeTopLeft.y * edgeBottomRight.x * edgeBottomRight.y);

		AOAndBounce( fragPos, fragNormal, samplePosition, sampleColor, aoTotal, bounceTotal );

		numContributingSamples++;
	}

	// Normalise bounds
	bounceTotal /= numContributingSamples;
	float maxBounceChannel = max( max( bounceTotal.r, bounceTotal.g ), bounceTotal.b );
	float minBounceChannel = min( min( bounceTotal.r, bounceTotal.g ), bounceTotal.b );
	float colorBoost = max( 0.0f, (maxBounceChannel - minBounceChannel) / maxBounceChannel );
	colorBoost = mix( 1.0f, colorBoost, u_colorBleedingBoost );
	bounceTotal *= u_radiosityScalar * colorBoost;

	// Normalise ao
	aoTotal /= numContributingSamples;
	aoTotal = sqrt(aoTotal);
	aoTotal = max( 0.0, aoTotal );
	aoTotal *= 2.0f;
	aoTotal = min( 1.0, aoTotal );
	aoTotal *= roughness;
	aoTotal = 1.0f - aoTotal;
	bounceTotal *= roughness;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Reflections
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	vec3 randomDirection = randomSample.xyz;
	fragNormal += randomDirection * roughness * u_roughnessJitter;
	fragNormal = normalize(fragNormal);

	vec3 reflectVec = reflect( normalize( fragPos ), fragNormal ); 

	vec2 hitUV;					// Output
	vec3 hitPos;				// Output
	float hitDepthDiff;			// Output

	RayCast(reflectVec, fragPos, hitUV, hitPos, hitDepthDiff);
	vec3 reflectionColor = texture(s_directLightBuffer2D, hitUV ).rgb + texture(s_indirectLightBuffer2D, hitUV ).rgb;
	vec3 reflectionNormal = texture(s_normalBuffer, hitUV ).xyz;

	// Now we have a ray intersection, and we've got our color/normal sample.
	// Time to find all the things wrong with this sample, and modulate it to make its
	// shortcomings less obvious.
	float reflectionAlpha = 1.0f;

	// The RayCast function returns the point with the smallest depth difference it can find.
	// If the smallest depth difference it can find is still rather large, it's likely we're just missing
	// information, and shouldn't be reflecting this pixel.
	float confidence = 1.0f - smoothstep( 0.0f, u_maxReflectDepthDiff, hitDepthDiff );
	//reflectionAlpha *= confidence;

	// The ray will eventually just stop, and we'd like to fade a bit before it does so
	// there's no hard edge past a certain ray distance.
	float distanceStrength = 1.0f - min( length(hitPos - fragPos) / MAX_RAY_LENGTH, 1.0f );
	reflectionAlpha *= distanceStrength;

	// Rays that are pointing towards the camera are suspect, because we have no 'back face'
	// information in the framebuffer. 
	float backFaceFalloff = smoothstep( 0.0f, 1.0f, 1.0f - fragNormal.z );
	reflectionAlpha *= backFaceFalloff;

	// Rays that intersected at a surface pointing away from them also don't
	// make sense to reflect back
	//reflectionAlpha *= max( 0.0f, dot( reflectionNormal, -reflectVec ) );

	// Non-shiny things shouldn't reflect
	reflectionAlpha *= (1.0f-roughness);
	
	out_bounceAndAO = vec4( bounceTotal, aoTotal );
	out_reflections = vec4( reflectionColor, reflectionAlpha );
}
