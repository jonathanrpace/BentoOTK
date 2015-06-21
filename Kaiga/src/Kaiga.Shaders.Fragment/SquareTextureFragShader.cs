using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace Kaiga.Shaders.Fragment
{
	public class SquareTextureFragShader : AbstractFragmentShaderStage
	{
		public SquareTextureFragShader() : base( "SquareTextureFragShader.frag" )
		{
			
		}

		public void SetTexture( int texture )
		{
			SetTexture( "tex", texture, TextureTarget.Texture2D );
			SetUniform1( "u_mipRatio", (float)Mouse.GetState().X / 100.0f );
		}
	}
}

