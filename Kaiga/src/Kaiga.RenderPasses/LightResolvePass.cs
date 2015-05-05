using Kaiga.Core;
using Ramen;
using Kaiga.Shaders;

namespace Kaiga.RenderPasses
{
	public class LightResolvePass : AbstractRenderPass, IRenderPass
	{
		readonly LightResolveShader shader;

		public LightResolvePass() : base( RenderPhase.Resolve )
		{
			shader = new LightResolveShader();
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

