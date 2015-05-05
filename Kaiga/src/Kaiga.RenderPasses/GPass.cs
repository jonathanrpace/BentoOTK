using Kaiga.Geom;
using Kaiga.Core;
using Kaiga.Shaders;
using Kaiga.Components;
using Kaiga.Materials;

namespace Kaiga.RenderPasses
{
	public class GNode : Ramen.Node
	{
		public IGeometry geom;
		public Transform transform;
		public StandardMaterial material;
	}

	public class GPass : AbstractNodeRenderPass<GNode>, IRenderPass
	{
		private readonly GShader shader;

		public GPass() : base( RenderPhase.G )
		{
			shader = new GShader();
		}

		override public void Dispose()
		{
			base.Dispose();
			shader.Dispose();
		}

		public void Render( RenderParams renderParams )
		{
			shader.BindPipeline( renderParams );
			foreach ( var node in nodeList.Nodes )
			{
				renderParams.SetModelMatrix( node.transform.Matrix );

				node.geom.Bind();
				shader.BindPerMaterial( node.material );
				shader.BindPerModel( renderParams );
				node.geom.Draw();
				node.geom.Unbind();
			}

			shader.UnbindPipeline();
		}
	}
}

