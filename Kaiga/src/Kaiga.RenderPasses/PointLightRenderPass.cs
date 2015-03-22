using System;
using Kaiga.Components;
using Kaiga.Lights;
using Ramen;
using Kaiga.Shaders;
using Kaiga.Geom;
using OpenTK;

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
				var scale = 1.5f;//CalcPointLightRadius( node.light );
				Matrix4 mat = node.transform.Matrix * Matrix4.CreateScale( scale );
				renderParams.SetModelMatrix( mat );
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

		static float CalcPointLightRadius(PointLight light)
		{
			float maxChannel = Math.Max( Math.Max( light.Color.X, light.Color.Y ), light.Color.Z ) * light.Intensity;

			float ret = -light.AttenuationLinear + (float)Math.Sqrt( light.AttenuationLinear * light.AttenuationLinear -
				4.0f * light.AttenuationExp * ( light.AttenuationConstant - 256.0f * maxChannel ) );
			ret /= 2.0f * light.AttenuationExp;
				
			return ret;
		}
	}
}

