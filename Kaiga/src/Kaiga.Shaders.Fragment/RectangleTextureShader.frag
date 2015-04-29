#version 450 core

// Samplers
uniform sampler2DRect tex;

// Inputs
in Varying
{
	vec2 in_uv;
};

// Outputs
layout( location = 0 ) out vec4 out_fragColor;

void main(void)
{ 
	ivec2 texSize = textureSize( tex, 0 );
	vec2 uv = in_uv;
	uv.y = 1.0 - uv.y;
	uv *= texSize;
	out_fragColor = texture2DRect( tex, uv );
}