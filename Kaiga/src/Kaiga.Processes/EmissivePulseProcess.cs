using System;
using Ramen;
using Kaiga.Materials;
using Kaiga.Components;

namespace Kaiga.Processes
{
	public class EmissivePulseProcess : IProcess
	{
		class Node : Ramen.Node
		{
			public StandardMaterial material = null;
			public EmissivePulser pulser = null;
		}

		readonly NodeList<Node> nodeList;
		double elapsed;

		public EmissivePulseProcess()
		{
			nodeList = new NodeList<Node>();
		}

		public void Dispose()
		{
			nodeList.Clear();
		}
		
		public void OnAddedToScene( Scene scene )
		{
			nodeList.BindToScene( scene );
			elapsed = 0.0;
		}

		public void OnRemovedFromScene( Scene scene )
		{
			nodeList.Clear();
		}

		public void Update( double dt )
		{
			elapsed += dt;

			foreach ( var node in nodeList.Nodes )
			{
				float ratio = (float)Math.Sin( node.pulser.Offset + elapsed * node.pulser.Frequency * 0.5f );
				ratio = ratio < 0.0f ? 0.0f : ratio;
				//ratio *= ratio;
				//ratio *= 0.5f;
				//ratio = ( ratio + 1.0f ) * 0.5f;
				float range = node.pulser.Max - node.pulser.Min;

				node.material.Emissive = node.pulser.Min + Math.Max( 0.0f, ratio ) * range;
			}
		}

		public string Name
		{
			get
			{
				return "EmissivePulseProcess";
			}
		}
	}
}

