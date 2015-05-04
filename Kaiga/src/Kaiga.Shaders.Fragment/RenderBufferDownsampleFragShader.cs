using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders.Fragment
{
	public class RenderBufferDownsampleFragShader : AbstractFragmentShaderStage
	{
		public RenderBufferDownsampleFragShader()
			:base( "RenderBufferDownsampleShader.frag" )
		{
		}

		public void SetTexture( int texture, int level, float aspectRatio )
		{
			SetUniformTexture( "s_tex", texture, TextureTarget.Texture2D );
			SetUniform1( "u_mipLevel", level );
			SetUniform1( "u_aspectRatio", aspectRatio );
		}
	}
}