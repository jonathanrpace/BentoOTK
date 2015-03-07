using System;

namespace Kaiga.Core
{
	public interface IRenderTarget
	{
		int FrameBuffer{ get; }
		int NormalBuffer { get; }
		int PositionBuffer { get; }
		int DepthBuffer { get; }
		int LBuffer { get; }
		int MaterialBuffer { get; }
		int PostBuffer { get; }
	}
}

