using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using System.Diagnostics;

namespace Kaiga.Core
{
	public class RenderTarget2D : IRenderTarget
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

		bool invalid = true;
		
		public void CreateGraphicsContextResources()
		{
			Invalidate();
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

			invalid = true;
		}

		public void SetSize( int width, int height )
		{
			if ( width == Width && height == Height )
			{
				return;
			}

			Width = width;
			Height = height;

			Invalidate();
		}

		public void Bind()
		{
			if ( invalid )
			{
				Validate();
				invalid = false;
			}
			
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );


		}

		public void Unbind()
		{
			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, 0 );
		}

		void Invalidate()
		{
			DisposeGraphicsContextResources();
			invalid = true;
		}

		void Validate()
		{
			FrameBuffer = GL.GenFramebuffer();
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, FrameBuffer );

			// Normal -> Colour0
			NormalBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.TextureRectangle, NormalBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureRectangle, NormalBuffer, 0 );

			// Position -> Colour1
			PositionBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.TextureRectangle, PositionBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.TextureRectangle, PositionBuffer, 0 );

			// Albedo -> Colour2
			AlbedoBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.TextureRectangle, AlbedoBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.TextureRectangle, AlbedoBuffer, 0 );

			// Material -> Colour3
			MaterialBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.TextureRectangle, MaterialBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, TextureTarget.TextureRectangle, MaterialBuffer, 0 );

			// Output -> Colour4
			OutputBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.TextureRectangle, OutputBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment4, TextureTarget.TextureRectangle, OutputBuffer, 0 );

			// Post -> Colour5
			PostBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.TextureRectangle, PostBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment5, TextureTarget.TextureRectangle, PostBuffer, 0 );

			// Depth -> Depth
			DepthBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, DepthBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Width, Height );
			GL.FramebufferRenderbuffer( FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthBuffer );



			var status = GL.CheckFramebufferStatus( FramebufferTarget.Framebuffer );

			if ( status != FramebufferErrorCode.FramebufferComplete )
			{
				Debug.WriteLine( status );
			}

			GL.DrawBuffers( 6, new [] {
				DrawBuffersEnum.ColorAttachment0,
				DrawBuffersEnum.ColorAttachment1,
				DrawBuffersEnum.ColorAttachment2,
				DrawBuffersEnum.ColorAttachment3,
				DrawBuffersEnum.ColorAttachment4,
				DrawBuffersEnum.ColorAttachment5
			} );

			GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 );
		}

		void InitTexture()
		{
			GL.TexImage2D( TextureTarget.TextureRectangle, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, new IntPtr(0) );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge );
			//GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMaxLevel, 0 );
		}
	}
}

