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

			uniform float u_attenuationLinear;
			uniform float u_attenuationExp;
			uniform vec3 u_color;
			uniform float u_intensity;
			uniform vec3 u_lightPosition;

			// Outputs
			layout( location = 0 ) out vec4 out_color;

			void main(void)
			{
				vec4 position = texture2DRect( s_positionBuffer, gl_FragCoord.xy );
				vec4 normal = texture2DRect( s_normalBuffer, gl_FragCoord.xy );
				vec4 material = texture2DRect( s_materialBuffer, gl_FragCoord.xy );

				vec3 lightDirection = normalize( u_lightPosition - position.xyz );
				
				float dotProduct = clamp( dot( lightDirection, normal.xyz ), 0.0, 1.0 );
				
				out_color = vec4( dotProduct.xxx, 1.0 );
			}
			";
		}

	}
}

