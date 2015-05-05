using Kaiga.Core;
using Kaiga.Shaders;
using Kaiga.Lights;
using Kaiga.Geom;

namespace Kaiga.RenderPasses
{
	public class AmbientLightNode : Ramen.Node
	{
		public AmbientLight light;
	}

	public class AmbientLightRenderPass : AbstractNodeRenderPass<AmbientLightNode>, IRenderPass
	{
		readonly AmbientLightShader shader;
		readonly ScreenQuadGeometry geom;

		public AmbientLightRenderPass() : base( RenderPhase.IndirectLight )
		{
			shader = new AmbientLightShader();
			geom = new ScreenQuadGeometry();
		}
		
		override public void Dispose()
		{
			base.Dispose();
			shader.Dispose();
			geom.Dispose();
		}
		
		public void Render( RenderParams renderParams )
		{
			shader.BindPipeline( renderParams );
			geom.Bind();

			foreach ( var node in nodeList.Nodes )
			{
				shader.BindPerLight( renderParams, node.light );
				geom.Draw();
			}

			geom.Unbind();
			shader.UnbindPipeline();
		}
	}
}

