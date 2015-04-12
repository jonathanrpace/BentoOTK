using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Lights;
using OpenTK;

namespace Kaiga.ShaderStages
{
	public class AmbientLightFragShader : AbstractFragmentShaderStage
	{
		override public void BindPerPass( RenderParams renderParams )
		{
			SetUniformTexture( 0, "s_positionBuffer", renderParams.RenderTarget.PositionBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( 1, "s_normalBuffer", renderParams.RenderTarget.NormalBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( 2, "s_materialBuffer", renderParams.RenderTarget.MaterialBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( 3, "s_albedoBuffer", renderParams.RenderTarget.AlbedoBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( 4, "s_aoBuffer", renderParams.AORenderTarget.AOBuffer.Texture, TextureTarget.TextureRectangle );
		}

		public void BindPerLight( RenderParams renderParams, AmbientLight light )
		{
			SetUniform3( "u_color", light.Color );
			SetUniform1( "u_intensity", light.Intensity );
		}

		override protected string GetShaderSource()
		{
			return @"
#version 440 core

// Samplers
uniform sampler2DRect s_positionBuffer;
uniform sampler2DRect s_normalBuffer;
uniform sampler2DRect s_materialBuffer;
uniform sampler2DRect s_albedoBuffer;
uniform sampler2DRect s_aoBuffer;

// Scalars
uniform vec3 u_color;
uniform float u_intensity;

// Outputs
layout( location = 0 ) out vec4 out_color;

float G1V(float dotNV, float k)
{
	return 1.0f/(dotNV*(1.0f-k)+k);
}

float angularDot( vec3 A, vec3 B, in float angularSize )
{
	float dotValue = clamp( dot( A, B ), 0.0f, 1.0f );
	float pi = 3.14159f;

	float angle = 1.0f - acos(dotValue) * (2.0f / pi);
	angle = min( angle, angularSize );
	angle *= rcp( angularSize );

	float ret = sin( angle * pi * 0.5f );
	
	return ret;
}

float LightingFuncGGX_REF( vec3 N, vec3 V, vec3 L, float roughness, float F0, float angularSize )
{
	float alpha = roughness*roughness;

	vec3 H = normalize(V+L);

	float dotNL = angularDot(N,L,angularSize);
	float dotNV = clamp(dot(N,V), 0.0f, 1.0f);
	float dotNH = angularDot(N,H,angularSize);
	float dotLH = angularDot(L,H,angularSize);

	// D
	float alphaSqr = alpha*alpha;
	float pi = 3.14159f;
	float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0f;
	float D = alphaSqr/(pi * denom * denom);

	// F
	float dotLH5 = pow(1.0f-dotLH,5);
	float F = F0 + (1.0-F0)*(dotLH5);

	// V
	float k = alpha/2.0f;
	float vis = G1V(dotNL,k)*G1V(dotNV,k);

	float numerator = D * F * vis;
    float denominator = dotNL;
    float rs = numerator/ denominator;

	return dotNL * D * F * vis;
}

void main()
{
	vec3 position = texture2DRect( s_positionBuffer, gl_FragCoord.xy ).xyz;
	vec3 normal = texture2DRect( s_normalBuffer, gl_FragCoord.xy ).xyz;
	vec4 material = texture2DRect( s_materialBuffer, gl_FragCoord.xy );
	vec3 albedo = texture2DRect( s_albedoBuffer, gl_FragCoord.xy ).xyz;
	float ao = texture2DRect( s_aoBuffer, gl_FragCoord.xy * 0.5 ).x;

	vec3 viewDir = normalize( position );

	vec3 lightDir = reflect( viewDir, normal );
	lightDir = normalize( lightDir );

	float roughness = material.x;
	float reflectivity = material.y;
	float emissive = material.z;

	//vec3 light = vec3( LightingFuncGGX_REF( normal, viewDir, lightDir, roughness, reflectivity, 0.999f ) );
	
	vec3 light = u_color;
	light *= ao;
	light *= u_color;
	light *= u_intensity;
	light *= reflectivity;
	light += emissive;
	light *= albedo;

	out_color = vec4( light, 1.0 );
}
";
		}

	}
}
