using System;
using Ramen;
using Kaiga.Components;
using OpenTK.Input;
using OpenTK;
using Kaiga.Lights;

namespace Kaiga.Processes
{
	public class DebugMouseDirectionalTransformProcess : NamedObject, IProcess
	{
		class Node : Ramen.Node
		{
			public Transform transform = null;
			public DirectionalLight light = null;
		}

		readonly NodeList<Node> nodeList;
		private Scene scene;

		public DebugMouseDirectionalTransformProcess() : base( "DebugDirectionTransformProcess" )
		{
			nodeList = new NodeList<Node>();
		}

		#region IProcess implementation

		public void OnAddedToScene( Scene scene )
		{
			this.scene = scene;
			nodeList.BindToScene( scene );
		}

		public void OnRemovedFromScene( Scene scene )
		{
			this.scene = null;
			nodeList.Clear();
		}

		public void Update( double dt )
		{
			var mouseX = (float)Mouse.GetState().X / scene.GameWindow.Width;
			var mouseY = (float)Mouse.GetState().Y / scene.GameWindow.Height;

			foreach ( Node node in nodeList.Nodes )
			{
				node.transform.Identity();
				node.transform.RotateX( mouseY * (float)Math.PI * 0.5f );
				node.transform.RotateY( mouseX * (float)Math.PI * 1.0f );
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose()
		{
			nodeList.Clear();
		}

		#endregion
	}
}

