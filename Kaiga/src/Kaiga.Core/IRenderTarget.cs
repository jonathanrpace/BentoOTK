using System;

namespace Kaiga.Core
{
	public interface IRenderTarget : IDisposable
	{
		int FrameBuffer{ get; }
	}
}

