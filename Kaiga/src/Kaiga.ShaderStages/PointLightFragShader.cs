using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Lights;

namespace Kaiga.ShaderStages
{
	public class PointLightFragShader : AbstractFragmentShaderStage
	{
		override public void BindPerPass( RenderParams renderParams )
		{
			SetUniform1( "positionBuffer", 0 );
			GL.ActiveTexture( TextureUnit.Texture0 );
			GL.BindTexture( TextureTarget.TextureRectangle, renderParams.RenderTarget.GetTexture( FBAttachmentName.Position ) );

			SetUniform1( "normalBuffer", 1 );
			GL.ActiveTexture( TextureUnit.Texture1 );
			GL.BindTexture( TextureTarget.TextureRectangle, renderParams.RenderTarget.GetTexture( FBAttachmentName.Normal ) );

			SetUniform1( "materialBuffer", 2 );
			GL.ActiveTexture( TextureUnit.Texture2 );
			GL.BindTexture( TextureTarget.TextureRectangle, renderParams.RenderTarget.GetTexture( FBAttachmentName.Material ) );
		}

		public void BindPerLight( PointLight light )
		{
			SetUniform1( "u_attenuationLinear", light.AttenuationLinear );
			SetUniform1( "u_attenuationExp", light.AttenuationExp );
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

			uniform float u_attenuationLinear;
			uniform float u_attenuationExp;
			uniform vec3 u_color;
			uniform float u_intensity;

			// Outputs
			layout( location = 4 ) out vec4 out_color;

			void main(void)
			{
				out_color = vec4( 1.0, 0.0, 0.0, 1.0 );
			}
			";
		}

	}
}

