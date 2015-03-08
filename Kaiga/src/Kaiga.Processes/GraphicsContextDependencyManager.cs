using System;
using Ramen;
using Kaiga.Core;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kaiga.Processes
{
	class Node : Ramen.Node
	{
		public IGraphicsContextDependant dependant;

		public Node()
		{
			dependant = null;
		}
	}

	public class GraphicsContextDependencyManager : NamedObject, IProcess
	{
		NodeList<Node> nodeList;
		Dictionary<IGraphicsContextDependant, int> refCountTable;
		Scene scene;

		public GraphicsContextDependencyManager()
		{
			refCountTable = new Dictionary<IGraphicsContextDependant, int>();
		}

		public void OnAddedToScene( Scene scene )
		{
			this.scene = scene;

			nodeList = new NodeList<Node>( scene );
			nodeList.NodeAdded += HandleNodeAdded;
			nodeList.NodeRemoved += HandleNodeRemoved;

			scene.GameWindow.Unload += HandleWindowUnload;
			scene.ProcessAdded += HandleProcessAdded;
			scene.ProcessRemoved += HandleProcessRemoved;

			foreach ( Node node in nodeList.Nodes )
			{
				HandleNodeAdded( node );
			}

			foreach ( IProcess process in scene.Processes )
			{
				HandleProcessAdded( process );
			}
		}

		public void OnRemovedFromScene( Scene scene )
		{
			foreach ( Node node in nodeList.Nodes )
			{
				HandleNodeRemoved( node );
			}

			foreach ( IProcess process in scene.Processes )
			{
				HandleProcessRemoved( process );
			}

			scene.GameWindow.Unload -= HandleWindowUnload;
			scene.ProcessAdded -= HandleProcessAdded;
			scene.ProcessRemoved -= HandleProcessRemoved;

			nodeList.NodeAdded -= HandleNodeAdded;
			nodeList.NodeRemoved -= HandleNodeRemoved;
			nodeList.Dispose();
			nodeList = null;

			this.scene = null;
		}

		public void Update( double dt )
		{

		}

		void HandleNodeAdded (Node node)
		{
			HandleDependantAdded( node.dependant );
		}

		void HandleNodeRemoved (Node node)
		{
			HandleDependantRemoved( node.dependant );
		}

		void HandleDependantAdded( IGraphicsContextDependant dependant )
		{
			if ( refCountTable.ContainsKey( dependant ) == false )
			{
				refCountTable.Add( dependant, 0 );
			}
			var refCount = ++refCountTable[ dependant ];

			if ( refCount == 1 )
			{
				dependant.CreateGraphicsContextResources();
			}
		}

		void HandleDependantRemoved( IGraphicsContextDependant dependant )
		{
			if ( refCountTable.ContainsKey( dependant ) == false )
				return;

			var refCount = --refCountTable[ dependant ];

			Debug.Assert( refCount >= 0 );

			if ( refCount == 0 )
			{
				dependant.DisposeGraphicsContextResources();

				refCountTable.Remove( dependant );
			}
		}

		void HandleWindowUnload( object sender, EventArgs e )
		{
			foreach ( Node node in nodeList.Nodes )
			{
				HandleNodeRemoved( node );
			}
		}

		void HandleProcessAdded (IProcess process)
		{
			if ( process is IGraphicsContextDependant )
			{
				HandleDependantAdded( process as IGraphicsContextDependant );
			}
		}

		void HandleProcessRemoved (IProcess process)
		{
			if ( process is IGraphicsContextDependant )
			{
				HandleDependantRemoved( process as IGraphicsContextDependant );
			}
		}
	}
}

