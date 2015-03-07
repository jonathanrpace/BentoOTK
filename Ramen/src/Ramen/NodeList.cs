﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ramen
{
	public class NodeList<NodeType> where NodeType :Node, new()
	{
		public List<NodeType> Nodes { get { return nodes; } }

		private readonly Scene 							scene;
		private readonly Dictionary<Entity, NodeType> 	nodesByEntity;
		private readonly Dictionary<Type, string> 		componentNamesByType;
		private readonly List<NodeType> 				nodes;
		private readonly Type 							nodeType;

		public delegate void NodeListChangeDelegate( NodeType node );

		public event NodeListChangeDelegate NodeAdded;
		public event NodeListChangeDelegate NodeRemoved;
		
		public NodeList( Scene scene )
		{
			this.scene = scene;

			nodesByEntity = new Dictionary<Entity, NodeType>();
			componentNamesByType = new Dictionary<Type, string>();
			nodes = new List<NodeType>();

			nodeType = typeof(NodeType);
			FieldInfo[] fields = nodeType.GetFields();
			foreach ( FieldInfo fieldInfo in fields )
			{
				if ( fieldInfo.FieldType == typeof(Entity) )
					continue;
				componentNamesByType[ fieldInfo.FieldType ] = fieldInfo.Name;
			}

			foreach ( Entity entity in scene.Entities )
			{
				AddIfMatch( entity );
			}

			scene.EntityAdded += OnEntityAdded;
			scene.EntityRemoved += OnEntityRemoved;
			scene.ComponentAddedToEntity += OnComponentAddedToEntity;
			scene.ComponentRemovedFromEntity += OnComponentRemovedFromEntity;
		}

		~NodeList()
		{
			Dispose();
		}

		public void Dispose()
		{
			scene.EntityAdded -= OnEntityAdded;
			scene.EntityRemoved -= OnEntityRemoved;
			scene.ComponentAddedToEntity -= OnComponentAddedToEntity;
			scene.ComponentRemovedFromEntity -= OnComponentRemovedFromEntity;

			foreach ( Entity entity in nodesByEntity.Keys )
			{
				RemoveIfMatch( entity );
			}
		}

		private void AddIfMatch( Entity entity )
		{
			if ( nodesByEntity.ContainsKey( entity ) )
				return;

			foreach ( Type componentType in componentNamesByType.Keys )
			{
				if ( entity.HasComponentOfType( componentType ) == false )
				{
					return;
				}
			}
			
			var node = new NodeType();
			node.Entity = entity;
			foreach ( Type componentType in componentNamesByType.Keys )
			{
				FieldInfo fieldInfo = nodeType.GetField( componentNamesByType[ componentType ] );
				fieldInfo.SetValue( node, entity.GetComponentByType( componentType ) );
			}
			nodesByEntity[ entity ] = node;
			nodes.Add( node );

			if ( NodeAdded != null )
			{
				NodeAdded( node );
			}
		}

		private void RemoveIfMatch( Entity entity )
		{
			if ( nodesByEntity.ContainsKey( entity ) == false )
				return;

			foreach ( Type componentType in componentNamesByType.Keys )
			{
				if ( entity.HasComponentOfType( componentType ) == false )
				{
					nodesByEntity.Remove( entity );

					var node = nodesByEntity[entity];
					nodes.Remove( node );

					if ( NodeRemoved != null )
					{
						NodeRemoved( node );
					}
					return;
				}
			}
		}

		private void OnEntityAdded( Entity entity )
		{
			AddIfMatch( entity );
		}

		private void OnEntityRemoved( Entity entity )
		{
			RemoveIfMatch( entity );
		}

		private void OnComponentAddedToEntity( Entity entity, Object component )
		{
			AddIfMatch( entity );
		}

		private void OnComponentRemovedFromEntity( Entity entity, Object component )
		{
			RemoveIfMatch( entity );
		}
	}
}

