using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Lights;
using OpenTK;

namespace Kaiga.ShaderStages
{
	public class PointLightFragShader : AbstractFragmentShaderStage
	{
		override public void BindPerPass( RenderParams renderParams )
		{
			SetUniformTexture( 0, "s_positionBuffer", renderParams.RenderTarget.GetTexture( FBAttachmentName.Position ), TextureTarget.TextureRectangle );
			SetUniformTexture( 1, "s_normalBuffer", renderParams.RenderTarget.GetTexture( FBAttachmentName.Normal ), TextureTarget.TextureRectangle );
			SetUniformTexture( 2, "s_materialBuffer", renderParams.RenderTarget.GetTexture( FBAttachmentName.Material ), TextureTarget.TextureRectangle );
			SetUniformTexture( 3, "s_albedoBuffer", renderParams.RenderTarget.GetTexture( FBAttachmentName.Albedo ), TextureTarget.TextureRectangle );
		}

		public void BindPerLight( RenderParams renderParams, PointLight light )
		{
			SetUniform1( "u_attenuationConstant", light.AttenuationLinear );
			SetUniform1( "u_attenuationLinear", light.AttenuationLinear );
			SetUniform1( "u_attenuationExp", light.AttenuationExp );
			SetUniform1( "u_radius", light.Radius );
			SetUniform3( "u_color", light.Color );

			// Scale intensity by surface area
			var surfaceArea = 4.0f * (float)Math.PI * light.Radius * light.Radius;
			var intensity = surfaceArea * light.Intensity;
			SetUniform1( "u_intensity", intensity );
			SetUniform3( "u_lightPosition", renderParams.ModelViewMatrix.ExtractTranslation() );
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

uniform float u_attenuationConstant;
uniform float u_attenuationLinear;
uniform float u_attenuationExp;
uniform float u_radius;
uniform vec3 u_color;
uniform float u_intensity;
uniform vec3 u_lightPosition;

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

void main(void)
{
	vec3 position = texture2DRect( s_positionBuffer, gl_FragCoord.xy ).xyz;
	vec3 normal = texture2DRect( s_normalBuffer, gl_FragCoord.xy ).xyz;
	vec4 material = texture2DRect( s_materialBuffer, gl_FragCoord.xy );
	vec3 albedo = texture2DRect( s_albedoBuffer, gl_FragCoord.xy ).xyz;

	vec3 lightDir = u_lightPosition - position.xyz;
	float distance = length( lightDir );
	lightDir = normalize( lightDir );

	float angularSize = clamp( atan( u_radius / distance ), 0.0f, 1.0f );
	angularSize = 1.0f - angularSize;
	
	vec3 viewDir = normalize( -position );

	float roughness = material.x;
	float reflectivity = material.y;
	float emissive = material.z;

	float attenuation = u_attenuationConstant + 
						u_attenuationLinear * distance + 
						u_attenuationExp * distance * distance;
	attenuation = max( attenuation, 1.0f );
	
	//vec3 light = vec3( clamp( dot( lightDir, normal ), 0.0, 1.0 ) );

	vec3 light = vec3( LightingFuncGGX_REF( normal, viewDir, lightDir, roughness, reflectivity, angularSize ) );
	
	light *= u_color;
	light *= u_intensity;
	light /= attenuation;

	light += emissive * albedo;

	out_color = vec4( light, 1.0 );
	//out_color = vec4( angularSize, angularSize, angularSize, 1.0f );
}
";
		}

	}
}

