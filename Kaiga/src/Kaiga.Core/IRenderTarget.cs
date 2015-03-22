using System;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Core
{
	public interface IRenderTarget : IGraphicsContextDependant
	{
		int FrameBuffer{ get; }
		int GetTexture( FramebufferAttachment fba );
	}
}

