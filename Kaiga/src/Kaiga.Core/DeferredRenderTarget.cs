using System;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

namespace Kaiga.Core
{
	public class DeferredRenderTarget : IRenderTarget
	{
		public int Width { get; private set; }
		public int Height { get; private set; }

		public int FrameBuffer { get; private set; }
		public int DepthBuffer { get; private set; }
		public int NormalBuffer { get; private set; }
		public int PositionBuffer { get; private set; }
		public int AlbedoBuffer { get; private set; }
		public int MaterialBuffer { get; private set; }
		public int OutputBuffer { get; private set; }
		public int PostBuffer { get; private set; }

		Dictionary<FramebufferAttachment,int> textureByFrameBufferAttachment;

		DrawBuffersEnum[] gPhaseDrawBuffers = { 
			DrawBufferName.Normal, 
			DrawBufferName.Position, 
			DrawBufferName.Albedo, 
			DrawBufferName.Material
		};

		DrawBuffersEnum[] lightPhaseDrawBuffers = { 
			DrawBufferName.Output
		};

		DrawBuffersEnum[] allDrawBuffers = { 
			DrawBufferName.Normal, 
			DrawBufferName.Position, 
			DrawBufferName.Albedo, 
			DrawBufferName.Material,
			DrawBufferName.Output,
			DrawBufferName.Post
		};
		
		public void CreateGraphicsContextResources()
		{
			textureByFrameBufferAttachment = new Dictionary<FramebufferAttachment, int>();

			FrameBuffer = GL.GenFramebuffer();
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, FrameBuffer );

			NormalBuffer = InitTextureBuffer( FBAttachmentName.Normal );
			PositionBuffer = InitTextureBuffer( FBAttachmentName.Position );
			AlbedoBuffer = InitTextureBuffer( FBAttachmentName.Albedo );
			MaterialBuffer = InitTextureBuffer( FBAttachmentName.Material );
			OutputBuffer = InitTextureBuffer( FBAttachmentName.Output );
			PostBuffer = InitTextureBuffer( FBAttachmentName.Post );

			// Depth -> Depth
			DepthBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, DepthBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height );
			GL.FramebufferRenderbuffer( FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, DepthBuffer );

			var status = GL.CheckFramebufferStatus( FramebufferTarget.Framebuffer );

			if ( status != FramebufferErrorCode.FramebufferComplete )
			{
				Debug.WriteLine( status );
			}

			GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 );
		}

		public void DisposeGraphicsContextResources()
		{
			// Dispose existing frameBuffer
			if ( GL.IsFramebuffer( FrameBuffer ) )
			{
				GL.DeleteFramebuffer( FrameBuffer );

				GL.DeleteRenderbuffer( DepthBuffer );

				GL.DeleteTexture( NormalBuffer );
				GL.DeleteTexture( PositionBuffer );
				GL.DeleteTexture( AlbedoBuffer );
				GL.DeleteTexture( MaterialBuffer );
				GL.DeleteTexture( OutputBuffer );
				GL.DeleteTexture( PostBuffer );
			}
		}

		public void SetSize( int width, int height )
		{
			if ( width == Width && height == Height )
			{
				return;
			}

			Width = width;
			Height = height;

			DisposeGraphicsContextResources();
			CreateGraphicsContextResources();
		}

		public void Clear()
		{
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( allDrawBuffers.Length, allDrawBuffers );
			GL.ClearColor( Color.Black );
			GL.ClearDepth( 1.0 );
			GL.ClearStencil( 0 );
			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit );
		}

		public void BindForGPhase()
		{
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( gPhaseDrawBuffers.Length, gPhaseDrawBuffers );
		}

		public void BindForLightPhase()
		{
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( lightPhaseDrawBuffers.Length, lightPhaseDrawBuffers );
		}

		public void BindForNoDraw()
		{
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffer( DrawBufferMode.None );
		}
		
		public void Unbind()
		{
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, 0 );
		}

		public int GetTexture( FramebufferAttachment fba )
		{
			return textureByFrameBufferAttachment[ fba ];
		}
		
		int InitTextureBuffer( FramebufferAttachment frameBufferAttachment )
		{
			var texture = GL.GenTexture();
			textureByFrameBufferAttachment.Add( frameBufferAttachment, texture );

			GL.BindTexture( TextureTarget.TextureRectangle, texture );
			GL.TexImage2D( TextureTarget.TextureRectangle, 0, PixelInternalFormat.Rgba32f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, new IntPtr(0) );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge );
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, frameBufferAttachment, TextureTarget.TextureRectangle, texture, 0 );

			return texture;
		}
	}
}

