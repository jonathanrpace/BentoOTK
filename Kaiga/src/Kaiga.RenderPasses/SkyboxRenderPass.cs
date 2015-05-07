using Kaiga.Core;
using Kaiga.Materials;
using Kaiga.Shaders;
using Kaiga.Geom;

namespace Kaiga.RenderPasses
{
	public class SkyboxNode : Ramen.Node
	{
		public SkyboxMaterial Material = null;
	}

	public class SkyboxRenderPass : AbstractNodeRenderPass<SkyboxNode>, IRenderPass
	{
		readonly SkyboxShader shader;
		readonly SkyboxGeometry geom;

		public SkyboxRenderPass() : base( RenderPhase.PostLight )
		{
			shader = new SkyboxShader();
			geom = new SkyboxGeometry();
		}

		override public void Dispose()
		{
			base.Dispose();
			shader.Dispose();
			geom.Dispose();
		}

		public void Render( RenderParams renderParams )
		{
			geom.Bind();
			shader.BindPipeline( renderParams );

			//GL.Disable( EnableCap.DepthTest );
			//GL.Disable( EnableCap.CullFace );
			//GL.DepthFunc( DepthFunction.Greater );
			foreach ( var node in nodeList.Nodes )
			{
				shader.SetTexture( node.Material.Texture.Texture );
				geom.Draw();
			}
			//GL.DepthFunc( DepthFunction.Less );

			shader.UnbindPipeline();
			geom.Unbind();
		}
	}
}

