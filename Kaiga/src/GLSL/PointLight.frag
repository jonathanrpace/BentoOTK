// Samplers
uniform sampler2DRect s_positionBuffer;
uniform sampler2DRect s_normalBuffer;
uniform sampler2DRect s_materialBuffer;
uniform sampler2DRect s_albedoBuffer;

// Scalars
uniform float u_attenuationRadius;
uniform float u_attenuationPower;
uniform float u_radius;
uniform float u_intensity;
uniform vec3 u_color;
uniform vec3 u_lightPosition;

// Outputs
layout( location = 0 ) out vec4 out_color;

void main()
{
	vec3 position 	= texture2DRect( s_positionBuffer, 	gl_FragCoord.xy ).xyz;
	vec3 normal 	= texture2DRect( s_normalBuffer, 	gl_FragCoord.xy ).xyz;
	vec4 material 	= texture2DRect( s_materialBuffer, 	gl_FragCoord.xy );
	vec3 albedo 	= texture2DRect( s_albedoBuffer, 	gl_FragCoord.xy ).xyz;

	vec3 lightDir = u_lightPosition - position.xyz;
	float distance = max( length( lightDir ) - u_radius, 0.0f );
	lightDir = normalize( lightDir );

	float angularSize = clamp( atan( u_radius / distance ) * 2.0f / 3.142f , 0.0f, 1.0f );
	angularSize = 1.0f - angularSize;
	
	vec3 viewDir = normalize( -position );

	float roughness = material.x;
	float reflectivity = material.y;
	float emissive = material.z;

	float attenuation = 1.0f - min( distance / u_attenuationRadius, 1.0f );
	attenuation = pow( attenuation, u_attenuationPower );
	
	float lightAmount = LightingFuncGGXAngular( normal, viewDir, lightDir, roughness, reflectivity, angularSize );
	vec3 light = vec3( lightAmount );
	
	light *= u_color;
	light *= u_intensity;
	light *= attenuation;
	light *= albedo;

	out_color = vec4( light, 1.0 );
}