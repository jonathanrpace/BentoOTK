using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;

namespace Kaiga.ShaderStages
{
	public class NormalBufferFragShader : ShaderStage
	{
		override protected ShaderType GetShaderType()
		{
			return ShaderType.FragmentShader;
		}
		
		public void Bind( RenderParams renderParams )
		{
			int texUniformLoc = GL.GetUniformLocation( ShaderProgram, "tex" );
			GL.Uniform1( texUniformLoc, 0 );

			GL.ActiveTexture( TextureUnit.Texture0 );
			GL.BindTexture( TextureTarget.TextureRectangle, renderParams.RenderTarget.PositionBuffer );
		}

		public void Unbind()
		{
			GL.BindSampler( 0, 0 );
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

