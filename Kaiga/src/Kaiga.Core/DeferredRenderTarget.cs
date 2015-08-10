using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using Kaiga.Textures;

namespace Kaiga.Core
{
	public class DeferredRenderTarget : AbstractRenderTarget
	{
		public RectangleTexture NormalBuffer { get; private set; }
		public RectangleTexture PositionBuffer { get; private set; }
		public RectangleTexture AlbedoBuffer { get; private set; }
		public RectangleTexture MaterialBuffer { get; private set; }
		public RectangleTexture DirectLightBuffer { get; private set; }
		public RectangleTexture IndirectLightBuffer { get; private set; }
		public RectangleTexture OutputBuffer { get; private set; }
		public RectangleTexture DepthBuffer { get; private set; }

		DrawBuffersEnum[] gPhaseDrawBuffers = { 
			DrawBufferName.Position, 
			DrawBufferName.Normal, 
			DrawBufferName.Albedo, 
			DrawBufferName.Material,
			DrawBufferName.DirectLight
		};

		DrawBuffersEnum[] directLightPhaseDrawBuffers = { 
			DrawBufferName.DirectLight
		};

		DrawBuffersEnum[] indirectLightPhaseDrawBuffers = { 
			DrawBufferName.IndirectLight
		};

		DrawBuffersEnum[] resolvePhaseDrawBuffers = { 
			DrawBufferName.Output
		};

		DrawBuffersEnum[] allDrawBuffers = { 
			DrawBufferName.Position, 
			DrawBufferName.Normal, 
			DrawBufferName.Albedo, 
			DrawBufferName.Material,
			DrawBufferName.DirectLight,
			DrawBufferName.IndirectLight,
			DrawBufferName.Output
		};

		public DeferredRenderTarget() :
		base( PixelInternalFormat.Rgba16f, true, true )
		{
			PositionBuffer = new RectangleTexture( internalFormat );
			NormalBuffer = new RectangleTexture( internalFormat );
			AlbedoBuffer = new RectangleTexture( internalFormat );
			MaterialBuffer = new RectangleTexture( internalFormat );
			DirectLightBuffer = new RectangleTexture( internalFormat );
			IndirectLightBuffer = new RectangleTexture( internalFormat );
			OutputBuffer = new RectangleTexture( internalFormat );
			DepthBuffer = new RectangleTexture( PixelInternalFormat.DepthComponent24, 256, 256, PixelFormat.DepthComponent );
			DepthBuffer.MagFilter = TextureMagFilter.Nearest;
			DepthBuffer.MinFilter = TextureMinFilter.Nearest;

			AttachTexture( FBAttachmentName.Position, PositionBuffer );
			AttachTexture( FBAttachmentName.Normal, NormalBuffer );
			AttachTexture( FBAttachmentName.Albedo, AlbedoBuffer );
			AttachTexture( FBAttachmentName.Material, MaterialBuffer );
			AttachTexture( FBAttachmentName.DirectLight, DirectLightBuffer );
			AttachTexture( FBAttachmentName.IndirectLight, IndirectLightBuffer );
			AttachTexture( FBAttachmentName.Output, OutputBuffer );
			AttachTexture( FramebufferAttachment.DepthAttachment, DepthBuffer );
		}

		public void Clear()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( allDrawBuffers.Length, allDrawBuffers );
			GL.ClearColor( Color.Black );
			GL.ClearDepth( 1.0 );
			GL.ClearStencil( 0 );
			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit );
		}

		public void BindForGPhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( gPhaseDrawBuffers.Length, gPhaseDrawBuffers );
		}

		public void BindForDirectLightPhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( directLightPhaseDrawBuffers.Length, directLightPhaseDrawBuffers );
		}

		public void BindForIndirectLightPhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( indirectLightPhaseDrawBuffers.Length, indirectLightPhaseDrawBuffers );
		}

		public void BindForResolvePhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( resolvePhaseDrawBuffers.Length, resolvePhaseDrawBuffers );
		}

		public void BindForNoDraw()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffer( DrawBufferMode.None );
		}
	}
}

