using System;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Core
{
	public interface IRenderTarget : IDisposable
	{
		int FrameBuffer{ get; }
		//int GetTexture( FramebufferAttachment fba );
	}
}

