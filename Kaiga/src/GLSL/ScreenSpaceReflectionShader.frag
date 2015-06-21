#version 450 core

//-----------------------------------------------------------
// Outputs
//-----------------------------------------------------------
layout( location = 0 ) out vec4 out_fragColor;

const int MAX_REFLECTION_STEPS = 128;
const int NUM_BINARY_SERACH_STEPS = 6;
const float rayStep = 0.01f;
const float MAX_RAY_LENGTH = rayStep * MAX_REFLECTION_STEPS;

uniform sampler2D s_positionBuffer;
uniform sampler2D s_normalBuffer;
uniform sampler2D s_directLightBuffer2D;
uniform sampler2D s_indirectLightBuffer2D;
uniform sampler2D s_randomTexture;
uniform sampler2DRect s_material;

uniform mat4 u_projectionMatrix;
uniform float u_roughnessJitter;
uniform float u_lightTransportResolutionScalar;
uniform float u_maxReflectDepthDiff;

in Varying
{
	in vec2 in_uv;
};


//-----------------------------------------------------------
// Functions
//-----------------------------------------------------------

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
	reflectionAlpha *= confidence;

	// The ray will eventually just stop, and we'd like to fade a bit before it does so
	// there's no hard edge past a certain ray distance.
	float distanceStrength = 1.0f - min( length(hitPos - fragPos) / MAX_RAY_LENGTH, 1.0f );
	//reflectionAlpha *= distanceStrength;

	// Rays that are pointing towards the camera are suspect, because we have no 'back face'
	// information in the framebuffer. 
	float backFaceFalloff = smoothstep( 0.0f, 1.0f, 1.0f - fragNormal.z );
	reflectionAlpha *= backFaceFalloff;

	// Rays that intersected at a surface pointing away from them also don't
	// make sense to reflect back
	//reflectionAlpha *= max( 0.0f, dot( reflectionNormal, -reflectVec ) );

	// Non-shiny things shouldn't reflect
	reflectionAlpha *= (1.0f-roughness);

	reflectionColor *= reflectionAlpha;

	gl_FragColor = reflectionColor;
}