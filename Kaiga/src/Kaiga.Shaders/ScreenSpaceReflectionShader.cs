using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using Kaiga.Geom;
using Kaiga.Core;

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

		public void Render( RenderParams renderParams )
		{
			BindPipeline( renderParams );

			BindFragmentShader();
			fragmentShader.BindPerPass( renderParams );
			BindVertexShader();

			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
			screenQuadGeom.Unbind();

			UnbindPipeline();
		}
	}
}
