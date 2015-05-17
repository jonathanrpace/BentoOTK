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
using OpenTK.Input;

namespace Kaiga.Core
{
	public class DeferredRenderer : NamedObject, IRenderer, IDisposable, IMultiTypeObject
	{
		// Private
		Scene												scene;
		readonly RenderParams 								renderParams;
		readonly List<IRenderPass>							renderPasses;
		readonly Dictionary<RenderPhase, List<IRenderPass>> passesByPhase;
		readonly Dictionary<Type, IRenderPass>				passesByType; 

		// Shaders
		readonly TextureRectToScreenShader 					textureRectOutputShader;
		readonly Texture2DToScreenShader 					texture2DOutputShader;
		readonly LightTransportShader				 		lightTransportShader;
		readonly ResolveShader						 		resolveShader;
		//readonly ScreenSpaceReflectionShader				screenSpaceReflectionShader;

		// Helpers
		readonly TextureRectToMippedTexture2DHelper 		directLightTextureMipper;
		readonly TextureRectToMippedTexture2DHelper 		indirectLightTextureMipper;
		readonly TextureRectToMippedTexture2DHelper 		positionTextureMipper;	
		readonly TextureRectToMippedTexture2DHelper 		normalTextureMipper;

		// Properties
		public Entity							Camera { get; private set; }
		public IEnumerable<IRenderPass>			RenderPasses { get { return renderPasses.Skip( 0 ); } }
		public BackBufferOutputMode 			BackBufferOutputMode { get; set; }

		// Events
		public event RenderPassDelegate 		OnRenderPassAdded;
		public event RenderPassDelegate 		OnRenderPassRemoved;

		public DeferredRenderer() : this( "Deferred Renderer" )
		{
		}

		public DeferredRenderer( string name ) : base( name )
		{
			renderParams = new RenderParams();
			renderPasses = new List<IRenderPass>();
			passesByPhase = new Dictionary<RenderPhase, List<IRenderPass>>();
			passesByType = new Dictionary<Type, IRenderPass>();

			Camera = new Entity();
			var lens = new PerspectiveLens();
			Camera.AddComponent( lens );
			var cameraTransform = new Transform( Matrix4.Identity * Matrix4.CreateTranslation( 0.0f, 0.0f, -2.0f ) );
			Camera.AddComponent( cameraTransform );

			AddRenderPhase( RenderPhase.G );
			AddRenderPhase( RenderPhase.DirectLight );
			AddRenderPhase( RenderPhase.IndirectLight );
			AddRenderPhase( RenderPhase.PostLight );

			textureRectOutputShader = new TextureRectToScreenShader();
			texture2DOutputShader = new Texture2DToScreenShader();
			directLightTextureMipper = new TextureRectToMippedTexture2DHelper();
			indirectLightTextureMipper = new TextureRectToMippedTexture2DHelper();
			positionTextureMipper = new TextureRectToMippedTexture2DHelper();
			normalTextureMipper = new TextureRectToMippedTexture2DHelper();
			lightTransportShader = new LightTransportShader();
			resolveShader = new ResolveShader();
			//screenSpaceReflectionShader = new ScreenSpaceReflectionShader();
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

			renderParams.Dispose();

			textureRectOutputShader.Dispose();
			directLightTextureMipper.Dispose();
			indirectLightTextureMipper.Dispose();
			lightTransportShader.Dispose();
			resolveShader.Dispose();
			//screenSpaceReflectionShader.Dispose();
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

		public void Update( double dt )
		{
			RenderToBackBuffer();
		}

		public void Render()
		{

		}

		public void RenderToBackBuffer()
		{
			//float scalar = Math.Max( 0.2f, (float)Mouse.GetState().X / 1000.0f );
			//Debug.WriteLine( scalar );
			//const float scalar = 1.0f;
			//renderParams.LightTransportResolutionScalar = scalar;

			DeferredRenderTarget renderTarget = renderParams.RenderTarget;
			LightTransportRenderTarget lightTransportRenderTarget = renderParams.LightTransportRenderTarget;

			int renderWidth = scene.GameWindow.Width;
			int renderHeight = scene.GameWindow.Height;
			renderTarget.SetSize( renderWidth, renderHeight );
			renderParams.BackBufferWidth = renderWidth;
			renderParams.BackBufferHeight = renderHeight;

			int lightTransportRenderWidth = (int)( renderWidth * renderParams.LightTransportResolutionScalar );
			int lightTransportRenderHeight = (int)( renderHeight * renderParams.LightTransportResolutionScalar );
			lightTransportRenderTarget.SetSize( lightTransportRenderWidth, lightTransportRenderHeight );
			renderParams.LightTransportBufferWidth = lightTransportRenderWidth;
			renderParams.LightTransportBuffferHeight = lightTransportRenderHeight;

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

			// Setup some default render states
			GL.Viewport( 0, 0, renderWidth, renderHeight );
			renderTarget.Clear();
			GL.Enable( EnableCap.CullFace );
			GL.CullFace( CullFaceMode.Back );
			GL.Disable( EnableCap.Blend );

			// Geometry pass

			renderTarget.BindForGPhase();
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask( true );
			RenderPassesInPhase( RenderPhase.G );

			// No more depth writing
			GL.DepthMask( false );		

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
			GL.Disable( EnableCap.DepthTest );

			// Downsample direct and indirect buffers into 2D mipmapped textures
			directLightTextureMipper.Render( renderParams, renderTarget.DirectLightBuffer );
			renderParams.DirectLightTexture2D = directLightTextureMipper.Output;
			indirectLightTextureMipper.Render( renderParams, renderTarget.IndirectLightBuffer );
			renderParams.IndirectLightTexture2D = indirectLightTextureMipper.Output;

			// Convert position and normal buffers to 2D mipped textures
			// These are used during resolve pass to provide cache performant scalable AO and radiosity.
			positionTextureMipper.Render( renderParams, renderTarget.PositionBuffer );
			renderParams.PositionTexture2D = positionTextureMipper.Output;
			normalTextureMipper.Render( renderParams, renderTarget.NormalBuffer );
			renderParams.NormalTexture2D = normalTextureMipper.Output;

			// Perform light transport.
			GL.Viewport( 0, 0, lightTransportRenderWidth, lightTransportRenderHeight );
			lightTransportShader.Render( renderParams );
			
			// Resolve
			GL.Viewport( 0, 0, renderWidth, renderHeight );
			renderTarget.BindForResolvePhase();
			resolveShader.Render( renderParams );

			// Switch draw target to back buffer
			GL.Viewport( 0, 0, renderWidth, renderHeight );
			GL.DepthMask( true );
			GL.Disable( EnableCap.DepthTest );
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, 0 );
			//GL.Enable( EnableCap.FramebufferSrgb );

			// Output final output texture to backbuffer
			//screenSpaceReflectionShader.Render( renderParams );]

			//textureRectOutputShader.Render( renderParams, renderParams.DirectLightTextureRect.Texture );
			//texture2DOutputShader.Render( renderParams, aoRenderTarget.AOBuffer.Texture );
			textureRectOutputShader.Render( renderParams, renderTarget.OutputBuffer.Texture );
			//texture2DOutputShader.Render( renderParams, renderParams.AORenderTarget.AOBuffer.Texture );

			scene.GameWindow.SwapBuffers();
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

		static readonly Type[] types = { typeof(DeferredRenderer), typeof(IRenderer) };
		public Type[] Types { get { return types; } }
	}
}

