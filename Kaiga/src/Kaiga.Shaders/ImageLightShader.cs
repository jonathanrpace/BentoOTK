using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class ImageLightShader : AbstractShader<ScreenQuadVertexShader, ImageLightFragShader>
	{
		public void BindPerLight( RenderParams renderParams, ImageLight light )
		{
			fragmentShader.BindPerLight( renderParams, light );
		}
	}
}

