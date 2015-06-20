#version 440 core

// Uniforms
uniform mat4 u_mvpMat;

// Inputs
layout(location = 0) in vec3 in_position;

// Outputs
out gl_PerVertex 
{
	vec4 gl_Position;
};

void main(void)
{
	gl_Position = u_mvpMat * vec4(in_position, 1);
} 