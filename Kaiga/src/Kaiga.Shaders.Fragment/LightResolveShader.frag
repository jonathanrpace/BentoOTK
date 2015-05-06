#version 450 core

// Samplers
uniform sampler2DRect s_directLightBuffer;
uniform sampler2DRect s_indirectLightBuffer;
uniform sampler2DRect s_materialBuffer;
//uniform sampler2DRect s_aoBuffer;
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
uniform int u_maxMip;
uniform float u_colorBleedingBoost;
uniform bool u_aoEnabled;
uniform bool u_radiosityEnabled;

// tau[N-1] = optimal number of spiral turns for N samples
const int tauArray[ ] = 
{
	1, 1, 2, 3, 2, 5, 2, 3, 2, 3, 3, 5, 5, 3, 4, 7, 5, 5, 7, 9, 8, 5, 5, 7, 7, 7, 8, 5, 8, 11, 12, 7, 10, 13, 8, 11,
	8, 7, 14, 11, 11, 13, 12, 13, 19, 17, 13, 11, 18, 19, 11, 11, 14, 17, 21, 15, 16, 17, 18, 13, 17, 11, 17, 19,
	18, 25, 18, 19, 19, 29, 21, 19, 27, 31, 29, 21,	18, 17, 29, 31, 31, 23, 18, 25, 26, 25, 23, 19, 34, 19, 27,	21, 
	25, 39, 29, 17, 21, 27
};
// The total screen space radius to applied the effect
//const float radius = 1.0f;
const int numSamples = 64;
const float PI = 3.142f;
// The radius at which we start to drop mip levels.
//const float q = 0.00f;
// The ideal number of 'turns' in spiral sample pattern based in number of samples
const int tau = tauArray[numSamples];	
//const float bias = 0.00f;
const float epsilon = 0.0001f;

// Inputs
in Varying
{
	in vec2 in_uv;
};

// Outputs
layout( location = 0 ) out vec4 out_fragColor;

void AOAndBounce( vec3 fragPos, vec3 fragNormal, vec3 samplePos, vec3 sampleNormal, vec3 sampleColor, out float aoTotal, out vec3 bounceTotal )
{
	vec3 v = samplePos - fragPos;
	float vLength = length(v);

	float falloff = 1.0f - min(1.0, vLength / ( falloffScalar / fragPos.z * fragPos.z));

	float denominator = vLength + epsilon;

	v = normalize(v);

	float vDotFragNormal = max( dot( v, fragNormal ) - bias, 0.0f );
	float vDotSampleNormal = max( dot( v, -sampleNormal ), 0.0f );

	float ao = falloff * max( vDotFragNormal / denominator, 0.0f );

	vec3 bounceColor = sampleColor * falloff * min( 1.0f, ( vDotSampleNormal * vDotFragNormal ) / denominator );

	aoTotal += ao;
	bounceTotal += bounceColor;
}

void main()
{
	vec3 fragPos = texture( s_positionBuffer, in_uv ).xyz;
	vec3 fragNormal = texture( s_normalBuffer, in_uv ).xyz;

	float radiusOverDepth = radius;// / ((fragPos.z * fragPos.z) + 1.0f);

	ivec2 randomTextureSize = textureSize( s_randomTexture, 0 );
	float randomNumber = texture2D( s_randomTexture, (gl_FragCoord.xy / randomTextureSize) ).x;

	float angle = randomNumber * PI * 2.0f;
	float aoTotal = 0.0f;
	vec3 bounceTotal = vec3(0.0f, 0.0f, 0.0f);

	for ( int i = 0; i < numSamples; i++ )
	{
		// The ratio along the spiral
		float theta = (1.0f / numSamples) * (i + 0.5f);
		float currAngle = 2.0f * PI * theta * tau + angle;

		// Distance away from frag UV
		float h = radiusOverDepth * theta;

		// UV offset away from frag UV
		vec2 offset = vec2( cos( currAngle ), sin( currAngle ) );
		offset.y *= u_aspectRatio;

		vec2 sampleUV = in_uv + offset * h;
		float mipLevel = min( h * q * radius, u_maxMip );

		vec3 samplePosition = textureLod( s_positionBuffer, sampleUV, mipLevel ).xyz;

		if ( abs(samplePosition.x) < 0.0001f )
		{
			continue;
		}

		vec3 sampleNormal =  textureLod( s_normalBuffer, sampleUV, mipLevel ).xyz;
		vec3 sampleDirectColor = textureLod( s_directLightBuffer2D, sampleUV, mipLevel ).xyz;
		vec3 sampleIndirectColor = textureLod( s_indirectLightBuffer2D, sampleUV, mipLevel ).xyz;
		vec3 sampleColor = sampleDirectColor + sampleIndirectColor;

		AOAndBounce( fragPos, fragNormal, samplePosition, sampleNormal, sampleColor, aoTotal, bounceTotal );
	}

	bounceTotal /= numSamples;

	float maxBounceChannel = max( max( bounceTotal.r, bounceTotal.g ), bounceTotal.b );
	float minBounceChannel = min( min( bounceTotal.r, bounceTotal.g ), bounceTotal.b );
	float colorBoost = max( 0.0f, (maxBounceChannel - minBounceChannel) / maxBounceChannel );
	colorBoost = mix( 1.0f, colorBoost, u_colorBleedingBoost );
	bounceTotal *= u_radiosityScalar * colorBoost;

	aoTotal /= numSamples;
	//aoTotal *= 2.0f;
	aoTotal = max( 0.0f, 1.0f - sqrt(aoTotal) );


	
	vec3 directLight = texture( s_directLightBuffer, gl_FragCoord.xy ).xyz;
	vec3 indirectLight = texture( s_indirectLightBuffer, gl_FragCoord.xy ).xyz;
	vec4 material = texture( s_materialBuffer, gl_FragCoord.xy );
	float roughness = material.x;
	float reflectivity = material.y;
	//float emissive = 0.0f;//material.z;

	bounceTotal *= roughness;
	bounceTotal *= reflectivity;

	if ( u_aoEnabled == false )
	{
		aoTotal = 1.0;
	}

	if ( u_radiosityEnabled == false )
	{
		bounceTotal *= 0.0f;
	}

	aoTotal += (1.0f-aoTotal) * (1.0f-roughness);

	//out_fragColor = vec4( aoTotal, aoTotal, aoTotal, 1.0f );
	//out_fragColor = vec4( bounceTotal, 1.0f );
	//out_fragColor = vec4( fragPos, 1.0f );

	vec3 outColor = directLight + (indirectLight * aoTotal) + bounceTotal;
	//vec3 outColor = vec3( roughness );

	out_fragColor = vec4( outColor, 1.0f );
}
