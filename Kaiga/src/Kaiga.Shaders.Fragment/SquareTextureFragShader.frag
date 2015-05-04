#version 450 core

// Samplers
uniform sampler2D tex;

uniform float u_mipRatio;

// Inputs
in Varying
{
	vec2 in_uv;
};

// Outputs
layout( location = 0 ) out vec4 out_fragColor;

void main(void)
{ 
	out_fragColor = textureLod( tex, in_uv, u_mipRatio );
}