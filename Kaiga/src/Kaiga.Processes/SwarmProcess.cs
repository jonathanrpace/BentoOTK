using System;
using Ramen;
using Kaiga.Components;
using System.Collections.Generic;
using OpenTK;

namespace Kaiga.Processes
{
	public class SwarmProcess : IProcess
	{
		class Node : Ramen.Node
		{
			public Transform transform = null;
			public SwarmMember swarmMember = null;
		}

		NodeList<Node> nodeList;
		Dictionary<Node, Vector3> offsetTable;
		Random rand;
		double elapsed;

		#region IProcess implementation

		public void OnAddedToScene( Scene scene )
		{
			rand = new Random();
			elapsed = 0.0;

			offsetTable = new Dictionary<Node, Vector3>();

			nodeList = new NodeList<Node>( scene );
			nodeList.NodeAdded += NodeList_NodeAdded;
			nodeList.NodeRemoved += NodeList_NodeRemoved;
			foreach ( var node in nodeList.Nodes )
			{
				NodeList_NodeAdded( node );
			}
		}
		
		public void OnRemovedFromScene( Scene scene )
		{
			nodeList.Dispose();
			nodeList = null;
			offsetTable.Clear();
			offsetTable = null;
			rand = null;
		}

		public void Update( double dt )
		{
			elapsed += dt;

			foreach ( var node in nodeList.Nodes )
			{
				var offset = offsetTable[ node ];
				var swarmMember = node.swarmMember;

				float x = (float)Math.Sin( ( elapsed + offset.X ) * swarmMember.Speed.X ) * swarmMember.Radius.X;
				float y = ((float)Math.Sin( ( elapsed + offset.Y ) * swarmMember.Speed.Y ) + 1.0f )* swarmMember.Radius.Y;
				float z = (float)Math.Sin( ( elapsed + offset.Z ) * swarmMember.Speed.Z ) * swarmMember.Radius.Z;

				node.transform.Matrix = Matrix4.Identity * Matrix4.CreateTranslation( x, y, z );
			}
		}

		public string Name
		{
			get
			{
				return "SwarmProcess";
			}
		}

		void NodeList_NodeAdded (Node node)
		{
			var offset = new Vector3(
				             (float)rand.NextDouble(),
				             (float)rand.NextDouble(),
				             (float)rand.NextDouble()
			             );
			offsetTable.Add( node, offset );
		}

		void NodeList_NodeRemoved (Node node)
		{
			offsetTable.Remove( node );
		}

		#endregion
	}
}

