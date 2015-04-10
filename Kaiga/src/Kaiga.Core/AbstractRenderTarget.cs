using System;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using Kaiga.Textures;
using System.Collections.Generic;

namespace Kaiga.Core
{
	public class AbstractRenderTarget : AbstractValidatable, IRenderTarget
	{
		int frameBuffer;
		public int FrameBuffer
		{
			get
			{
				validate();
				return frameBuffer;
			}
		}

		protected readonly bool isRectangular;
		protected readonly bool hasDepthStencil;
		protected readonly PixelInternalFormat internalFormat;
		protected readonly RenderbufferStorage depthStencilFormat;
		readonly Dictionary<FramebufferAttachment, ITexture> texturesByAttachment;

		int width = 256;
		int height = 256;
		int depthBuffer;

		public AbstractRenderTarget
		( 
			PixelInternalFormat internalFormat, 
			bool isRectangular,
			bool hasDepthStencil, 
			RenderbufferStorage depthStencilFormat = RenderbufferStorage.Depth24Stencil8 
		)
		{
			this.internalFormat = internalFormat;
			this.isRectangular = isRectangular;
			this.hasDepthStencil = hasDepthStencil;
			this.depthStencilFormat = depthStencilFormat;

			texturesByAttachment = new Dictionary<FramebufferAttachment, ITexture>();
		}

		public void SetSize( int width, int height )
		{
			if ( this.width == width && this.height == height )
				return;
			
			this.width = width;
			this.height = height;
			invalidate();
		}

		protected void AttachTexture( FramebufferAttachment attachment, RectangleTexture texture )
		{
			Debug.Assert( isRectangular );
			texturesByAttachment[ attachment ] = texture;
		}

		protected override void onValidate()
		{
			frameBuffer = GL.GenFramebuffer();
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, frameBuffer );

			foreach ( var attachment in texturesByAttachment.Keys )
			{
				var texture = texturesByAttachment[ attachment ];
				texture.SetSize( width, height );
				GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, attachment, TextureTarget.TextureRectangle, texture.Texture, 0 );
			}

			if ( hasDepthStencil )
			{
				depthBuffer = GL.GenRenderbuffer();
				GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, depthBuffer );
				GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, depthStencilFormat, width, height );
				GL.FramebufferRenderbuffer( FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, depthBuffer );
			}

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

		protected override void onInvalidate()
		{
			if ( GL.IsFramebuffer( frameBuffer ) )
			{
				GL.DeleteFramebuffer( frameBuffer );
			}

			if ( GL.IsRenderbuffer( depthBuffer ) )
			{
				GL.DeleteRenderbuffer( depthBuffer );
			}
		}
	}
}

