using Kaiga.Core;
using Kaiga.Geom;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class BoxBlurShader : AbstractShader<ScreenQuadVertexShader, DepthAwareBlurFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		public BoxBlurShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( RenderParams renderParams, int texture, float radiusU, float radiusV )
		{
			BindPipeline( renderParams );
			fragmentShader.Bind( renderParams, texture, radiusU, radiusV );
			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
			UnbindPipeline();
		}
	}
}

