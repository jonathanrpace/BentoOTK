using System;
using Ramen;
using Kaiga.Core;
using Kaiga.Shaders;
using Kaiga.Lights;

namespace Kaiga.RenderPasses
{
	public class ImageLightRenderPass : IRenderPass
	{
		class Node : Ramen.Node
		{
			public ImageLight light = null;
		}

		private NodeList<Node> nodeList;

		ImageLightShader shader;

		public ImageLightRenderPass()
		{
			shader = new ImageLightShader();
		}

		#region IDisposable implementation

		public void Dispose()
		{
			shader.Dispose();
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
			shader.BindPerPass( renderParams );

			foreach ( var node in nodeList.Nodes )
			{
				shader.BindPerLight( renderParams, node.light );
				shader.Render();
			}

			shader.UnbindPerPass();
		}

		public RenderPhase RenderPhase
		{
			get
			{
				return RenderPhase.Light;
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

