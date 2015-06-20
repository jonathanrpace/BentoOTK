#version 450 core

//-----------------------------------------------------------
// Outputs
//-----------------------------------------------------------
layout( location = 0 ) out vec4 out_fragColor;


//-----------------------------------------------------------
// Inputs
//-----------------------------------------------------------
uniform sampler2DRect s_directLight;
uniform sampler2DRect s_indirectLight;
uniform sampler2DRect s_lightTransport;
uniform sampler2DRect s_reflections;

uniform float u_lightTransportResScalar;

in Varying
{
	in vec2 in_uv;
};


//-----------------------------------------------------------
// Functions
//-----------------------------------------------------------
void main()
{
	vec3 directLight = texture( s_directLight, gl_FragCoord.xy ).xyz;
	vec3 indirectLight = texture( s_indirectLight, gl_FragCoord.xy ).xyz;
	vec4 lightTransport = texture( s_lightTransport, gl_FragCoord.xy * u_lightTransportResScalar );
	vec4 reflections = texture( s_reflections, gl_FragCoord.xy * u_lightTransportResScalar );

	vec3 bounceLight = lightTransport.rgb;
	float ao = lightTransport.a;

	vec3 outColor = directLight + indirectLight * ao + bounceLight;

	outColor = mix( outColor, reflections.rgb, reflections.a );

	out_fragColor = vec4( outColor, 1.0f );
}