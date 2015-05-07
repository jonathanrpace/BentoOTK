#version 450 core

// Samplers
uniform sampler2DRect s_directLight;
uniform sampler2DRect s_indirectLight;
uniform sampler2DRect s_material;
uniform sampler2DRect s_lightTransport;

uniform float u_lightTransportResScalar;

// Inputs
in Varying
{
	in vec2 in_uv;
};

// Outputs
layout( location = 0 ) out vec4 out_fragColor;


void main()
{
	vec3 directLight = texture( s_directLight, gl_FragCoord.xy ).xyz;
	vec3 indirectLight = texture( s_indirectLight, gl_FragCoord.xy ).xyz;
	vec4 material = texture( s_material, gl_FragCoord.xy );
	float roughness = material.x;
	float reflectivity = material.y;
	float emissive = material.z;

	vec4 lightTransport = texture( s_lightTransport, gl_FragCoord.xy * u_lightTransportResScalar );
	vec3 bounceLight = lightTransport.rgb;
	float ao = lightTransport.a;

	vec3 outColor = directLight + indirectLight * ao + bounceLight;

	out_fragColor = vec4( outColor, 1.0f );
}