#version 140

mat4 MVPMatrix;


layout(location = 0) in vec3 in_position;

void main(void)
{
	gl_Position = MVPMatrix * vec4(in_position, 1);
} 