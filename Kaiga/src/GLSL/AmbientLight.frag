// Samplers
uniform sampler2DRect s_materialBuffer;
uniform sampler2DRect s_albedoBuffer;
uniform sampler2DRect s_positionBuffer;
uniform sampler2DRect s_normalBuffer;

// Scalars
uniform vec3 u_color;
uniform float u_intensity;

// Outputs
layout( location = 0 ) out vec4 out_color;

void main()
{
	vec4 material 	= texture( s_materialBuffer, 	gl_FragCoord.xy );
	vec3 albedo 	= texture( s_albedoBuffer, 		gl_FragCoord.xy ).xyz;
	vec3 fragPos 	= texture( s_positionBuffer, 	gl_FragCoord.xy ).xyz;
	vec3 fragNormal = texture( s_normalBuffer, 		gl_FragCoord.xy ).xyz;

	vec3 viewDir = normalize( -fragPos );
	float dotNV = clamp( dot( viewDir, fragNormal ), 0.0f, 1.0f );

	float roughness = material.x;
	float reflectivity = material.y;

	float F = Fresnel( dotNV, reflectivity );

	vec3 light = pow( u_color * u_intensity, vec3( 2.2 ) );
	light *= F;
	light *= albedo;

	out_color = vec4( light, 1.0 );
}