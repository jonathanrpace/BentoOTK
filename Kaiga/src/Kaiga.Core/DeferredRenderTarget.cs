using System;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using Kaiga.Textures;

namespace Kaiga.Core
{
	public class DeferredRenderTarget : AbstractValidatable, IRenderTarget
	{
		public int Width { get; private set; }
		public int Height { get; private set; }

		public int FrameBuffer { get; private set; }
		public int DepthBuffer { get; private set; }
		public RectangleTexture NormalBuffer { get; private set; }
		public RectangleTexture PositionBuffer { get; private set; }
		public RectangleTexture AlbedoBuffer { get; private set; }
		public RectangleTexture MaterialBuffer { get; private set; }
		public RectangleTexture OutputBuffer { get; private set; }
		public RectangleTexture PostBuffer { get; private set; }

		public int HalfResFrameBuffer { get; private set; }
		public RectangleTexture AOBuffer { get; private set; }

		Dictionary<FramebufferAttachment,RectangleTexture> textureByFrameBufferAttachment;

		DrawBuffersEnum[] gPhaseDrawBuffers = { 
			DrawBufferName.Normal, 
			DrawBufferName.Position, 
			DrawBufferName.Albedo, 
			DrawBufferName.Material
		};

		DrawBuffersEnum[] aoPhaseDrawBuffers = { 
			DrawBufferName.AO
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

		DrawBuffersEnum[] justPositionBuffer = { 
			DrawBufferName.Position
		};

		public DeferredRenderTarget()
		{
			NormalBuffer = new RectangleTexture( PixelInternalFormat.Rgba32f );
			PositionBuffer = new RectangleTexture( PixelInternalFormat.Rgba32f );
			AlbedoBuffer = new RectangleTexture( PixelInternalFormat.Rgba32f );
			MaterialBuffer = new RectangleTexture( PixelInternalFormat.Rgba32f );
			OutputBuffer = new RectangleTexture( PixelInternalFormat.Rgba32f );
			PostBuffer = new RectangleTexture( PixelInternalFormat.Rgba32f );

			AOBuffer = new RectangleTexture( PixelInternalFormat.R8 );
		}
		
		override protected void onValidate()
		{
			textureByFrameBufferAttachment = new Dictionary<FramebufferAttachment, RectangleTexture>();

			FrameBuffer = GL.GenFramebuffer();
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, FrameBuffer );


			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FBAttachmentName.Normal, TextureTarget.TextureRectangle, NormalBuffer.Texture, 0 );
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FBAttachmentName.Position, TextureTarget.TextureRectangle, PositionBuffer.Texture, 0 );
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FBAttachmentName.Albedo, TextureTarget.TextureRectangle, AlbedoBuffer.Texture, 0 );
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FBAttachmentName.Material, TextureTarget.TextureRectangle, MaterialBuffer.Texture, 0 );
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FBAttachmentName.Output, TextureTarget.TextureRectangle, OutputBuffer.Texture, 0 );
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FBAttachmentName.Post, TextureTarget.TextureRectangle, PostBuffer.Texture, 0 );

			// Depth -> Depth
			DepthBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, DepthBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height );
			GL.FramebufferRenderbuffer( FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, DepthBuffer );

			// Check status of frame buffer
			{
				var status = GL.CheckFramebufferStatus( FramebufferTarget.Framebuffer );
				if ( status != FramebufferErrorCode.FramebufferComplete )
				{
					Debug.WriteLine( status );
				}
			}
			
			// AO FrameBuffer
			HalfResFrameBuffer = GL.GenFramebuffer();
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, HalfResFrameBuffer );

			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FBAttachmentName.AO, TextureTarget.TextureRectangle, AOBuffer.Texture, 0 );

			// Check status of frame buffer
			{
				var status = GL.CheckFramebufferStatus( FramebufferTarget.Framebuffer );
				if ( status != FramebufferErrorCode.FramebufferComplete )
				{
					Debug.WriteLine( status );
				}
			}

			GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 );
		}

		override protected void onInvalidate()
		{
			// Dispose existing frameBuffer
			if ( GL.IsFramebuffer( FrameBuffer ) )
			{
				GL.DeleteFramebuffer( FrameBuffer );
				GL.DeleteRenderbuffer( DepthBuffer );
				GL.DeleteFramebuffer( HalfResFrameBuffer );
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

			NormalBuffer.SetSize( Width, Height );
			PositionBuffer.SetSize( Width, Height );
			AlbedoBuffer.SetSize( Width, Height );
			MaterialBuffer.SetSize( Width, Height );
			OutputBuffer.SetSize( Width, Height );
			PostBuffer.SetSize( Width, Height );

			AOBuffer.SetSize( Width >> 1, Height >> 1 );

			invalidate();
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


			GL.DrawBuffers( justPositionBuffer.Length, justPositionBuffer );
			GL.ClearColor( 0.0f, 0.0f, -9999999.0f, 0.0f );
			GL.Clear( ClearBufferMask.ColorBufferBit );


			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, HalfResFrameBuffer );
			GL.DrawBuffers( aoPhaseDrawBuffers.Length, aoPhaseDrawBuffers );
			GL.ClearColor( Color.Black );
			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit );
		}

		public void BindForGPhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( gPhaseDrawBuffers.Length, gPhaseDrawBuffers );
		}

		public void BindForLightPhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( lightPhaseDrawBuffers.Length, lightPhaseDrawBuffers );
		}

		public void BindForAOPhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, HalfResFrameBuffer );
			GL.DrawBuffers( aoPhaseDrawBuffers.Length, aoPhaseDrawBuffers );
		}

		public void BindForNoDraw()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffer( DrawBufferMode.None );
		}
		
		public void Unbind()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, 0 );
		}
		/*
		public int GetTexture( FramebufferAttachment fba )
		{
			validate();

			return textureByFrameBufferAttachment[ fba ].Texture;
		}
		*/
		/*
		int InitTextureBuffer( FramebufferAttachment frameBufferAttachment, PixelInternalFormat format = PixelInternalFormat.Rgba32f)
		{
			var texture = GL.GenTexture();
			textureByFrameBufferAttachment.Add( frameBufferAttachment, texture );

			GL.BindTexture( TextureTarget.TextureRectangle, texture );
			GL.TexImage2D( TextureTarget.TextureRectangle, 0, format, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, new IntPtr(0) );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge );
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, frameBufferAttachment, TextureTarget.TextureRectangle, texture, 0 );

			return texture;
		}

		int InitHalfResTextureBuffer( FramebufferAttachment frameBufferAttachment, PixelInternalFormat format = PixelInternalFormat.R8)
		{
			var texture = GL.GenTexture();
			textureByFrameBufferAttachment.Add( frameBufferAttachment, texture );

			GL.BindTexture( TextureTarget.TextureRectangle, texture );
			GL.TexImage2D( TextureTarget.TextureRectangle, 0, format, Width >> 1, Height >> 1, 0, PixelFormat.Rgba, PixelType.Float, new IntPtr(0) );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge );
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, frameBufferAttachment, TextureTarget.TextureRectangle, texture, 0 );

			return texture;
		}
		*/
	}
}

