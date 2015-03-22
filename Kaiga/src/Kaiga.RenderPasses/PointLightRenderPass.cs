using System;
using Kaiga.Components;
using Kaiga.Lights;
using Ramen;
using Kaiga.Shaders;
using Kaiga.Geom;

namespace Kaiga.RenderPasses
{
	public class PointLightRenderPass : IRenderPass
	{
		class Node : Ramen.Node
		{
			public Transform transform = null;
			public PointLight light = null;
		}

		NodeList<Node> nodeList;

		readonly PointLightShader shader;
		readonly SphereGeometry geom;

		public PointLightRenderPass()
		{
			shader = new PointLightShader();
			geom = new SphereGeometry();
			geom.Radius = 0.5f;
		}

		#region IRenderPass implementation

		public void OnAddedToScene( Ramen.Scene scene )
		{
			nodeList = new NodeList<Node>( scene );
		}

		public void OnRemovedFromScene( Ramen.Scene scene )
		{
			nodeList.Dispose();
			nodeList = null;
		}

		public void Render( Kaiga.Core.RenderParams renderParams )
		{
			shader.BindPerPass( renderParams );
			geom.Bind();

			foreach ( Node node in nodeList.Nodes )
			{
				renderParams.SetModelMatrix( node.transform.Matrix );
				shader.BindPerLight( renderParams, node.light );

				shader.Render( renderParams, geom );
			}

			shader.UnbindPerPass();
		}

		public Kaiga.Core.RenderPhase RenderPhase
		{
			get
			{
				return Kaiga.Core.RenderPhase.Light;
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

		#region IGraphicsContextDependant implementation

		public void CreateGraphicsContextResources()
		{
			shader.CreateGraphicsContextResources();
			geom.CreateGraphicsContextResources();
		}

		public void DisposeGraphicsContextResources()
		{
			shader.DisposeGraphicsContextResources();
			geom.DisposeGraphicsContextResources();
		}

		#endregion
	}
}

