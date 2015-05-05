using System;
using Kaiga.RenderPasses;
using Ramen;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Components;
using OpenTK;
using Kaiga.Shaders;

namespace Kaiga.Core
{
	public class DeferredRenderer : NamedObject, IRenderer, IDisposable, IMultiTypeObject
	{
		private Scene										scene;
		private RenderParams 								renderParams;
		private List<IRenderPass>							renderPasses;
		private Dictionary<RenderPhase, List<IRenderPass>> 	passesByPhase;
		private Dictionary<Type, IRenderPass>				passesByType;
		private DeferredRenderTarget						renderTarget;
		private AORenderTarget								aoRenderTarget;

		public Entity									Camera { get; private set; }
		public IEnumerable<IRenderPass>					RenderPasses { get { return renderPasses.Skip( 0 ); } }
		public BackBufferOutputMode 					BackBufferOutputMode { get; set; }

		public event RenderPassDelegate OnRenderPassAdded;
		public event RenderPassDelegate OnRenderPassRemoved;

		readonly ScreenQuadTextureRectShader textureOutputShader;
		readonly ScreenQuadTextureShader squareTextureOutputShader;

		readonly LightBufferConvolver directLightBufferDownsampler;
		readonly LightBufferConvolver indirectLightBufferDownsampler;

		public DeferredRenderer() : this( "Deferred Renderer" )
		{

		}

		public DeferredRenderer( string name ) : base( name )
		{
			renderParams = new RenderParams();
			renderPasses = new List<IRenderPass>();
			passesByPhase = new Dictionary<RenderPhase, List<IRenderPass>>();
			passesByType = new Dictionary<Type, IRenderPass>();

			renderTarget = renderParams.RenderTarget = new DeferredRenderTarget();
			aoRenderTarget = renderParams.AORenderTarget = new AORenderTarget();

			Camera = new Entity();
			var lens = new PerspectiveLens();
			Camera.AddComponent( lens );
			var cameraTransform = new Transform( Matrix4.Identity * Matrix4.CreateTranslation( 0.0f, 0.0f, -2.0f ) );
			Camera.AddComponent( cameraTransform );

			AddRenderPhase( RenderPhase.G );
			AddRenderPhase( RenderPhase.DirectLight );
			AddRenderPhase( RenderPhase.IndirectLight );
			AddRenderPhase( RenderPhase.AO );
			AddRenderPhase( RenderPhase.Resolve );

			textureOutputShader = new ScreenQuadTextureRectShader();
			squareTextureOutputShader = new ScreenQuadTextureShader();
			directLightBufferDownsampler = new LightBufferConvolver();
			indirectLightBufferDownsampler = new LightBufferConvolver();

			GL.Enable( EnableCap.FramebufferSrgb );
		}

		public void Dispose()
		{
			foreach ( var renderPass in renderPasses )
			{
				renderPass.Dispose();
			}

			renderPasses.Clear();
			passesByPhase.Clear();
			passesByType.Clear();

			renderTarget.Dispose();
			textureOutputShader.Dispose();
			directLightBufferDownsampler.Dispose();
			indirectLightBufferDownsampler.Dispose();
		}
		
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

		public void RenderToBackBuffer( DeferredRenderTarget renderTarget )
		{
			renderTarget.SetSize( scene.GameWindow.Width, scene.GameWindow.Height );
			aoRenderTarget.SetSize( scene.GameWindow.Width >> 1, scene.GameWindow.Height >> 1 );

			GL.Viewport( 0, 0, scene.GameWindow.Width, scene.GameWindow.Height );

			renderParams.CameraLens = Camera.GetComponentByType<ILens>();
			renderParams.CameraLens.AspectRatio = (float)scene.GameWindow.Width / scene.GameWindow.Height;
			renderParams.ViewMatrix = Camera.GetComponentByType<Transform>().Matrix;
			renderParams.NormalViewMatrix = renderParams.ViewMatrix.ClearScale();
			renderParams.NormalViewMatrix = renderParams.NormalViewMatrix.ClearTranslation();
			renderParams.InvViewMatrix = renderParams.ViewMatrix.Inverted();
			renderParams.ProjectionMatrix = renderParams.CameraLens.ProjectionMatrix;
			renderParams.InvProjectionMatrix = renderParams.CameraLens.InvProjectionMatrix;
			renderParams.ViewProjectionMatrix = renderParams.ViewMatrix * renderParams.ProjectionMatrix;
			renderParams.InvViewProjectionMatrix = renderParams.ViewProjectionMatrix.Inverted();
			renderParams.NormalViewProjectionMatrix = renderParams.NormalViewMatrix * renderParams.ProjectionMatrix;
			renderParams.NormalInvViewMatrix = new Matrix3( renderParams.InvViewMatrix.ClearTranslation() );
				
			GL.Enable( EnableCap.CullFace );
			GL.CullFace( CullFaceMode.Back );

			renderTarget.Clear();

			// Geometry pass
			renderTarget.BindForGPhase();
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask( true );
			GL.Disable( EnableCap.Blend );
			RenderPassesInPhase( RenderPhase.G );

			// No more depth writing
			GL.DepthMask( false );		

			// AO pass
			aoRenderTarget.BindForAOPhase();
			RenderPassesInPhase( RenderPhase.AO );

			// Light passes are additive
			GL.Enable( EnableCap.Blend );
			GL.BlendFunc( BlendingFactorSrc.One, BlendingFactorDest.One );
			GL.BlendEquation( BlendEquationMode.FuncAdd );
			
			// Direct Light pass
			renderTarget.BindForDirectLightPhase();
			RenderPassesInPhase( RenderPhase.DirectLight );

			// Indirect light pass
			renderTarget.BindForIndirectLightPhase();
			RenderPassesInPhase( RenderPhase.IndirectLight );

			// No more light passes, back to no blending
			GL.Disable( EnableCap.Blend );

			// No more geometry - just full screen compositing. No need for depth test
			GL.Disable(EnableCap.DepthTest);

			// Downsample direct and indirect buffers into 2D mipmapped textures
			directLightBufferDownsampler.Render( renderParams, renderParams.RenderTarget.DirectLightBuffer );
			indirectLightBufferDownsampler.Render( renderParams, renderParams.RenderTarget.IndirectLightBuffer );


			// Resolve
			GL.Viewport( 0, 0, scene.GameWindow.Width, scene.GameWindow.Height );
			renderTarget.BindForResolvePhase();
			RenderPassesInPhase( RenderPhase.Resolve );
			
			// Switch draw target to back buffer
			GL.DepthMask( true );
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, 0 );

			// Output final output texture to backbuffer
			//squareTextureOutputShader.Render( renderParams, directLightBufferDownsampler.Output.Texture );
			textureOutputShader.Render( renderParams, renderTarget.OutputBuffer.Texture );
			//textureOutputShader.Render( renderParams, renderParams.AORenderTarget.AOBuffer.Texture );

			scene.GameWindow.SwapBuffers();
		}


		void RenderPassesInPhase( RenderPhase phase )
		{
			var passes = passesByPhase[ phase ];
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

