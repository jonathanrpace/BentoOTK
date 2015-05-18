using Kaiga.Textures;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Core
{
	public class LightTransportRenderTarget : AbstractRenderTarget
	{
		public RectangleTexture RadiosityAndAOTextureRect { get; private set; }
		public RectangleTexture ReflectionsTextureRect { get; private set; }
		public RectangleTexture BlurBufferTextureRect { get; private set; }

		readonly DrawBuffersEnum[] lightTransportDrawBuffers = 
		{ 
			DrawBuffersEnum.ColorAttachment0,
			DrawBuffersEnum.ColorAttachment1
		};

		readonly DrawBuffersEnum[] blurADrawBuffers = 
		{ 
			DrawBuffersEnum.ColorAttachment2
		};

		readonly DrawBuffersEnum[] blurBDrawBuffers = 
		{ 
			DrawBuffersEnum.ColorAttachment0
		};

		public LightTransportRenderTarget() :
		base( PixelInternalFormat.Rgba16f, true, false )
		{
			RadiosityAndAOTextureRect = new RectangleTexture( internalFormat );
			ReflectionsTextureRect = new RectangleTexture( internalFormat );
			BlurBufferTextureRect = new RectangleTexture( internalFormat );
			RadiosityAndAOTextureRect.MagFilter = TextureMagFilter.Linear;
			BlurBufferTextureRect.MagFilter = TextureMagFilter.Linear;
			AttachTexture( FramebufferAttachment.ColorAttachment0, RadiosityAndAOTextureRect );
			AttachTexture( FramebufferAttachment.ColorAttachment1, ReflectionsTextureRect );

			AttachTexture( FramebufferAttachment.ColorAttachment2, BlurBufferTextureRect );
		}

		public void BindForLightTransport()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( lightTransportDrawBuffers.Length, lightTransportDrawBuffers );
		}

		public void BindForBlurA()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( blurADrawBuffers.Length, blurADrawBuffers );
		}

		public void BindForBlurB()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( blurBDrawBuffers.Length, blurBDrawBuffers );
		}
	}
}

