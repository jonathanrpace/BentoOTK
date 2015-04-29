using System;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders.Fragment
{
	public class SquareTextureFragShader : AbstractFragmentShaderStage
	{
		public SquareTextureFragShader() : base( "SquareTextureFragShader.frag" )
		{
			
		}

		public void SetTexture( int texture )
		{
			SetUniformTexture( "tex", texture, TextureTarget.Texture2D );
		}
	}
}

