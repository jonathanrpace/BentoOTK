using System;
using Kaiga.Core;
using Ramen;

namespace Kaiga.RenderPasses
{
	public class AbstractNodeRenderPass<T> : AbstractRenderPass, IDisposable 
		where T : Ramen.Node, new()
	{
		protected NodeList<T> nodeList;

		public AbstractNodeRenderPass( RenderPhase renderPhase ) : base( renderPhase )
		{
			nodeList = new NodeList<T>();
		}

		virtual public void Dispose()
		{
			nodeList.Clear();
		}

		virtual public void OnAddedToScene( Scene scene )
		{
			nodeList.BindToScene( scene );
		}

		virtual public void OnRemovedFromScene( Scene scene )
		{
			nodeList.Clear();
		}
	}
}

