using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders.Fragment
{
	public class RectangleTextureFragShader : AbstractFragmentShaderStage
	{
		public RectangleTextureFragShader() : base( "RectangleTextureShader.frag" )
		{
		}

		public void SetTexture( int texture )
		{
			SetTexture( "tex", texture, TextureTarget.TextureRectangle );
		}

	}
}

