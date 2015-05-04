using Kaiga.Core;

namespace Kaiga.RenderPasses
{
	public class AbstractRenderPass
	{
		protected bool enabled = true;
		readonly RenderPhase renderPhase;

		public AbstractRenderPass( RenderPhase renderPhase )
		{
			this.renderPhase = renderPhase;
		}
		
		public RenderPhase RenderPhase{ get { return renderPhase; } }
		public bool Enabled{ get { return enabled; } }
	}
}

