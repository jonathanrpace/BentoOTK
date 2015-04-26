using System;
using Kaiga.Core;
using Ramen;
using Kaiga.Shaders;

namespace Kaiga.RenderPasses
{
	public class LightResolvePass : IRenderPass
	{
		readonly LightResolveShader shader;

		public LightResolvePass()
		{
			shader = new LightResolveShader();
		}

		public void Dispose()
		{
			shader.Dispose();
		}

		#region IRenderPass implementation

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

		public RenderPhase RenderPhase
		{
			get
			{
				return RenderPhase.Resolve;
			}
		}

		public bool Enabled
		{
			get
			{
				return true;
			}
		}

		#endregion
	}
}

