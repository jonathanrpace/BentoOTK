// Samplers
uniform sampler2DRect s_positionBuffer;
uniform sampler2DRect s_normalBuffer;
uniform sampler2DRect s_materialBuffer;
uniform sampler2DRect s_albedoBuffer;

// Scalars
uniform float u_intensity;
uniform vec3 u_color;
uniform vec3 u_lightDirection;

// Outputs
layout( location = 0 ) out vec4 out_color;

void main()
{
	vec3 position 	= texture2DRect( s_positionBuffer, 	gl_FragCoord.xy ).xyz;
	vec3 normal 	= texture2DRect( s_normalBuffer, 	gl_FragCoord.xy ).xyz;
	vec4 material 	= texture2DRect( s_materialBuffer, 	gl_FragCoord.xy );
	vec3 albedo 	= texture2DRect( s_albedoBuffer, 	gl_FragCoord.xy ).xyz;

	vec3 viewDir = normalize( -position );

	float roughness = material.x;
	float reflectivity = material.y;
	float emissive = material.z;
	
	float lightAmount = LightingFuncGGX( normal, viewDir, u_lightDirection, roughness, reflectivity );
	vec3 light = vec3( lightAmount );
	
	light *= u_color;
	light *= u_intensity;
	light *= albedo;

	out_color = vec4( light, 1.0 );
}