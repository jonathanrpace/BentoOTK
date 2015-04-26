using System;
using Kaiga.Core;
using Kaiga.Materials;
using Ramen;
using Kaiga.Shaders;
using Kaiga.Geom;
using OpenTK.Graphics.OpenGL4;

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
			shader.BindPerPass( renderParams );

			//GL.Disable( EnableCap.DepthTest );
			//GL.Disable( EnableCap.CullFace );
			//GL.DepthFunc( DepthFunction.Greater );
			foreach ( var node in nodeList.Nodes )
			{
				shader.SetTexture( node.Material.Texture.Texture );
				GL.DrawElements( PrimitiveType.Triangles, geom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
			}
			//GL.DepthFunc( DepthFunction.Less );

			shader.UnbindPerPass();
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

