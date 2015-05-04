#version 450 core

// Inputs
in Varying
{
	vec2 in_uv;
};

uniform sampler2D s_tex;
uniform int u_mipLevel;
uniform float u_aspectRatio;

layout( location = 0 ) out vec4 out_fragColor;

void main()
{
	ivec2 texSize = textureSize( s_tex, 0 );
	float scalar = 1.0f / (texSize.x >> u_mipLevel);
	scalar *= 2.0f;
	
	const int numSamples = 9;
	const float weights[numSamples] = 
	{ 
		0.077847,	0.123317,	0.077847,
		0.123317,	0.195346,	0.123317,
		0.077847,	0.123317,	0.077847
	};
	const vec2 offsets[numSamples] = 
	{ 
		vec2( -1.5, -1.5 ), vec2( 0, -1.5 ), vec2( 1.5, -1.5 ), 
		vec2( -1.5,  0.5 ), vec2( 0.5, 0.5 ), vec2( 1.5,  0.5 ), 
		vec2( -1.5,  1.5 ), vec2( 0,  1.5 ), vec2( 1.5,  1.5 ) 
	};
		
	vec4 outColor = vec4( 0.0f, 0.0f, 0.0f, 1.0f );
	for ( int i = 0; i < numSamples; i++ )
	{
		vec2 offset = offsets[i];
		offset.y *= u_aspectRatio;
		outColor += textureLod( s_tex, in_uv + offset * scalar, u_mipLevel ) * weights[i];
	}

	outColor.a = 1.0f;
	out_fragColor = outColor;
}