using System;
using Kaiga.Textures;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Core
{
	public class AORenderTarget : AbstractRenderTarget
	{
		public RectangleTexture AOBuffer { get; private set; }

		readonly DrawBuffersEnum[] aoPhaseDrawBuffers = { 
			DrawBufferName.AO
		};

		public AORenderTarget() :
		base( OpenTK.Graphics.OpenGL4.PixelInternalFormat.R8, true, false )
		{
			AOBuffer = new RectangleTexture( internalFormat );
			AttachTexture( FBAttachmentName.AO, AOBuffer );
		}

		public void BindForAOPhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( aoPhaseDrawBuffers.Length, aoPhaseDrawBuffers );
		}
	}
}

