#version 450 core

// Samplers
uniform sampler2D s_positionBuffer;
uniform sampler2D s_normalBuffer;
uniform sampler2D s_directLightBuffer2D;
uniform sampler2D s_indirectLightBuffer2D;
uniform sampler2D s_randomTexture;

uniform float radius;
uniform float bias;
uniform float falloffScalar;
uniform float q;
uniform float u_aspectRatio;
uniform float u_radiosityScalar;
uniform float u_colorBleedingBoost;
uniform float epsilon;
uniform int u_maxMip;
uniform bool u_aoEnabled;
uniform bool u_radiosityEnabled;

// TAU_LOOKUP[N-1] = optimal number of spiral turns for N samples
const int TAU_LOOKUP[ ] = 
{
	1, 1, 2, 3, 2, 5, 2, 3, 2, 3, 3, 5, 5, 3, 4, 7, 5, 5, 7, 9, 8, 5, 5, 7, 7, 7, 8, 5, 8, 11, 12, 7, 10, 13, 8, 11,
	8, 7, 14, 11, 11, 13, 12, 13, 19, 17, 13, 11, 18, 19, 11, 11, 14, 17, 21, 15, 16, 17, 18, 13, 17, 11, 17, 19,
	18, 25, 18, 19, 19, 29, 21, 19, 27, 31, 29, 21,	18, 17, 29, 31, 31, 23, 18, 25, 26, 25, 23, 19, 34, 19, 27,	21, 
	25, 39, 29, 17, 21, 27
};
const int NUM_SAMPLES = 32;
const float NUM_SAMPLES_RCP = 1.0f / NUM_SAMPLES;
const int TAU = TAU_LOOKUP[NUM_SAMPLES];
const float PI = 3.142f;


// Inputs
in Varying
{
	in vec2 in_uv;
};

// Outputs
layout( location = 0 ) out vec4 out_fragColor;

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

	float falloff = 1.0f - min(1.0f, vLength / falloffScalar );
	
	float vDotFragNormal = max( dot( v, fragNormal ) - bias, 0.0f );

	float denominator = vLength + epsilon;
	aoTotal += falloff * min( 1.0f, ( vDotFragNormal / denominator ) );
	bounceTotal += sampleColor * falloff * min( 1.0f, ( vDotFragNormal  ) / denominator );
}

void main()
{
	vec3 fragPos = texture( s_positionBuffer, in_uv ).xyz;
	vec3 fragNormal = texture( s_normalBuffer, in_uv ).xyz;
	
	ivec2 randomTextureSize = textureSize( s_randomTexture, 0 );
	float randomNumber = texture2D( s_randomTexture, (gl_FragCoord.xy / randomTextureSize) ).x;

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
		float h = radius * theta;

		// UV offset away from frag UV
		vec2 offset = vec2( cos( currAngle ), sin( currAngle ) );
		offset.y *= u_aspectRatio;
		vec2 sampleUV = in_uv + offset * h;

		// Discard samples that lie outside the viewport.
		if ( sampleUV.x < 0.0f || sampleUV.y < 0.0f || sampleUV.x > 1.0f || sampleUV.y > 1.0f )
		{
			continue;
		}

		float mipLevel = min( log2( h / q ), u_maxMip );

		vec3 samplePosition = textureLod( s_positionBuffer, sampleUV, mipLevel ).xyz;

		if ( abs(samplePosition.x) < 0.0001f )
		{
			continue;
		}

		vec3 sampleDirectColor = textureLod( s_directLightBuffer2D, sampleUV, mipLevel ).xyz;
		vec3 sampleIndirectColor = textureLod( s_indirectLightBuffer2D, sampleUV, mipLevel ).xyz;
		vec3 sampleColor = sampleDirectColor + sampleIndirectColor;

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
	aoTotal *= 6.0f;
	aoTotal = max( 0.0f, 1.0f - sqrt(aoTotal) );

	//aoTotal = u_aoEnabled ? aoTotal : 1.0f;
	//bounceTotal *= u_radiosityEnabled ? 1.0f : 0.0f;

	vec3 outColor = bounceTotal * aoTotal;

	out_fragColor = vec4( bounceTotal, aoTotal );
}
