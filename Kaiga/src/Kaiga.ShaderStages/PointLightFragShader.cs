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
 	
    vec3 final = max(0.0, nDotL) * (specularColor * rs + diffuseColor);
 	
    return vec3(final);
}

void main(void)
{
	vec4 position = texture2DRect( s_positionBuffer, gl_FragCoord.xy );
	vec4 normal = texture2DRect( s_normalBuffer, gl_FragCoord.xy );
	vec4 material = texture2DRect( s_materialBuffer, gl_FragCoord.xy );

	vec3 lightDir = u_lightPosition - position.xyz;
	float distance = length( lightDir );


	float attenuation = u_attenuationConstant + 
						u_attenuationLinear * distance + 
						u_attenuationExp * distance * distance;

	if ( attenuation < 1.0/255.0 )
	{
		out_color = vec4( 0.0, 0.0, 0.0, 1.0 );
	}
	else
	{

		vec3 light = cookTorrence( normal.xyz, vec3( 0.0, 0.0, 1.0 ), normalize( lightDir ), material.y, material.x, u_color, u_color );

		light *= u_intensity;
		light /= attenuation;

		out_color = vec4( light.xyz, 1.0 );
	}
}
";
		}

	}
}

