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
	public class TestRenderPass : IRenderPass
	{
		class Node : Ramen.Node
		{
			public Geometry geom = null;
			public Transform transform = null;
			public StandardMaterial material = null;
		}

		private NodeList<Node> nodeList;
		private readonly GShader shader;

		public TestRenderPass()
		{
			shader = new GShader();
		}

		public void Dispose()
		{
			shader.Dispose();
			if ( nodeList != null )
			{
				nodeList.Dispose();
				nodeList = null;
			}
		}

		public void OnAddedToScene( Scene scene )
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

		public void Render( RenderParams renderParams )
		{
			shader.BindPerPass( renderParams );
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

