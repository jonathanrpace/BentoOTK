using Kaiga.Core;
using Kaiga.Materials;
using Ramen;
using Kaiga.Shaders;
using Kaiga.Geom;

namespace Kaiga.RenderPasses
{
	public class SkyboxRenderPass : IRenderPass
	{
		class Node : Ramen.Node
		{
			public SkyboxMaterial Material = null;
		}

		NodeList<Node> nodeList;
		SkyboxShader shader;
		SkyboxGeometry geom;

		public SkyboxRenderPass()
		{
			shader = new SkyboxShader();
			geom = new SkyboxGeometry();
		}

		#region IDisposable implementation

		public void Dispose()
		{
			if ( nodeList != null )
			{
				nodeList.Dispose();
				nodeList = null;
			}

			shader.Dispose();
			geom.Dispose();
		}

		#endregion

		#region IRenderPass implementation

		public void OnAddedToScene( Ramen.Scene scene )
		{
			nodeList = new NodeList<Node>( scene );
		}

		public void OnRemovedFromScene( Ramen.Scene scene )
		{
			if ( nodeList != null )
			{
				nodeList.Dispose();
				nodeList = null;
			}
		}

		public void Render( Kaiga.Core.RenderParams renderParams )
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

		public RenderPhase RenderPhase
		{
			get
			{
				return RenderPhase.Resolve;
			}
		}

		public bool Enabled
		{
			get
			{
				return true;
			}
		}

		#endregion


	}
}

