using System;
using Ramen;
using Kaiga.Core;
using Kaiga.Shaders;
using Kaiga.Lights;
using Kaiga.Geom;

namespace Kaiga.RenderPasses
{
	public class AmbientLightRenderPass : AbstractRenderPass, IRenderPass
	{
		class Node : Ramen.Node
		{
			public AmbientLight light = null;
		}

		NodeList<Node> nodeList;
		readonly AmbientLightShader shader;
		readonly ScreenQuadGeometry geom;

		public AmbientLightRenderPass() 
		: base( RenderPhase.IndirectLight )
		{
			shader = new AmbientLightShader();
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
		
		#endregion
	}
}

