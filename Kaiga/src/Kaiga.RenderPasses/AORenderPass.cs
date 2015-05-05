using Kaiga.Core;
using Ramen;
using Kaiga.Shaders;

namespace Kaiga.RenderPasses
{
	public class AORenderPass : AbstractRenderPass, IRenderPass
	{
		readonly SSAOShader shader;

		public AORenderPass() : base( RenderPhase.AO )
		{
			shader = new SSAOShader();
		}

		public void Dispose()
		{
			shader.Dispose();
		}
		
		public void OnAddedToScene( Scene scene )
		{
			
		}

		public void OnRemovedFromScene( Scene scene )
		{
			
		}

		public void Render( RenderParams renderParams )
		{
			shader.Render( renderParams );
		}
	}
}

