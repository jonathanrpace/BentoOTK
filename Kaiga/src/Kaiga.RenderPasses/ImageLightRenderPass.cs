using System;
using Ramen;
using Kaiga.Core;
using Kaiga.Shaders;
using Kaiga.Lights;
using Kaiga.Geom;

namespace Kaiga.RenderPasses
{
	public class ImageLightRenderPass : IRenderPass
	{
		class Node : Ramen.Node
		{
			public ImageLight light = null;
		}

		NodeList<Node> nodeList;
		ImageLightShader shader;
		ScreenQuadGeometry geom;

		public ImageLightRenderPass()
		{
			shader = new ImageLightShader();
			geom = new ScreenQuadGeometry();
		}

		#region IDisposable implementation

		public void Dispose()
		{
			shader.Dispose();
			geom.Dispose();
			if ( nodeList != null )
			{
				nodeList.Dispose();
				nodeList = null;
			}
		}

		#endregion

		#region IRenderPass implementation

		public void OnAddedToScene( Scene scene )
		{
			nodeList = new NodeList<Node>( scene );
		}

		public void OnRemovedFromScene( Scene scene )
		{
			if ( nodeList != null )
			{
				nodeList.Dispose();
				nodeList = null;
			}
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

		public RenderPhase RenderPhase
		{
			get
			{
				return RenderPhase.IndirectLight;
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

