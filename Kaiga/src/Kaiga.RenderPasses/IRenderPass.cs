using System;
using Ramen;
using Kaiga.Core;

namespace Kaiga.RenderPasses
{
	public interface IRenderPass
	{
		void OnAddedToScene( Scene scene );
		void OnRemovedFromScene( Scene scene );
		void Render( RenderParams renderParams );

		RenderPhase RenderPhase{ get; }
		bool Enabled{ get; }
	}
}

