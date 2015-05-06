using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders.Fragment
{
	public class LightBufferConvolutionFragShader : AbstractFragmentShaderStage
	{
		public LightBufferConvolutionFragShader()
			:base( "LightBufferConvolutionShader.frag" )
		{
		}

		public void SetTexture( int texture, int level, float aspectRatio )
		{
			SetTexture( "s_tex", texture, TextureTarget.Texture2D );
			SetUniform1( "u_mipLevel", level );
			SetUniform1( "u_aspectRatio", aspectRatio );
		}
	}
}