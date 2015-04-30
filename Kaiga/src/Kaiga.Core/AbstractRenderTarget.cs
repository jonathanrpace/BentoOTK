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
		readonly Dictionary<FramebufferAttachment, ITexture2D> texturesByAttachment;
		readonly Dictionary<FramebufferAttachment, int> levelByAttachment;

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

			texturesByAttachment = new Dictionary<FramebufferAttachment, ITexture2D>();
			levelByAttachment = new Dictionary<FramebufferAttachment, int>();
		}

		public void SetSize( int width, int height )
		{
			if ( this.width == width && this.height == height )
				return;

			Debug.Assert( isRectangular || width == height );
				
			this.width = width;
			this.height = height;
			invalidate();
		}


		public void Bind()
		{
			validate();

			GL.Viewport( 0, 0, width, height );

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( 1, new [] { DrawBuffersEnum.ColorAttachment0 } );
		}

		public void AttachTexture( FramebufferAttachment attachment, RectangleTexture texture, int level = 0 )
		{
			Debug.Assert( isRectangular );
			texturesByAttachment[ attachment ] = texture;
			levelByAttachment[ attachment ] = level;
		}

		public void AttachTexture( FramebufferAttachment attachment, SquareTexture2D texture, int level = 0 )
		{
			Debug.Assert( !isRectangular );
			texturesByAttachment[ attachment ] = texture;
			levelByAttachment[ attachment ] = level;
		}

		protected override void onValidate()
		{
			frameBuffer = GL.GenFramebuffer();
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, frameBuffer );

			TextureTarget textureTarget = isRectangular ? TextureTarget.TextureRectangle : TextureTarget.Texture2D;
			foreach ( var attachment in texturesByAttachment.Keys )
			{
				var texture = texturesByAttachment[ attachment ];
				var level = levelByAttachment[ attachment ];
				texture.SetSize( width << level, height << level );
				GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, attachment, textureTarget, texture.Texture, level );
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

