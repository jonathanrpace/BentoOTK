using System;
using Kaiga.RenderPasses;
using Ramen;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Components;
using OpenTK;
using OpenTK.Graphics;
using System.Security.AccessControl;

namespace Kaiga.Core
{
	public class DeferredRenderer : NamedObject, IRenderer, IGraphicsContextDependant, IMultiTypeObject
	{
		private Scene										scene;
		private RenderParams 								renderParams;
		private List<IRenderPass>							renderPasses;
		private Dictionary<RenderPhase, List<IRenderPass>> 	passesByPhase;
		private Dictionary<Type, IRenderPass>				passesByType;
		private RenderTarget2D								renderTarget;

		public Entity									Camera { get; private set; }
		public IEnumerable<IRenderPass>					RenderPasses { get { return renderPasses.Skip( 0 ); } }
		public BackBufferOutputMode 					BackBufferOutputMode { get; set; }

		public event RenderPassDelegate OnRenderPassAdded;
		public event RenderPassDelegate OnRenderPassRemoved;

		bool graphicsContextAvailable = false;

		public DeferredRenderer() : this( "Deferred Renderer" )
		{

		}

		public DeferredRenderer( string name ) : base( name )
		{
			renderParams = new RenderParams();
			renderPasses = new List<IRenderPass>();
			passesByPhase = new Dictionary<RenderPhase, List<IRenderPass>>();
			passesByType = new Dictionary<Type, IRenderPass>();
			renderTarget = new RenderTarget2D();

			Camera = new Entity();
			var lens = new PerspectiveLens();
			Camera.AddComponent( lens );
			var cameraTransform = new Transform( Matrix4.Identity * Matrix4.CreateTranslation( 0.0f, 0.0f, -2.0f ) );
			Camera.AddComponent( cameraTransform );

			AddRenderPhase( RenderPhase.G );
			AddRenderPhase( RenderPhase.Light );
			AddRenderPhase( RenderPhase.Material );
			AddRenderPhase( RenderPhase.Post );
		}

		#region IGLContextDependant implementation

		public void CreateGraphicsContextResources()
		{
			graphicsContextAvailable = true;
			renderTarget.CreateGraphicsContextResources();

			foreach ( var renderPass in renderPasses )
			{
				renderPass.CreateGraphicsContextResources();
			}

			GL.ClearColor( System.Drawing.Color.AliceBlue );
		}

		public void DisposeGraphicsContextResources()
		{
			graphicsContextAvailable = false;
			renderTarget.DisposeGraphicsContextResources();

			foreach ( var renderPass in renderPasses )
			{
				renderPass.DisposeGraphicsContextResources();
			}
		}

		#endregion

		public void OnAddedToScene( Scene scene )
		{
			this.scene = scene;

			foreach ( IRenderPass renderPass in renderPasses )
			{
				renderPass.OnAddedToScene( scene );
			}
		}

		public void OnRemovedFromScene( Scene scene )
		{
			this.scene = null;

			foreach ( IRenderPass renderPass in renderPasses )
			{
				renderPass.OnRemovedFromScene( scene );
			}
		}

		public void AddRenderPass( IRenderPass renderPass )
		{
			Debug.Assert( passesByPhase.ContainsKey( renderPass.RenderPhase ), "Phase does not exist." );
			Debug.Assert( passesByType.ContainsKey( renderPass.GetType() ) == false, "Pass of this type already added." );

			List<IRenderPass> passesForThisPhase = passesByPhase[ renderPass.RenderPhase ];
			passesForThisPhase.Add( renderPass );
			renderPasses.Add( renderPass );
			passesByType.Add( renderPass.GetType(), renderPass );

			if ( scene != null )
			{
				renderPass.OnAddedToScene( scene );
			}

			if ( OnRenderPassAdded != null )
			{
				OnRenderPassAdded( renderPass );
			}
		}

		public void RemoveRenderPass( IRenderPass renderPass )
		{
			Debug.Assert( passesByPhase.ContainsKey( renderPass.RenderPhase ), "Phase does not exist." );
			Debug.Assert( passesByType.ContainsKey( renderPass.GetType() ) == false, "Pass not child of this renderer." );

			List<IRenderPass> passesForThisPhase = passesByPhase[ renderPass.RenderPhase ];
			passesForThisPhase.Remove( renderPass );
			renderPasses.Remove( renderPass );
			passesByType.Remove( renderPass.GetType() );

			if ( scene != null )
			{
				renderPass.OnRemovedFromScene( scene );
			}

			if ( OnRenderPassRemoved != null )
			{
				OnRenderPassRemoved( renderPass );
			}
		}

		public T GetRenderPass<T>() where T : IRenderPass
		{
			return (T)passesByType[ typeof(T) ];
		}

		public void Update( double dt )
		{
			RenderToBackBuffer( renderTarget );
		}

		public void Render( IRenderTarget renderTarget )
		{

		}

		public void RenderToBackBuffer( RenderTarget2D renderTarget )
		{
			if (!graphicsContextAvailable )
			{
				return;
			}
			renderTarget.SetSize( scene.GameWindow.Width, scene.GameWindow.Height );

			renderParams.CameraLens = Camera.GetComponentByType<ILens>();
			renderParams.CameraLens.AspectRatio = (float)scene.GameWindow.Width / scene.GameWindow.Height;

			renderParams.ViewMatrix = Camera.GetComponentByType<Transform>().Matrix;
			renderParams.InvViewMatrix = renderParams.ViewMatrix.Inverted();
			renderParams.ProjectionMatrix = renderParams.CameraLens.ProjectionMatrix;
			renderParams.InvProjectionMatrix = renderParams.CameraLens.InvProjectionMatrix;
			renderParams.ViewProjectionMatrix = renderParams.ViewMatrix * renderParams.ProjectionMatrix;
			renderParams.InvViewProjectionMatrix = renderParams.ViewProjectionMatrix.Inverted();

			/*
			renderParams.renderTarget = renderTarget;
			renderParams.surfaceSelector = surfaceSelector;
			renderParams.cameraLens = camera.getComponent(PerspectiveLens);
			renderParams.cameraTransform = camera.getComponent(Transform3D);
			renderParams.cameraLens.aspectRatio = scene.viewport.width / scene.viewport.height;
			renderParams.viewMatrix.copyFrom( renderParams.cameraTransform.matrix );
			renderParams.normalViewMatrix.copyFrom( renderParams.cameraTransform.rotationMatrix );
			renderParams.viewMatrix.invert();
			renderParams.normalViewMatrix.invert();
			renderParams.invViewMatrix.copyFrom( renderParams.cameraTransform.matrix );
			renderParams.projectionMatrix.copyFrom( renderParams.cameraLens.matrix );
			renderParams.viewProjectionMatrix.copyFrom( renderParams.viewMatrix );
			renderParams.viewProjectionMatrix.append( renderParams.projectionMatrix );
			renderParams.invProjectionMatrix.copyFrom( renderParams.projectionMatrix );
			renderParams.invProjectionMatrix.invert();
			renderParams.invViewProjectionMatrix.copyFrom( renderParams.invProjectionMatrix );
			renderParams.invViewProjectionMatrix.append( renderParams.invViewMatrix );

			renderParams.cameraPosition[0] = renderParams.cameraTransform.positionX;
			renderParams.cameraPosition[1] = renderParams.cameraTransform.positionY;
			renderParams.cameraPosition[2] = renderParams.cameraTransform.positionZ;
			renderParams.cameraForward = renderParams.cameraTransform.getForward();
			renderParams.cameraBackward = renderParams.cameraForward.clone();
			renderParams.cameraBackward.negate();

			renderParams.gBuffer = renderTarget.getGBuffer();
			renderParams.lBuffer = renderTarget.getLBuffer();
			renderParams.outBuffer = renderTarget.getOutBuffer();
			renderParams.gBufferPost = renderTarget.getGBufferPost();
			renderParams.outBufferPost = renderTarget.getOutBufferPost();
			*/

			/*
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, renderTarget.FrameBuffer );

			float[] black = { 1.0f, 1.0f, 0.0f, 0.0f };

			// G Phase

			// Bind normal buffer to Color0
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, renderTarget.NormalBuffer );
			GL.FramebufferRenderbuffer
			( 
				FramebufferTarget.DrawFramebuffer,
				FramebufferAttachment.ColorAttachment0, 
				RenderbufferTarget.Renderbuffer, 
				renderTarget.NormalBuffer
			);

			// Bind position buffer to Color1
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, renderTarget.PositionBuffer );
			GL.FramebufferRenderbuffer
			( 
				FramebufferTarget.DrawFramebuffer,
				FramebufferAttachment.ColorAttachment1, 
				RenderbufferTarget.Renderbuffer, 
				renderTarget.PositionBuffer
			);

			// Bind depth buffer to Depth
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, renderTarget.DepthBuffer );
			GL.FramebufferRenderbuffer
			( 
				FramebufferTarget.DrawFramebuffer,
				FramebufferAttachment.DepthAttachment, 
				RenderbufferTarget.Renderbuffer, 
				renderTarget.DepthBuffer
			);

			var code = GL.CheckFramebufferStatus( FramebufferTarget.DrawFramebuffer );

			GL.ClearBuffer( ClearBuffer.Color, renderTarget.FrameBuffer, black );
			//GL.ClearBuffer( ClearBuffer.Depth, renderTarget.FrameBuffer, black );

			//GL.ClearColor( 0.0f, 0.0f, 0.0f, 0.0f );
			//GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

			RenderPassesInPhase( passesByPhase[ RenderPhase.G ] );

			// Light Phase
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, renderTarget.LBuffer );
			GL.FramebufferRenderbuffer
			( 
				FramebufferTarget.DrawFramebuffer,
				FramebufferAttachment.ColorAttachment0, 
				RenderbufferTarget.Renderbuffer, 
				renderTarget.LBuffer
			);

			RenderPassesInPhase( passesByPhase[ RenderPhase.Light ] );

			// Material Pass
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, renderTarget.MaterialBuffer );
			GL.FramebufferRenderbuffer
			( 
				FramebufferTarget.DrawFramebuffer,
				FramebufferAttachment.ColorAttachment0, 
				RenderbufferTarget.Renderbuffer, 
				renderTarget.MaterialBuffer
			);

			GL.ClearBuffer( ClearBuffer.Color, renderTarget.FrameBuffer, black ); 

			RenderPassesInPhase( passesByPhase[ RenderPhase.Material ] );

			*/

			renderTarget.Bind();

			GL.Enable(EnableCap.DepthTest);
			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

			// Material Pass
			//GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, 0 );
			RenderPassesInPhase( passesByPhase[ RenderPhase.Material ] );

			renderTarget.Unbind();

			// Copy to the back buffer

			GL.BindFramebuffer( FramebufferTarget.ReadFramebuffer, renderTarget.FrameBuffer );
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, 0 );

			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );
			GL.BlitFramebuffer
			(
				0, 0, renderTarget.Width, renderTarget.Height, 
				0, 0, renderTarget.Width, renderTarget.Height,
				ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest
			);


			scene.GameWindow.SwapBuffers();
		}


		void RenderPassesInPhase( List<IRenderPass> passes )
		{
			foreach ( IRenderPass renderPass in passes )
			{
				if ( !renderPass.Enabled )
				{
					continue;
				}
				renderPass.Render( renderParams );
			}
		}

		void AddRenderPhase( RenderPhase renderPhase )
		{
			Debug.Assert( passesByPhase.ContainsKey( renderPhase ) == false, "RenderPhase already added" );
			passesByPhase.Add( renderPhase, new List<IRenderPass>() );
		}

		#region IMultiTypeObject implementation

		static readonly Type[] types = { typeof(DeferredRenderer), typeof(IRenderer) };
		public Type[] Types { get { return types; } }

		#endregion
	}
}

