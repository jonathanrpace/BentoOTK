using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class SkyboxShader : AbstractShader<SkyboxVertexShader, SkyboxFragmentShader>
	{
		public void SetTexture( int texture )
		{
			fragmentShader.SetTexture( texture );
		}
	}
}

