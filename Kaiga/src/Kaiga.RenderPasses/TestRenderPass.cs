using System;
using Ramen;
using Kaiga.Geom;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders;

namespace Kaiga.RenderPasses
{
	class Node : Ramen.Node
	{
		public Geometry geom;

		public Node()
		{
			geom = null;
		}
	}

	public class TestRenderPass : IRenderPass
	{
		private NodeList<Node> nodeList;
		private readonly TestShader shader;

		public TestRenderPass()
		{
			shader = new TestShader();
		}

		#region IGraphicsContextDependant implementation

		public void CreateGraphicsContextResources()
		{
			shader.CreateGraphicsContextResources();
		}

		public void DisposeGraphicsContextResources()
		{
			shader.DisposeGraphicsContextResources();
		}

		#endregion

		public void OnAddedToScene( Scene scene )
		{
			nodeList = new NodeList<Node>( scene );
		}

		public void OnRemovedFromScene( Ramen.Scene scene )
		{
			nodeList.Dispose();
			nodeList = null;
		}

		public void Render( RenderParams renderParams )
		{
			shader.Bind( renderParams );

			foreach ( Node node in nodeList.Nodes )
			{
				node.geom.Bind();
				GL.DrawElements( PrimitiveType.Triangles, node.geom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
				node.geom.Unbind();
			}

			shader.Unbind();
		}

		public RenderPhase RenderPhase
		{
			get
			{
				return RenderPhase.Material;
			}
		}

		public bool Enabled
		{
			get
			{
				return true;
			}
		}
	}
}

