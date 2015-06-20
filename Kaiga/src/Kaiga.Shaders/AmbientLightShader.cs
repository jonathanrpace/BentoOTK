using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class AmbientLightShader : AbstractShader<ScreenQuadVertexShader, AmbientLightFragShader>
	{
		public void BindPerLight( AmbientLight light )
		{
			fragmentShader.BindPerLight( light );
		}
	}
}

