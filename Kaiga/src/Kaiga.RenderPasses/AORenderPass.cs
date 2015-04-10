using System;
using Kaiga.Core;
using Ramen;
using Kaiga.Shaders;

namespace Kaiga.RenderPasses
{
	public class AORenderPass : IRenderPass
	{
		readonly FFAOShader shader;

		public AORenderPass()
		{
			shader = new FFAOShader();
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
				return RenderPhase.AO;
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

