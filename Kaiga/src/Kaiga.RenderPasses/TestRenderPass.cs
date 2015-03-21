using System;
using Ramen;
using Kaiga.Geom;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders;
using Kaiga.Components;
using Kaiga.Materials;

namespace Kaiga.RenderPasses
{
	class Node : Ramen.Node
	{
		public Geometry geom = null;
		public Transform transform = null;
		public StandardMaterial material = null;
	}

	public class TestRenderPass : IRenderPass
	{
		private NodeList<Node> nodeList;
		private readonly GShader shader;

		public TestRenderPass()
		{
			shader = new GShader();
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
			shader.BindPerPass();
			foreach ( Node node in nodeList.Nodes )
			{
				renderParams.SetModelMatrix( node.transform.Matrix );

				node.geom.Bind();
				shader.BindPerMaterial( node.material );
				shader.BindPerModel( renderParams );

				GL.DrawElements( PrimitiveType.Triangles, node.geom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 

				node.geom.Unbind();
			}

			shader.UnbindPerPass();
		}

		public RenderPhase RenderPhase
		{
			get
			{
				return RenderPhase.G;
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

