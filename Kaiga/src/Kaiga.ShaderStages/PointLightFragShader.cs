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
		}

		public void BindPerLight( RenderParams renderParams, PointLight light )
		{
			SetUniform1( "u_attenuationConstant", light.AttenuationLinear );
			SetUniform1( "u_attenuationLinear", light.AttenuationLinear );
			SetUniform1( "u_attenuationExp", light.AttenuationExp );
			SetUniform3( "u_color", light.Color );
			SetUniform1( "u_intensity", light.Intensity );
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

uniform float u_attenuationConstant;
uniform float u_attenuationLinear;
uniform float u_attenuationExp;
uniform vec3 u_color;
uniform float u_intensity;
uniform vec3 u_lightPosition;

// Outputs
layout( location = 0 ) out vec4 out_color;

float geomTerm( float vDotH, float nDotH, float nDotV, float nDotL )
{
	float geo_numerator   = 2.0f * nDotH;
	float geo_denominator = vDotH;
 
	float geo_b = (geo_numerator * nDotV ) / geo_denominator;
	float geo_c = (geo_numerator * nDotL) / geo_denominator;
	float geo = min( 1.0, min( geo_b, geo_c ) );

	return geo;
}

float roughnessTerm( float roughness, float nDotH )
{
	float roughness_a = 1.0 / ( 4.0 * roughness * roughness * pow( nDotH, 4 ) );
	float roughness_b = nDotH * nDotH - 1.0;
	float roughness_c = roughness * roughness * nDotH * nDotH;
 
	return roughness_a * exp( roughness_b / roughness_c );
}

float fresnelTerm( float vDotH, float min )
{
	float ret = pow( 1.0 - vDotH, 5.0 );
	ret *= ( 1.0 - min );
	ret += min;
	return ret;
}

vec3 cookTorrence( vec3 normal, vec3 viewer, vec3 lightDir, float roughness, float reflectivity, vec3 specularColor, vec3 diffuseColor )
{
	vec3 halfVector = normalize( lightDir + viewer );
    float nDotL		= clamp( dot( normal, lightDir ), 0.0, 1.0 );
    float nDotH 	= clamp( dot( normal, halfVector ), 0.0, 1.0 );
    float nDotV		= clamp( dot( normal, viewer ), 0.0, 1.0 );
    float vDotH		= clamp( dot( viewer, halfVector ), 0.0, 1.0 );

	float g = geomTerm( vDotH, nDotH, nDotV, nDotL );
	float r = roughnessTerm( roughness, nDotH );
	float f = fresnelTerm( vDotH, reflectivity );


    float numerator = f * g * r ;
    float denominator = nDotV * nDotL;
    vec3 rs = vec3( numerator/ denominator );
 	
    vec3 final = nDotL * (specularColor * rs + diffuseColor);
 	
    return final;
}

float G1V(float dotNV, float k)
{
	return 1.0f/(dotNV*(1.0f-k)+k);
}

float LightingFuncGGX_REF(vec3 N, vec3 V, vec3 L, float roughness, float F0)
{
	float alpha = roughness*roughness;

	vec3 H = normalize(V+L);

	float dotNL = clamp(dot(N,L), 0.0f, 1.0f);
	float dotNV = clamp(dot(N,V), 0.0f, 1.0f);
	float dotNH = clamp(dot(N,H), 0.0f, 1.0f);
	float dotLH = clamp(dot(L,H), 0.0f, 1.0f);

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

	//return (roughness * dotNL) + (1-roughness) * (dotNL * D * F * vis);

	return dotNL * D * F * vis;
}

void main(void)
{
	vec3 position = texture2DRect( s_positionBuffer, gl_FragCoord.xy ).xyz;
	vec3 normal = texture2DRect( s_normalBuffer, gl_FragCoord.xy ).xyz;
	vec4 material = texture2DRect( s_materialBuffer, gl_FragCoord.xy );

	vec3 lightDir = u_lightPosition - position.xyz;
	float distance = length( lightDir );
	lightDir = normalize( lightDir );

	vec3 viewDir = normalize( -position );

	float roughness = material.x;
	float reflectivity = material.y;

	float attenuation = u_attenuationConstant + 
						u_attenuationLinear * distance + 
						u_attenuationExp * distance * distance;

	//vec3 light = cookTorrence( normal, viewDir, lightDir, roughness, reflectivity, u_color, u_color );
	//vec3 light = vec3( clamp( dot( lightDir, normal ), 0.0, 1.0 ) );

	vec3 light = vec3( LightingFuncGGX_REF( normal, viewDir, lightDir, roughness, reflectivity ) );
	

	light *= u_color;
	light *= u_intensity;
	light /= attenuation;

	out_color = vec4( light, 1.0 );
}
";
		}

	}
}

