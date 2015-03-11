using System;

namespace Kaiga.Core
{
	public interface IRenderTarget : IGraphicsContextDependant
	{
		int FrameBuffer{ get; }
		int NormalBuffer{ get; }
		void Bind();
	}
}

