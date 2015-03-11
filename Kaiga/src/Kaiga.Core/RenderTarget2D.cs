using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;

namespace Kaiga.Core
{
	public class RenderTarget2D : IRenderTarget
	{
		public int Width { get; private set; }
		public int Height { get; private set; }

		public int FrameBuffer
		{
			get
			{
				return frameBuffer;
			}
		}
			
		int frameBuffer;
		int depthBuffer;
		int normalBuffer;
		int positionBuffer;
		int lBuffer;
		int albedoBuffer;
		int outputBuffer;
		int postBuffer;

		bool invalid = true;
		
		public void CreateGraphicsContextResources()
		{
			Invalidate();
		}

		public void DisposeGraphicsContextResources()
		{
			// Dispose existing frameBuffer
			if ( GL.IsFramebuffer( frameBuffer ) )
			{
				GL.DeleteFramebuffer( frameBuffer );

				GL.DeleteRenderbuffer( depthBuffer );

				GL.DeleteTexture( normalBuffer );
				GL.DeleteTexture( positionBuffer );
				GL.DeleteTexture( lBuffer );
				GL.DeleteTexture( albedoBuffer );
				GL.DeleteTexture( outputBuffer );
				GL.DeleteTexture( postBuffer );
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
			
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, frameBuffer );
		}

		public void Unbind()
		{
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 );
		}

		void Invalidate()
		{
			DisposeGraphicsContextResources();
			invalid = true;
		}

		void Validate()
		{
			frameBuffer = GL.GenFramebuffer();
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, frameBuffer );

			// Normal -> Colour0
			normalBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.Texture2D, normalBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, normalBuffer, 0 );

			// Position -> Colour1
			positionBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.Texture2D, positionBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, positionBuffer, 0 );

			// Albedo -> Colour2
			albedoBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.Texture2D, albedoBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, albedoBuffer, 0 );

			// Light -> Colour3
			lBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.Texture2D, lBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, TextureTarget.Texture2D, lBuffer, 0 );

			// Output -> Colour4
			outputBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.Texture2D, outputBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment4, TextureTarget.Texture2D, outputBuffer, 0 );

			// Post -> Colour5
			postBuffer = GL.GenTexture();
			GL.BindTexture( TextureTarget.Texture2D, postBuffer );
			InitTexture();
			GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment5, TextureTarget.Texture2D, postBuffer, 0 );

			// Depth -> Depth
			depthBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, depthBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Width, Height );
			GL.FramebufferRenderbuffer( FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer );


			GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 );
		}

		void InitTexture()
		{
			GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, new IntPtr(0) );
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear );
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
		}
	}
}

