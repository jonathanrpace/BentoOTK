using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ramen
{
	public class Entity : NamedObject, IDisposable
	{
		// Fields
		private readonly List<Object> components;
		public IEnumerable<Object> Components {	get	{ return components.Skip(0); } }

		private readonly Dictionary<Type, Object> componentsByType;

		// Delegates
		public delegate void ComponentHandler( Entity entity, Object component );

		// Events
		public event ComponentHandler ComponentAdded;
		public event ComponentHandler ComponentRemoved;

		public Entity() : this( "Entity" )
		{

		}

		public Entity( string name ) : base( name )
		{
			components = new List<Object> ();
			componentsByType = new Dictionary<Type, Object> ();
		}

		public void Dispose()
		{
			foreach ( var component in components )
			{
				if ( component is IDisposable )
				{
					( component as IDisposable ).Dispose();
				}
			}
			components.Clear();
			componentsByType.Clear();
		}

		public void AddComponent( Object component, int index = -1 )
		{
			Debug.Assert( component != null );

			Type[] types;
			if ( component is IMultiTypeObject )
			{
				types = ((IMultiTypeObject)component).Types;
			}
			else
			{
				types = new Type[1]{ component.GetType() };
			}

			foreach ( Type type in types )
			{
				Debug.Assert( componentsByType.ContainsKey( type ) == false, "Component of this type already added." );

				componentsByType[type] = component;
			}

			if ( index == -1 )
			{
				components.Add( component );
			}
			else
			{
				components.Insert( index, component );
			}

			if ( ComponentAdded != null ) ComponentAdded( this, component );
		}

		public void RemoveComponent( Object component )
		{
			Debug.Assert( component != null );
			Debug.Assert( components.IndexOf( component ) != -1, "Component is not a child of this entity.");

			components.Remove( component );

			Type[] types;
			if ( component is IMultiTypeObject )
			{
				types = ((IMultiTypeObject)component).Types;
			}
			else
			{
				types = new Type[1]{ component.GetType() };
			}

			foreach ( Type type in types )
			{
				componentsByType.Remove( type );
			}

			if ( ComponentRemoved != null ) ComponentRemoved( this, component );
		}

		public bool HasComponentOfType( Type type )
		{
			return componentsByType.ContainsKey( type );
		}

		public object GetComponentByType( Type type )
		{
			return componentsByType[ type ];
		}

		public T GetComponentByType<T>()
		{
			return (T) componentsByType[ typeof(T) ];
		}
	}
}

