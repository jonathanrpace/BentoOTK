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
const int NUM_SAMPLES = 12;
const int TAU = TAU_LOOKUP[NUM_SAMPLES];
const float NUM_SAMPLES_RCP = 1.0f / NUM_SAMPLES;
const float PI = 3.142f;
const float Q = 0.25f;			// The screen space radius as which we begin dropping mips
uniform float BIAS = 0.06;		// The offset applied to minimise self-occlusion.
const float EPSILON = 0.003f;	// A small offset to avoid divide by zero

const int MAX_REFLECTION_STEPS = 32;
const int NUM_BINARY_SERACH_STEPS = 0;
uniform float RAY_STEP_SCALAR = 1.2f;
uniform float RAY_STEP = 0.02f;
const float MAX_RAY_LENGTH = RAY_STEP * MAX_REFLECTION_STEPS;
const int NUM_REFLECTION_SAMPLES = 4;

uniform sampler2D s_positionBuffer;
uniform sampler2D s_prevPositionBuffer;
uniform sampler2D s_normalBuffer;
uniform sampler2D s_directLightBuffer2D;
uniform sampler2D s_indirectLightBuffer2D;
uniform sampler2D s_randomTexture;
uniform sampler2DRect s_material;
uniform sampler2DRect s_albedo;
uniform sampler2DRect s_prevBounceAndAo;
uniform sampler2DRect s_prevReflection;

uniform int u_maxMip;

uniform mat4 u_projectionMatrix;
uniform mat4 u_invViewMatrix;
uniform mat4 u_prevViewProjectionMatrix;
uniform mat4 u_prevInvViewProjectionMatrix;

uniform float u_roughnessJitterMin = 0.001f;
uniform float u_roughnessJitterMax = 0.2f;
uniform float u_radius = 0.5f;
uniform float u_aoAndBounceRange = 1.0f;
uniform float u_aspectRatio;
uniform float u_radiosityScalar = 100.0f;
uniform float u_aoAttenutationScale = 4.0f;
uniform float u_aoAttenutationPower = 1.5f;
uniform float u_colorBleedingBoost = 0.0f;
uniform float u_lightTransportResolutionScalar;
uniform float u_maxReflectDepthDiff;
uniform float u_nominalReflectDepthDiff = 0.1f;
uniform float u_time;
uniform float SAMPLE_MIX_RATE = 0.15f;
uniform float SAMPLE_MIX_RATE_REFLECTIONS = 0.3f;
uniform bool u_flag = true;

// Inputs
in Varying
{
	in vec2 in_uv;
};

//-----------------------------------------------------------
// Functions
//-----------------------------------------------------------

vec4 RayCast
(
	vec3 dir, 
	vec3 startPos, 
	out vec2 bestHitUV, 
	out vec3 bestPosition
)
{
/*
	float cameraToWorldDist = length(startPos);

    vec3 newPos;
    vec4 newScreen;
    float i = 0;
    vec3 rayTrace = startPos;
    float currentWorldDist, rayDist;
    float incr = 0.02;
    do 
    {
        i += 0.05;
        rayTrace += dir*incr;
        //incr *= 1.01;
        newScreen = u_projectionMatrix * vec4(rayTrace, 1);
        newScreen.xyz/= newScreen.w;
        newPos = texture(s_positionBuffer, newScreen.xy/2.0+0.5).xyz;
        currentWorldDist = length(newPos.xyz);
        rayDist = length(rayTrace.xyz);
        if (newScreen.x > 1 || newScreen.x < -1 || newScreen.y > 1 || newScreen.y < -1 //|| newScreen.z > 1 || newScreen.z < -1 
        	|| i >= 1.0 || cameraToWorldDist > currentWorldDist) 
        {
        	break;
        }
    } 
    while(rayDist < currentWorldDist);
 
    if (cameraToWorldDist > currentWorldDist)
       return vec4(0,0,0,1); // Yellow indicates we found a pixel hidden behind another object
    else if (newScreen.x > 1 || newScreen.x < -1 || newScreen.y > 1 || newScreen.y < -1)
       return vec4(0,0,0,1); // Black used for outside of screen
    else if (newScreen.z > 1 && newScreen.z < -1)
        return vec4(0,0,0,1); // Red outside of frustum

	bestHitUV = newScreen.xy/2.0+0.5;
	return vec4(1,1,1,1); // White, success;

	*/


	vec4 pointAlongRay = vec4(startPos,1.0f);
    vec4 projectedCoord;
    vec4 samplePos;
    float rayStep = RAY_STEP;// * (1.0f+abs(dir.z));

    for(int i = 0; i < MAX_REFLECTION_STEPS; i++)
    {
    	vec3 offset = dir * rayStep;
    	//rayStep *= RAY_STEP_SCALAR;
		
		pointAlongRay.xyz += offset;
		projectedCoord = u_projectionMatrix * pointAlongRay;
		projectedCoord.xyz /= projectedCoord.w;

		if ( 	   projectedCoord.x < -1 
				|| projectedCoord.y < -1 
				|| projectedCoord.z < 0 
				|| projectedCoord.x > 1 
				|| projectedCoord.y > 1
				|| projectedCoord.z > 1 )

		{
			return vec4(0.0f);
		}

		projectedCoord.xy = projectedCoord.xy * 0.5 + 0.5;
		samplePos = texture(s_positionBuffer, projectedCoord.xy );
		float depthDiff = length( pointAlongRay.xyz ) - length( samplePos.xyz );

		if ( depthDiff > 0.0f && depthDiff < u_nominalReflectDepthDiff )
		{
			bestHitUV = projectedCoord.xy;
			bestPosition = pointAlongRay.xyz;

			return vec4(1.0f);
			/*
			for(int j = 0; j < NUM_BINARY_SERACH_STEPS; j++)
		    {
		    	pointAlongRay.xyz += offset;
				projectedCoord = u_projectionMatrix * pointAlongRay;
				projectedCoord.xy /= projectedCoord.w;
				projectedCoord.xy = projectedCoord.xy * 0.5 + 0.5;
				samplePos = texture(s_positionBuffer, projectedCoord.xy );
				depthDiff = length( pointAlongRay.xyz ) - length( samplePos.xyz );

		        if ( depthDiff > 0.0f && depthDiff < u_nominalReflectDepthDiff )
		        {
		        	if ( depthDiff < smallestDepthDiff  )
		        	{
		        		smallestDepthDiff = depthDiff;
						bestHitUV = projectedCoord.xy;
						bestPosition = pointAlongRay.xyz;
		        	}

		        	if ( forward )
		        	{
		        		offset *= -0.5f;
						forward = !forward;
		        	}
		        }
		        else if ( !forward )
		        {
		        	offset *= -0.5f;
					forward = !forward;
		        }
		    }
		    */

			//return success;
		}
	}

	return vec4(0.0f);

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
	vec3 fragPos = textureLod( s_positionBuffer, in_uv, 0 ).xyz;
	vec3 fragNormal = normalize( textureLod( s_normalBuffer, in_uv, 0 ).xyz );
	ivec2 randomTextureSize = textureSize( s_randomTexture, 0 );
	vec4 randomSample = texture2D( s_randomTexture, (gl_FragCoord.xy / randomTextureSize) );
	float randomNumber = randomSample.w;
	vec3 viewDir = normalize( -fragPos );
	vec3 albedo = texture( s_albedo, gl_FragCoord.xy / u_lightTransportResolutionScalar ).rgb;
	float dotNV = clamp( dot( viewDir, fragNormal ), 0.0f, 1.0f );
	//float bias = BIAS * -fragPos.z;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// AO and Bounce
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	float angle = (randomNumber + u_time) * PI * 2.0f;
	float aoTotal = 0.0f;
	vec3 bounceTotal = vec3(0.0f, 0.0f, 0.0f);
	int numContributingAOSamples = 0;
	int numContributingBounceSamples = 0;
	vec3 prominantBounceDirection = vec3(0.0f);
	vec3 prominantAODirection = vec3(0.0f);
	for ( int i = 0; i < NUM_SAMPLES; i++ )
	{
		// The ratio along the spiral
		float theta = NUM_SAMPLES_RCP * (i + 0.5f);
		float currAngle = 2.0f * PI * theta * TAU + angle;

		// Distance away from frag UV
		float h = u_radius * theta;
		h /= length(fragPos);
		
		// UV offset away from frag UV
		vec2 offset = vec2( cos( currAngle ), sin( currAngle ) * u_aspectRatio );
		vec2 sampleUV = in_uv + offset * h;

		float mipLevel = min( log2( h / Q ), u_maxMip );
		vec3 samplePosition = textureLod( s_positionBuffer, sampleUV, mipLevel ).xyz;
		vec3 sampleDirectColor = textureLod( s_directLightBuffer2D, sampleUV, mipLevel ).xyz;
		vec3 sampleIndirectColor = textureLod( s_indirectLightBuffer2D, sampleUV, mipLevel ).xyz;
		vec3 sampleBounce = texture( s_prevBounceAndAo, sampleUV * textureSize( s_prevBounceAndAo ).xy ).rgb;
		vec3 sampleMaterial = texture( s_material, sampleUV * materialTextureSize.xy ).xyz;
		float sampleRoughness = sampleMaterial.x;
		float sampleEmissive = sampleMaterial.z;
		vec3 sampleColor = sampleDirectColor + sampleIndirectColor + (sampleIndirectColor * sampleEmissive * 5.0f) + sampleBounce * 0.2f;

		// Calculate distance and direction between fragment and sample pos
		vec3 v = samplePosition - fragPos;
		float vLength = length(v);
		v = normalize(v);

		// Determine common value for AO and bounce
		float rangeScalar = min( 1.0f, max( 0.0f, (u_aoAndBounceRange - vLength) ) / u_aoAndBounceRange );

		float aoAttenuation = 1.0f / (1.0f + pow( vLength, u_aoAttenutationPower ) * u_aoAttenutationScale);

		float vDotFragNormal = clamp( dot( v, fragNormal ), 0.0f, 1.0f );
		float vDotFragNormalBiased = clamp( dot( v, fragNormal ) - BIAS, 0.0f, 1.0f );

		// Sum AO
		//float aoAmount = pow( rangeScalar,1.0f ) * vDotFragNormalBiased;
		float aoAmount = aoAttenuation * vDotFragNormalBiased;
		aoTotal += vLength * 0.1f;//aoAmount;
		numContributingAOSamples++;

		// Sum Bounce
		float vDotFragNormal2 = vDotFragNormal;//(dot( v, fragNormal ) + 1.0f) * 0.5f;
		vec3 bounceAmount = sampleColor * vDotFragNormal2 * rangeScalar * albedo;

		// Fade off bounce light from shiny surfaces, as their color is view dependant.
		//bounceAmount *= sampleRoughness;

		bounceTotal += max(vec3(0.0f), bounceAmount);
		float maxBounce = max( max( bounceAmount.r, bounceAmount.g ), bounceAmount.b );
		prominantBounceDirection -= v * maxBounce;
		prominantAODirection -= v * max( 0.0f, (aoAmount - min(1.0f,maxBounce)));

		numContributingBounceSamples++;
	}

	float prominantBounceStrength = min( 1.0f, length(prominantBounceDirection) / (NUM_SAMPLES * 0.25f) );
	float prominantAOStrength = min( 1.0f, length(prominantAODirection) / (NUM_SAMPLES * 0.25f) );

	// Normalise ao and bounce
	aoTotal /= NUM_SAMPLES;
	//aoTotal *= roughness;
	aoTotal = min( 1.0f, aoTotal * 4.0f );

	if ( numContributingBounceSamples > 0 )
	{
		bounceTotal /= numContributingBounceSamples;

		float maxBounceChannel = max( max( bounceTotal.r, bounceTotal.g ), bounceTotal.b );
		float minBounceChannel = min( min( bounceTotal.r, bounceTotal.g ), bounceTotal.b );
		float colorBoost = max( 0.0f, (maxBounceChannel - minBounceChannel) / maxBounceChannel );
		colorBoost = mix( 1.0f, colorBoost, u_colorBleedingBoost );
		bounceTotal *= u_radiosityScalar * colorBoost;

		// Apply fresnel to bounce
		float F = Fresnel( dotNV, reflectivity );
		bounceTotal *= F;

		bounceTotal *= roughness;
		bounceTotal *= albedo;

		// Modulate bounce light by ao
		prominantAODirection = normalize(prominantAODirection);
		float prominantDot = clamp( dot( prominantAODirection, prominantBounceDirection ), 0.0f, 1.0f );
		bounceTotal -= (prominantDot * aoTotal * prominantAOStrength * prominantBounceStrength);
		bounceTotal = max( vec3(0.0f), bounceTotal );
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Reflections
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	vec3 reflectionColor = vec3(0.0f,0.0f,0.0f);
	float reflectionAlpha = 0.0f;
	vec3 reflectionFragNormal = fragNormal;

	for ( int i = 0; i < NUM_REFLECTION_SAMPLES; i++ )
	{
		vec2 randomUV = (gl_FragCoord.xy / randomTextureSize) + randomSample.xy + randomSample.xy * 0.321f * u_time;
		randomUV += u_time;
		//float offset = u_time * randomTextureSize.x*randomTextureSize.y;
		//randomUV.x += mod( offset, 

		
		vec3 randomSampleReflect = texture2D( s_randomTexture, randomUV  ).xyz;
		vec3 randomDirection = randomSampleReflect.xyz;
		reflectionFragNormal += randomDirection * (u_roughnessJitterMin + pow(roughness,3.0f) * u_roughnessJitterMax);
		reflectionFragNormal = normalize(reflectionFragNormal);

		vec3 reflectVec = reflect( normalize( fragPos ), reflectionFragNormal ); 

		vec2 hitUV;					// Output
		vec3 hitPos;				// Output

		vec4 success = RayCast(reflectVec, fragPos, hitUV, hitPos);

		reflectionColor += texture(s_directLightBuffer2D, hitUV).rgb;
		reflectionColor += texture(s_indirectLightBuffer2D, hitUV).rgb;

		// Now we have a ray intersection, and we've got our color/normal sample.
		// Time to find all the things wrong with this sample, and modulate it to make its
		// shortcomings less obvious.
		float reflectionAlphaCurrent = success.x;

		// The ray will eventually just stop, and we'd like to fade a bit before it does so
		// there's no hard edge past a certain ray distance.
		float distanceStrength = 1.0f - min( length(hitPos.xyz - fragPos.xyz) / MAX_RAY_LENGTH, 1.0f );
		reflectionAlphaCurrent *= distanceStrength;

		// Reflection rays pointing towards the camere are suspect, as we have no 'back face' information
		float backfaceFalloff = clamp( dot( vec3(0.0f,0.0f,1.0f), -reflectVec ), 0.0f, 1.0f );
		reflectionAlphaCurrent *= backfaceFalloff;

		// Multiply by fresnel
		vec3 halfVector = normalize(viewDir + reflectVec);
		float dotLH = clamp(dot(reflectVec,halfVector), 0.0f, 1.0f);
		float F = Fresnel( dotNV, reflectivity );
		reflectionAlphaCurrent *= F;

		reflectionAlpha += reflectionAlphaCurrent;
	}
	reflectionColor /= NUM_REFLECTION_SAMPLES;
	reflectionAlpha /= NUM_REFLECTION_SAMPLES;

	// Non-shiny things shouldn't reflect
	reflectionAlpha *= (1.0f-roughness);
	reflectionColor *= albedo;


	// Retreive samples from the previous frame via reprojection
	vec4 fragWorldPos = u_invViewMatrix * vec4(fragPos,1.0);
	vec4 prevFragProjectedPos = u_prevViewProjectionMatrix * fragWorldPos;
	prevFragProjectedPos.xyz /= prevFragProjectedPos.w;
	vec2 prevUV = (prevFragProjectedPos.xy + 1.0) * 0.5;
	vec4 prevFragWorldPos = u_invViewMatrix * vec4( texture( s_prevPositionBuffer, prevUV ).xyz, 1.0f );
	float dis = length(prevFragWorldPos.xyz-fragWorldPos.xyz);
	if ( dis < 0.1f )
	{
		ivec2 bounceAndAOTextureSize = textureSize( s_prevBounceAndAo, 0 );
		vec4 prevBounceAndAO = texture( s_prevBounceAndAo, prevUV * bounceAndAOTextureSize.xy );

		float prevAOTotal= 1.0f - prevBounceAndAO.w;
		aoTotal = mix( prevAOTotal, aoTotal, SAMPLE_MIX_RATE );

		vec3 prevBounceTotal = prevBounceAndAO.rgb;
		bounceTotal = mix( prevBounceTotal, bounceTotal, vec3(SAMPLE_MIX_RATE) );

		vec4 prevReflectionColorAndAlpha = texture( s_prevReflection, prevUV * bounceAndAOTextureSize.xy );
		reflectionColor = mix( prevReflectionColorAndAlpha.rgb, reflectionColor, vec3(SAMPLE_MIX_RATE_REFLECTIONS) );
		reflectionAlpha = mix( prevReflectionColorAndAlpha.a, reflectionAlpha, SAMPLE_MIX_RATE_REFLECTIONS );
	}

	aoTotal = 1.0f - aoTotal;

	out_bounceAndAO = vec4( bounceTotal, aoTotal );
	out_reflections = vec4( reflectionColor, reflectionAlpha );
}
