using Kaiga.Textures;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Core
{
	public class LightTransportRenderTarget
	{
		public RectangleTexture RadiosityAndAOTextureRectA { get; private set; }
		public RectangleTexture RadiosityAndAOTextureRectB { get; private set; }
		public RectangleTexture BlurredRadiosityAndAOTexture { get; private set; }
		public RectangleTexture ReflectionsTextureRectA { get; private set; }
		public RectangleTexture ReflectionsTextureRectB { get; private set; }
		public RectangleTexture BlurBufferTextureRect { get; private set; }

		readonly AbstractRenderTarget renderTargetA;
		readonly AbstractRenderTarget renderTargetB;
		AbstractRenderTarget currRenderTarget;

		readonly DrawBuffersEnum[] lightTransportDrawBuffers = 
		{ 
			DrawBuffersEnum.ColorAttachment0,
			DrawBuffersEnum.ColorAttachment1
		};

		readonly DrawBuffersEnum[] blurXDrawBuffers = 
		{ 
			DrawBuffersEnum.ColorAttachment2
		};

		readonly DrawBuffersEnum[] blurYDrawBuffers = 
		{ 
			DrawBuffersEnum.ColorAttachment3
		};

		readonly DrawBuffersEnum[] radiosityAndAODrawBuffers = 
		{ 
			DrawBuffersEnum.ColorAttachment0
		};

		readonly DrawBuffersEnum[] reflectionDrawBuffers = 
		{ 
			DrawBuffersEnum.ColorAttachment1
		};

		public LightTransportRenderTarget()
		{
			PixelInternalFormat internalFormat = PixelInternalFormat.Rgba16f;
			renderTargetA = new AbstractRenderTarget(internalFormat, true, false);
			renderTargetB = new AbstractRenderTarget(internalFormat, true, false);


			RadiosityAndAOTextureRectA = new RectangleTexture( internalFormat );
			RadiosityAndAOTextureRectA.MagFilter = TextureMagFilter.Linear;

			RadiosityAndAOTextureRectB = new RectangleTexture( internalFormat );
			RadiosityAndAOTextureRectB.MagFilter = TextureMagFilter.Linear;

			ReflectionsTextureRectA = new RectangleTexture( internalFormat );
			ReflectionsTextureRectA.MagFilter = TextureMagFilter.Linear;

			ReflectionsTextureRectB = new RectangleTexture( internalFormat );
			ReflectionsTextureRectB.MagFilter = TextureMagFilter.Linear;

			BlurBufferTextureRect = new RectangleTexture( internalFormat );
			BlurBufferTextureRect.MagFilter = TextureMagFilter.Linear;

			BlurredRadiosityAndAOTexture = new RectangleTexture( internalFormat );
			BlurredRadiosityAndAOTexture.MagFilter = TextureMagFilter.Linear;
			
			renderTargetA.AttachTexture( FramebufferAttachment.ColorAttachment0, RadiosityAndAOTextureRectA );
			renderTargetA.AttachTexture( FramebufferAttachment.ColorAttachment1, ReflectionsTextureRectA );
			renderTargetA.AttachTexture( FramebufferAttachment.ColorAttachment2, BlurBufferTextureRect );
			renderTargetA.AttachTexture( FramebufferAttachment.ColorAttachment3, BlurredRadiosityAndAOTexture );

			renderTargetB.AttachTexture( FramebufferAttachment.ColorAttachment0, RadiosityAndAOTextureRectB );
			renderTargetB.AttachTexture( FramebufferAttachment.ColorAttachment1, ReflectionsTextureRectB );
			renderTargetB.AttachTexture( FramebufferAttachment.ColorAttachment2, BlurBufferTextureRect );
			renderTargetB.AttachTexture( FramebufferAttachment.ColorAttachment3, BlurredRadiosityAndAOTexture );

			currRenderTarget = renderTargetA;
		}

		public void SetSize( int width, int height )
		{
			renderTargetA.SetSize( width, height );
			renderTargetB.SetSize( width, height );
		}

		public void BindForLightTransport()
		{
			currRenderTarget.Bind();
			GL.DrawBuffers( lightTransportDrawBuffers.Length, lightTransportDrawBuffers );
		}

		public void BindForRadiosityAndAOBlurX()
		{
			currRenderTarget.Bind();
			GL.DrawBuffers( blurXDrawBuffers.Length, blurXDrawBuffers );
		}

		public void BindForRadiosityAndAOBlurY()
		{
			currRenderTarget.Bind();
			GL.DrawBuffers( blurYDrawBuffers.Length, blurYDrawBuffers );
		}

		public void BindForReflectionBlurX()
		{
			currRenderTarget.Bind();
			GL.DrawBuffers( blurXDrawBuffers.Length, blurXDrawBuffers );
		}

		public void BindForReflectionBlurY()
		{
			currRenderTarget.Bind();
			GL.DrawBuffers( reflectionDrawBuffers.Length, reflectionDrawBuffers );
		}

		public void SwapRadiosityAndAOTextures()
		{
			currRenderTarget = currRenderTarget == renderTargetA ? renderTargetB : renderTargetA;
		}

		public RectangleTexture GetCurrentRadiosityAndAOTexture()
		{
			return currRenderTarget == renderTargetA ? RadiosityAndAOTextureRectA : RadiosityAndAOTextureRectB;
		}

		public RectangleTexture GetPreviousRadiosityAndAOTexture()
		{
			return currRenderTarget == renderTargetA ? RadiosityAndAOTextureRectB : RadiosityAndAOTextureRectA;
		}

		public RectangleTexture GetCurrentReflectionTexture()
		{
			return currRenderTarget == renderTargetA ? ReflectionsTextureRectA : ReflectionsTextureRectB;
		}

		public RectangleTexture GetPreviousReflectionTexture()
		{
			return currRenderTarget == renderTargetA ? ReflectionsTextureRectB : ReflectionsTextureRectA;
		}
	}
}

