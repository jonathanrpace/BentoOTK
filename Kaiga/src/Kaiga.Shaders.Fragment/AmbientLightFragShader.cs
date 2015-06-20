using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Lights;

namespace Kaiga.Shaders.Fragment
{
	public class AmbientLightFragShader : AbstractFragmentShaderStage
	{
		override public void BindPerPass()
		{
			base.BindPerPass();
			
			SetTexture( "s_materialBuffer", RenderParams.RenderTarget.MaterialBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_albedoBuffer", RenderParams.RenderTarget.AlbedoBuffer.Texture, TextureTarget.TextureRectangle );
		}

		public void BindPerLight(AmbientLight light ) 
		{
			SetUniform3( "u_color", light.Color );
			SetUniform1( "u_intensity", light.Intensity );
		}

		override protected string GetShaderSource()
		{
			return @"
#version 440 core

// Samplers
uniform sampler2DRect s_materialBuffer;
uniform sampler2DRect s_albedoBuffer;

// Scalars
uniform vec3 u_color;
uniform float u_intensity;

// Outputs
layout( location = 0 ) out vec4 out_color;

void main()
{
	vec4 material = texture2DRect( s_materialBuffer, gl_FragCoord.xy );
	vec3 albedo = texture2DRect( s_albedoBuffer, gl_FragCoord.xy ).xyz;

	float roughness = material.x;
	float reflectivity = material.y;

	vec3 light = pow( u_color * u_intensity, vec3( 2.2 ) );
	light *= reflectivity;
	light *= albedo;

	out_color = vec4( light, 1.0 );
}
";
		}

	}
}
