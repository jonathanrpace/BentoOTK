using System;
using Kaiga.Components;
using Kaiga.Lights;
using Kaiga.Shaders;
using Kaiga.Geom;
using OpenTK;
using Kaiga.Core;

namespace Kaiga.RenderPasses
{
	public class DirectionalLightNode : Ramen.Node
	{
		public Transform transform;
		public DirectionalLight light;
	}

	public class DirectionalLightRenderPass : AbstractNodeRenderPass<DirectionalLightNode>, IRenderPass
	{
		readonly DirectionalLightShader shader;
		readonly ScreenQuadGeometry geom;

		public DirectionalLightRenderPass() : base( RenderPhase.DirectLight )
		{
			shader = new DirectionalLightShader();
			geom = new ScreenQuadGeometry();
		}

		override public void Dispose()
		{
			base.Dispose();
			shader.Dispose();
			geom.Dispose();
		}

		public void Render( RenderParams renderParams )
		{
			geom.Bind();
			shader.BindPerPass(renderParams);

			foreach ( var node in nodeList.Nodes )
			{
				shader.BindPerLight( renderParams, node.light );

				geom.Draw();
			}

			shader.UnbindPerPass();
			geom.Unbind();
		}
	}
}