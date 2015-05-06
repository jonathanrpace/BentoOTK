using System;
using Kaiga.Components;
using Kaiga.Lights;
using Kaiga.Shaders;
using Kaiga.Geom;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;

namespace Kaiga.RenderPasses
{
	public class PointLightNode : Ramen.Node
	{
		public Transform transform;
		public PointLight light;
	}

	public class PointLightRenderPass : AbstractNodeRenderPass<PointLightNode>, IRenderPass
	{
		readonly PointLightShader shader;
		readonly PointLightStencilShader stencilShader;
		readonly SphereGeometry geom;

		public PointLightRenderPass() : base( RenderPhase.DirectLight )
		{
			shader = new PointLightShader();
			stencilShader = new PointLightStencilShader();
			geom = new SphereGeometry();
			geom.Radius = 1.0f;
		}

		override public void Dispose()
		{
			base.Dispose();
			shader.Dispose();
			stencilShader.Dispose();
			geom.Dispose();
		}

		public void Render( RenderParams renderParams )
		{
			geom.Bind();

			stencilShader.BindPipeline( renderParams );

			GL.Disable( EnableCap.Blend );
			GL.Disable( EnableCap.CullFace );


			GL.Enable( EnableCap.StencilTest );
			GL.Enable( EnableCap.DepthTest );

			GL.StencilFunc( StencilFunction.Always, 0, 0 );
			GL.StencilOpSeparate( StencilFace.Back, StencilOp.Keep, StencilOp.IncrWrap, StencilOp.Keep );
			GL.StencilOpSeparate( StencilFace.Front, StencilOp.Keep, StencilOp.DecrWrap, StencilOp.Keep );

			renderParams.RenderTarget.BindForNoDraw();
			foreach ( var node in nodeList.Nodes )
			{
				//var scale = CalcPointLightRadius( node.light );
				var scale = node.light.AttenuationRadius * 2.0f;
				Matrix4 mat = Matrix4.CreateScale( scale ) * node.transform.Matrix;
				renderParams.SetModelMatrix( mat );
				stencilShader.BindPerLight( renderParams, node.light );
				geom.Draw();
			}
			stencilShader.UnbindPipeline();


			renderParams.RenderTarget.BindForDirectLightPhase();
			GL.Disable( EnableCap.DepthTest );

			GL.Enable( EnableCap.CullFace );
			GL.CullFace( CullFaceMode.Front );
			GL.Enable( EnableCap.Blend );

			GL.StencilFunc( StencilFunction.Notequal, 0, 0xFF );
			
			shader.BindPipeline( renderParams );
			foreach ( var node in nodeList.Nodes )
			{
				//var scale = CalcPointLightRadius( node.light );
				var scale = node.light.AttenuationRadius * 2.0f;
				Matrix4 mat = Matrix4.CreateScale( scale ) * node.transform.Matrix;
				renderParams.SetModelMatrix( mat );
				shader.BindPerLight( renderParams, node.light );

				geom.Draw();
			}
			shader.UnbindPipeline();

			GL.Enable( EnableCap.DepthTest );
			GL.Disable( EnableCap.StencilTest );
			GL.CullFace( CullFaceMode.Back );

			geom.Unbind();
		}
		/*
		static float CalcPointLightRadius(PointLight light)
		{
			float maxChannel = Math.Max( Math.Max( light.Color.X, light.Color.Y ), light.Color.Z ) * light.Intensity;

			float ret = -light.AttenuationLinear + (float)Math.Sqrt( light.AttenuationLinear * light.AttenuationLinear -
				4.0f * light.AttenuationExp * ( light.AttenuationExp - 256.0f * maxChannel ) );
			ret /= 2.0f * light.AttenuationExp;
				
			//return 5.0f;
			return ret * 2.0f;
		}
		*/
	}
}