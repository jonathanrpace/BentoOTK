#version 450 core

// Samplers
uniform sampler2D tex;

// Inputs
in Varying
{
	vec2 in_uv;
};

// Outputs
layout( location = 0 ) out vec4 out_fragColor;

void main(void)
{ 
	out_fragColor = textureLod( tex, in_uv, 3 );
}