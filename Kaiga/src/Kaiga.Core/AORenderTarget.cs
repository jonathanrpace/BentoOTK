using Kaiga.Textures;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Core
{
	public class AORenderTarget : AbstractRenderTarget
	{
		public RectangleTexture AOBuffer { get; private set; }
		public RectangleTexture AOBlurBuffer { get; private set; }

		readonly DrawBuffersEnum[] aoPhaseDrawBuffers = 
		{ 
			DrawBufferName.AO
		};

		readonly DrawBuffersEnum[] aoBlurAPhaseDrawBuffers = 
		{ 
			DrawBufferName.AOBlur
		};

		readonly DrawBuffersEnum[] aoBlurBPhaseDrawBuffers = 
		{ 
			DrawBufferName.AO
		};

		public AORenderTarget() :
		base( PixelInternalFormat.Rgba16f, true, false )
		{
			AOBuffer = new RectangleTexture( internalFormat );
			AOBlurBuffer = new RectangleTexture( internalFormat );

			AttachTexture( FBAttachmentName.AO, AOBuffer );
			AttachTexture( FBAttachmentName.AOBlur, AOBlurBuffer );
		}

		public void BindForAOPhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( aoPhaseDrawBuffers.Length, aoPhaseDrawBuffers );
		}

		public void BindForBlurA()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( aoBlurAPhaseDrawBuffers.Length, aoBlurAPhaseDrawBuffers );
		}

		public void BindForBlurB()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( aoBlurBPhaseDrawBuffers.Length, aoBlurBPhaseDrawBuffers );
		}
	}
}

