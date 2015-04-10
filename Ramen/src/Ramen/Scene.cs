using System;
using System.Collections.Generic;
using System.Timers;
using System.Diagnostics;
using System.Linq;
using OpenTK;

namespace Ramen
{
	public class Scene
	{
		// PRIVATE
		private readonly List<Entity> 					entities;
		private readonly List<IProcess> 				processes;
		private readonly List<Object> 					resources;
		private readonly Dictionary<Type, IProcess> 	processesByType;
		private readonly Dictionary<Object, uint> 	componentRefCountTable;
		private readonly Timer 							timer;
		private bool 									isRunning;

		// PUBLIC

		// Fields
		public GameWindow GameWindow { get; private set; }

		// Enumerables
		public IEnumerable<Entity> 		Entities	{ get {	return entities.Skip(0); } }
		public IEnumerable<IProcess> 	Processes	{ get {	return processes.Skip(0); } }
		public IEnumerable<Object> 		Resources	{ get {	return resources.Skip(0); } }

		// Delegates
		public delegate void EntityDelegate( Entity entity );
		public delegate void ProcessDelegate( IProcess process );
		public delegate void ResourceDelegate( Object resource );
		public delegate void ComponentDelegate( Object component );
		public delegate void EntityComponentDelegate( Entity entity, Object component );

		// Events
		public event ResourceDelegate ResourceAdded;
		public event ResourceDelegate ResourceRemoved;
		public event ProcessDelegate ProcessAdded;
		public event ProcessDelegate ProcessRemoved;
		public event EntityDelegate EntityAdded;
		public event EntityDelegate EntityRemoved;
		public event EntityComponentDelegate ComponentAddedToEntity;
		public event EntityComponentDelegate ComponentRemovedFromEntity;
		public event ComponentDelegate ComponentAddedToScene;
		public event ComponentDelegate ComponentRemovedFromScene;

		public Scene( GameWindow gameWindow )
		{
			GameWindow = gameWindow;

			entities = new List<Entity>();
			processes = new List<IProcess>();
			resources = new List<Object>();

			processesByType = new Dictionary<Type, IProcess>();
			componentRefCountTable = new Dictionary<Object, uint>();

			timer = new Timer( 1000 / 60 );
			timer.AutoReset = true;
			timer.Elapsed += OnTimerElapsed;

			isRunning = false;
		}

		~Scene()
		{
			Stop();
			timer.Elapsed -= OnTimerElapsed;
		}

		public void Dispose()
		{
			while ( entities.Count > 0 )
			{
				var entity = entities[ 0 ];
				RemoveEntity( entity );
				entity.Dispose();
			}

			while ( resources.Count > 0 )
			{
				var resource = resources[ 0 ];
				RemoveResource( resource );
				if ( resource is IDisposable )
				{
					( resource as IDisposable ).Dispose();
				}
			}

			while ( processes.Count > 0 )
			{
				var process = processes[ 0 ];
				RemoveProcess( process );
				process.Dispose();
			}
		}

		public void Start()
		{
			if ( isRunning )
			{
				return;
			}

			isRunning = true;
			timer.Start();
		}

		public void Stop()
		{
			if ( !isRunning )
			{
				return;
			}

			isRunning = false;
			timer.Stop();
		}

		public void AddEntity( Entity entity )
		{
			Debug.Assert( entity != null );
			Debug.Assert( entities.IndexOf( entity ) == -1, "Entity already a child of this scene." );

			entities.Add( entity );
			EntityAdded( entity );

			entity.ComponentAdded += OnComponentAddedToEntity;
			entity.ComponentRemoved += OnComponentRemovedFromEntity;

			foreach ( Object component in entity.Components )
			{
				if ( ComponentAddedToEntity != null ) ComponentAddedToEntity( entity, component );

				uint numRefs = componentRefCountTable.ContainsKey( component ) == false ? 
					componentRefCountTable[ component ] = 1 : componentRefCountTable[ component ]++;

				if ( numRefs == 1 )
				{
					if ( ComponentAddedToScene != null ) ComponentAddedToScene( component );
				}
			}
		}

		public void RemoveEntity( Entity entity )
		{
			Debug.Assert( entity != null );
			Debug.Assert( entities.IndexOf( entity ) != -1, "Entity not a child of this scene" );

			foreach ( Object component in entity.Components )
			{
				ComponentRemovedFromEntity( entity, component );

				uint numRefs = componentRefCountTable[ component ]--;
				if ( numRefs == 0 )
				{
					componentRefCountTable.Remove( component );
					ComponentRemovedFromScene( component );
				}
			}

			entities.Remove( entity );

			if ( EntityRemoved != null ) EntityRemoved( entity );

			entity.ComponentAdded -= OnComponentAddedToEntity;
			entity.ComponentRemoved -= OnComponentRemovedFromEntity;
		}

		public void AddProcess( IProcess process )
		{
			Debug.Assert( process != null );
			Debug.Assert( processes.IndexOf( process ) == -1, "Process already a child of this Scene." );


			Type[] types;
			if ( process is IMultiTypeObject )
			{
				types = ( process as IMultiTypeObject ).Types;
			}
			else
			{
				types = new Type[1]{ process.GetType() };
			}

			foreach ( var type in types )
			{
				Debug.Assert( processesByType.ContainsKey( type ) == false, "Process of this type already a child of this Scene." );
				processesByType[ type ] = process;
			}

			processes.Add( process );
			process.OnAddedToScene( this );

			if ( ProcessAdded != null ) ProcessAdded( process );
		}

		public void RemoveProcess( IProcess process )
		{
			Debug.Assert( process != null );
			Debug.Assert( processes.IndexOf( process ) != -1, "Process is not a child of this Scene" );

			Type[] types;
			if ( process is IMultiTypeObject )
			{
				types = ( process as IMultiTypeObject ).Types;
			}
			else
			{
				types = new Type[1]{ process.GetType() };
			}

			foreach ( var type in types )
			{
				processesByType.Remove( type );
			}

			processes.Remove( process );
			process.OnRemovedFromScene( this );

			if ( ProcessRemoved != null ) ProcessRemoved( process );
		}

		public void AddResource( Object resource )
		{
			Debug.Assert( resource != null );
			Debug.Assert( resources.IndexOf( resource ) == -1, "Resource already a child of this Scene" );

			resources.Add( resource );

			if ( ResourceAdded != null ) ResourceAdded( resource );
		}

		public void RemoveResource( Object resource )
		{
			Debug.Assert( resource != null );
			Debug.Assert( resources.IndexOf( resource ) != -1, "Resource is not a child of this Scene" );

			resources.Remove( resource );

			if ( ResourceRemoved != null ) ResourceRemoved( resource );
		}

		public void Update( double delta )
		{
			foreach ( IProcess process in processes )
			{
				process.Update( delta );
			}
		}

		public IProcess GetProcessByType( Type type )
		{
			return processesByType[ type ];
		}

		public T GetProcessByType<T>()
		{
			return (T) processesByType[ typeof(T) ];
		}

		private void OnTimerElapsed( object sender, ElapsedEventArgs e )
		{
			Update( timer.Interval );
		}

		private void OnComponentAddedToEntity( Entity entity, Object component )
		{
			ComponentAddedToEntity( entity, component );

			uint numRefs = componentRefCountTable.ContainsKey( component ) == false ? 
				componentRefCountTable[ component ] = 1 : componentRefCountTable[ component ]++;
			if ( numRefs == 1 )
			{
				if ( ComponentAddedToScene != null ) ComponentAddedToScene( component );
			}
		}

		private void OnComponentRemovedFromEntity( Entity entity, Object component )
		{
			ComponentRemovedFromEntity( entity, component );

			uint numRefs = componentRefCountTable[ component ]--;
			if ( numRefs == 0 )
			{
				componentRefCountTable.Remove( component );
				if ( ComponentRemovedFromScene != null ) ComponentRemovedFromScene( component );
			}
		}
	}
}

