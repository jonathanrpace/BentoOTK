using System;
using Ramen;
using Kaiga.Core;

namespace Kaiga.RenderPasses
{
	public interface IRenderPass : IDisposable
	{
		void OnAddedToScene( Scene scene );
		void OnRemovedFromScene( Scene scene );
		void Render();

		RenderPhase RenderPhase{ get; }
		bool Enabled{ get; }
	}
}

