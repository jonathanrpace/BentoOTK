using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;

namespace Kaiga.ShaderStages
{
	public class TextureOutputFragShader : AbstractFragmentShaderStage
	{
		public void Bind( int texture )
		{
			SetUniformTexture( 0, "text", texture, TextureTarget.TextureRectangle );
		}

		override protected string GetShaderSource()
		{
			return @"
			#version 450 core

			// Samplers
			uniform sampler2DRect tex;

			// Outputs
			layout( location = 0 ) out vec4 out_fragColor;

			void main(void)
			{ 
				out_fragColor = texture2DRect( tex, gl_FragCoord.xy );
			}
			";
		}

	}
}

