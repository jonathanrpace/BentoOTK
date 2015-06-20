using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using Kaiga.Geom;

namespace Kaiga.Shaders
{
	public class ScreenSpaceReflectionShader : AbstractShader<ScreenQuadVertexShader, ScreenSpaceReflectionFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		public ScreenSpaceReflectionShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render()
		{
			BindPerPass();

			fragmentShader.BindPerPass();

			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
		}
	}
}
