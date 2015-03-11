using System;
using Ramen;
using Kaiga.Geom;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders;
using Kaiga.Components;

namespace Kaiga.RenderPasses
{
	class Node : Ramen.Node
	{
		public Geometry geom;
		public Transform transform;

		public Node()
		{
			geom = null;
			transform = null;
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
			shader.Begin();
			foreach ( Node node in nodeList.Nodes )
			{
				node.geom.Bind();
				renderParams.SetModelMatrix( node.transform.Matrix );
				shader.BindPerModel( renderParams );

				GL.DrawElements( PrimitiveType.Triangles, node.geom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
				node.geom.Unbind();
			}

			shader.End();
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

