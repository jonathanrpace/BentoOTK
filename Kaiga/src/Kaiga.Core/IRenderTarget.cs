using System;

namespace Kaiga.Core
{
	public interface IRenderTarget : IGraphicsContextDependant
	{
		int FrameBuffer{ get; }
		int NormalBuffer{ get; }
		int PositionBuffer{ get; }
		int AlbedoBuffer{ get; }
		void Bind();
	}
}

