#version 450 core

//-----------------------------------------------------------
// Outputs
//-----------------------------------------------------------
layout( location = 0 ) out vec4 out_fragColor;


//-----------------------------------------------------------
// Inputs
//-----------------------------------------------------------
uniform sampler2DRect s_material;
uniform sampler2D s_positionBuffer;
uniform sampler2D s_normalBuffer;
uniform sampler2DRect s_resolveBuffer;
uniform sampler2D s_randomDirectionTexture;

uniform mat4 u_projectionMatrix;
uniform float u_roughnessJitter;
uniform float u_zDistanceMin;

const float rayStep = 0.02f;
const float minRayStep = 0.01f;
const int maxSteps = 64;
const int numBinarySearchSteps = 6;
const float distanceFalloff = rayStep * maxSteps + minRayStep;

in Varying
{
	in vec2 in_uv;
};


//-----------------------------------------------------------
// Functions
//-----------------------------------------------------------
vec3 BinarySearch(vec3 dir, float mipLevel, inout vec3 hitCoord, out float dDepth)
{
    float depth;
 	
    for(int i = 0; i < numBinarySearchSteps; i++)
    {
        vec4 projectedCoord = u_projectionMatrix * vec4(hitCoord, 1.0f);
        projectedCoord.xy /= projectedCoord.w;
        projectedCoord.xy = projectedCoord.xy * 0.5f + 0.5f;
 
        depth = textureLod(s_positionBuffer, projectedCoord.xy, mipLevel).z;
        dDepth = hitCoord.z - depth;
 
        if( dDepth > 0.0f )
            hitCoord += dir;
 
        dir *= 0.5f;
        hitCoord -= dir;  
    }

    vec4 projectedCoord = u_projectionMatrix * vec4(hitCoord, 1.0f);
    projectedCoord.xy /= projectedCoord.w;
    projectedCoord.xy = projectedCoord.xy * 0.5f + 0.5f;
 
    return vec3(projectedCoord.xy, depth);
}

vec3 RayCast(vec3 dir, ivec2 texSize, inout vec3 hitCoord, out float dDepth, vec3 reflectVec )
{
    dir *= rayStep;
    float depth;
    for(int i = 0; i < maxSteps; i++)
    {
    	float mipLevel = 1.0f;//(i / (maxSteps-1)) * 0.0f;

		hitCoord += dir;

		vec4 projectedCoord = u_projectionMatrix * vec4(hitCoord, 1.0);
		projectedCoord.xy /= projectedCoord.w;
		projectedCoord.xy = projectedCoord.xy * 0.5 + 0.5;

		depth = textureLod(s_positionBuffer, projectedCoord.xy, mipLevel).z;
        dDepth = hitCoord.z - depth;
		
		if( dDepth < 0.0f && -dDepth < 0.3f )
		{

			//vec3 normal = texture(s_normalBuffer, projectedCoord.xy, mipLevel ).xyz;
			//float dotProduct = dot( -normal, reflectVec );

			//if ( dotProduct > 0.1f )
			//{
				return BinarySearch(dir, mipLevel, hitCoord, dDepth);
			//}
		}
	}
 
	return vec3(0.0f, 0.0f, 0.0f);
}

void main()
{
	vec4 material = texture( s_material, gl_FragCoord.xy );
	float roughness = material.x;
	float reflectivity = material.y;
	float emissive = material.z;

	ivec2 texSize = textureSize( s_resolveBuffer );

	vec3 fragPos = texture( s_positionBuffer, in_uv ).rgb;
	vec3 fragNormal = texture( s_normalBuffer, in_uv ).rgb;

	vec3 randomDirection = texture( s_randomDirectionTexture, gl_FragCoord.xy / 64.0f ).xyz;
	fragNormal += randomDirection * ( roughness * 0.9f + (1.0f-roughness) * 0.1f ) * u_roughnessJitter;
	fragNormal = normalize(fragNormal);

	vec3 reflectVec = reflect( normalize( fragPos ), fragNormal ); 

	// Screen-space reflections
	vec3 hitPos = fragPos;
	float dDepth;
	vec3 coords = RayCast(reflectVec * max(minRayStep, -fragPos.z), texSize, hitPos, dDepth, reflectVec);
	vec3 reflectionColor = texture(s_resolveBuffer, coords.xy * texSize ).rgb;
	vec3 reflectionNormal = texture(s_normalBuffer, coords.xy ).xyz;

	// Now we have a ray intersection, and we've got our color/normal sample.
	// Time to find all the things wrong with this sample, and modulate it to make its
	// shortcomings less obvious.
	float reflectionAlpha = 1.0f;

	// The ray will eventually just stop, and we'd like to fade a bit before it does so
	// there's no hard edge past a certain ray distance.
	float distanceStrength = 1.0f - min( length(hitPos - fragPos) / distanceFalloff, 1.0f );
	reflectionAlpha *= distanceStrength;

	// Rays that are pointing towards the camera are suspect, because we have no 'back face'
	// information in the framebuffer. 
	float backFaceFalloff = smoothstep( 0.0f, 1.0f, 1.0f - fragNormal.z );
	reflectionAlpha *= backFaceFalloff;

	// Rays that intersected at a surface pointing away from them also don't
	// make sense to reflect back
	reflectionAlpha *= max( 0.0f, dot( reflectionNormal, -reflectVec ) );

	// Non-shiny things shouldn't reflect
	reflectionAlpha *= (1.0f-roughness);

	vec3 outputColor = texture( s_resolveBuffer, gl_FragCoord.xy ).rgb;

	// Alpha blend frame buffer with reflections
	outputColor *= 1.0f - reflectionAlpha;
	reflectionColor *= reflectionAlpha;

	out_fragColor = vec4( outputColor + reflectionColor, 1.0f );
}

/*
uniform sampler2D gColor;
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gEffect;
uniform vec2 gTexSizeInv;
 
const float reflectionSpecularFalloffExponent = 3.0;


*/