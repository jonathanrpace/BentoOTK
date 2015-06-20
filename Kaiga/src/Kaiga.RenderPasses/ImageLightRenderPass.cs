using Kaiga.Core;
using Kaiga.Shaders;
using Kaiga.Lights;
using Kaiga.Geom;

namespace Kaiga.RenderPasses
{
	public class ImageLightNode : Ramen.Node
	{
		public ImageLight light;
	}

	public class ImageLightRenderPass : AbstractNodeRenderPass<ImageLightNode>, IRenderPass
	{
		readonly ImageLightShader shader;
		readonly ScreenQuadGeometry geom;

		public ImageLightRenderPass() : base( RenderPhase.IndirectLight )
		{
			shader = new ImageLightShader();
			geom = new ScreenQuadGeometry();
		}

		override public void Dispose()
		{
			base.Dispose();
			shader.Dispose();
			geom.Dispose();
		}

		public void Render()
		{
			shader.BindPerPass();
			geom.Bind();

			foreach ( var node in nodeList.Nodes )
			{
				shader.BindPerLight( node.light );
				geom.Draw();
			}
		}
	}
}