using System;
using Kaiga.Components;
using Kaiga.Lights;
using Ramen;
using Kaiga.Shaders;
using Kaiga.Geom;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;

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
		readonly PointLightStencilShader stencilShader;
		readonly SphereGeometry geom;

		public PointLightRenderPass()
		{
			shader = new PointLightShader();
			stencilShader = new PointLightStencilShader();
			geom = new SphereGeometry();
			geom.Radius = 1.0f;
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

		/*
void DSStencilPass(unsigned int PointLightIndex)
{
    m_nullTech.Enable();

    // Disable color/depth write and enable stencil
    m_gbuffer.BindForStencilPass();
    glEnable(GL_DEPTH_TEST);

    glDisable(GL_CULL_FACE);

    glClear(GL_STENCIL_BUFFER_BIT);

    // We need the stencil test to be enabled but we want it
    // to succeed always. Only the depth test matters.
    glStencilFunc(GL_ALWAYS, 0, 0);

    glStencilOpSeparate(GL_BACK, GL_KEEP, GL_INCR_WRAP, GL_KEEP);
    glStencilOpSeparate(GL_FRONT, GL_KEEP, GL_DECR_WRAP, GL_KEEP);

    Pipeline p;
    p.WorldPos(m_pointLight[PointLightIndex].Position);
    float BBoxScale = CalcPointLightBSphere(m_pointLight[PointLightIndex].Color, 
        m_pointLight[PointLightIndex].DiffuseIntensity);
    p.Scale(BBoxScale, BBoxScale, BBoxScale);	
    p.SetCamera(m_pGameCamera->GetPos(), m_pGameCamera->GetTarget(), m_pGameCamera->GetUp());
    p.SetPerspectiveProj(m_persProjInfo);

    m_nullTech.SetWVP(p.GetWVPTrans());
    m_bsphere.Render(); 
}
*/

		public void Render( Kaiga.Core.RenderParams renderParams )
		{
			geom.Bind();

			stencilShader.BindPerPass( renderParams );

			GL.Disable( EnableCap.Blend );
			GL.Disable( EnableCap.CullFace );


			GL.Enable( EnableCap.StencilTest );
			GL.Enable( EnableCap.DepthTest );

			GL.StencilFunc( StencilFunction.Always, 0, 0 );
			GL.StencilOpSeparate( StencilFace.Back, StencilOp.Keep, StencilOp.IncrWrap, StencilOp.Keep );
			GL.StencilOpSeparate( StencilFace.Front, StencilOp.Keep, StencilOp.DecrWrap, StencilOp.Keep );
			/*
			Need to reorder and Interleaved stencil with lighting
			Light0-Stencil
			Light0-Shader
			Light1-Stencil
			Light1-Shader
			...
				*/
			( (DeferredRenderTarget)renderParams.RenderTarget ).BindForNoDraw();
			foreach ( Node node in nodeList.Nodes )
			{
				var scale = CalcPointLightRadius( node.light );
				//var scale = node.light.Radius;
				Matrix4 mat = Matrix4.CreateScale( scale ) * node.transform.Matrix;
				renderParams.SetModelMatrix( mat );
				stencilShader.BindPerLight( renderParams, node.light );
				stencilShader.Render( renderParams, geom );
			}
			stencilShader.UnbindPerPass();


			( (DeferredRenderTarget)renderParams.RenderTarget ).BindForLightPhase();
			GL.Disable( EnableCap.DepthTest );

			GL.Enable( EnableCap.CullFace );
			GL.CullFace( CullFaceMode.Front );
			GL.Enable( EnableCap.Blend );

			GL.StencilFunc( StencilFunction.Notequal, 0, 0xFF );
			
			shader.BindPerPass( renderParams );
			foreach ( Node node in nodeList.Nodes )
			{
				var scale = CalcPointLightRadius( node.light );
				//var scale = node.light.Radius;
				Matrix4 mat = Matrix4.CreateScale( scale ) * node.transform.Matrix;
				renderParams.SetModelMatrix( mat );
				shader.BindPerLight( renderParams, node.light );

				shader.Render( renderParams, geom );
			}
			shader.UnbindPerPass();

			GL.Enable( EnableCap.DepthTest );
			GL.Disable( EnableCap.StencilTest );
			GL.CullFace( CullFaceMode.Back );
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
			stencilShader.CreateGraphicsContextResources();
			geom.CreateGraphicsContextResources();
		}

		public void DisposeGraphicsContextResources()
		{
			shader.DisposeGraphicsContextResources();
			stencilShader.DisposeGraphicsContextResources();
			geom.DisposeGraphicsContextResources();
		}

		#endregion

		static float CalcPointLightRadius(PointLight light)
		{
			float maxChannel = Math.Max( Math.Max( light.Color.X, light.Color.Y ), light.Color.Z ) * light.Intensity;

			float ret = -light.AttenuationLinear + (float)Math.Sqrt( light.AttenuationLinear * light.AttenuationLinear -
				4.0f * light.AttenuationExp * ( light.AttenuationExp - 256.0f * maxChannel ) );
			ret /= 2.0f * light.AttenuationExp;
				
			//return 5.0f;
			return ret * 2.0f;
		}
	}
}