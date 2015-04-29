using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Lights;

namespace Kaiga.Shaders.Fragment
{
	public class AmbientLightFragShader : AbstractFragmentShaderStage
	{
		override public void BindPerPass( RenderParams renderParams )
		{
			base.BindPerPass( renderParams );

			SetUniformTexture( "s_normalBuffer", renderParams.RenderTarget.NormalBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( "s_materialBuffer", renderParams.RenderTarget.MaterialBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( "s_albedoBuffer", renderParams.RenderTarget.AlbedoBuffer.Texture, TextureTarget.TextureRectangle );
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

// Scalars
uniform vec3 u_color;
uniform float u_intensity;

// Outputs
layout( location = 0 ) out vec4 out_color;

void main()
{
	vec3 position = texture2DRect( s_positionBuffer, gl_FragCoord.xy ).xyz;
	vec3 normal = texture2DRect( s_normalBuffer, gl_FragCoord.xy ).xyz;
	vec4 material = texture2DRect( s_materialBuffer, gl_FragCoord.xy );
	vec3 albedo = texture2DRect( s_albedoBuffer, gl_FragCoord.xy ).xyz;

	vec3 viewDir = normalize( position );

	vec3 lightDir = reflect( viewDir, normal );
	lightDir = normalize( lightDir );

	float roughness = material.x;
	float reflectivity = material.y;
	float emissive = material.z;

	vec3 light = u_color;
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
