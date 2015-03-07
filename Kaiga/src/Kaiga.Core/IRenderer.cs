using System.Collections.Generic;
using Kaiga.RenderPasses;
using Ramen;

namespace Kaiga.Core
{
	public delegate void RenderPassDelegate( IRenderPass renderPass );

	public interface IRenderer : IProcess
	{
		void Render( IRenderTarget renderTarget );
		void RenderToBackBuffer( RenderTarget2D renderTarget );
		void AddRenderPass( IRenderPass renderPass );
		void RemoveRenderPass( IRenderPass renderPass );
		T GetRenderPass<T>() where T : IRenderPass;
		Entity Camera { get; }
		BackBufferOutputMode BackBufferOutputMode{ get; set; }
		IEnumerable<IRenderPass> RenderPasses{ get; }

		event RenderPassDelegate OnRenderPassAdded;
		event RenderPassDelegate OnRenderPassRemoved;
	}
}

