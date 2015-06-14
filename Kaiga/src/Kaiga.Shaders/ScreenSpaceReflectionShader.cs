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
			BindPerPass( renderParams );

			ActivateFragmentShader();
			fragmentShader.BindPerPass( renderParams );
			ActivateVertexShader();

			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
			screenQuadGeom.Unbind();

			UnbindPerPass();
		}
	}
}
