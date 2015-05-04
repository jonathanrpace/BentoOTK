﻿using System;
using Ramen;
using Kaiga.Geom;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders;
using Kaiga.Components;
using Kaiga.Materials;

namespace Kaiga.RenderPasses
{
	public class GPass : IRenderPass
	{
		class Node : Ramen.Node
		{
			public IGeometry geom = null;
			public Transform transform = null;
			public StandardMaterial material = null;
		}

		private NodeList<Node> nodeList;
		private readonly GShader shader;

		public GPass()
		{
			shader = new GShader();
		}

		public void Dispose()
		{
			shader.Dispose();
			if ( nodeList != null )
			{
				nodeList.Dispose();
				nodeList = null;
			}
		}

		public void OnAddedToScene( Scene scene )
		{
			nodeList = new NodeList<Node>( scene );
		}

		public void OnRemovedFromScene( Ramen.Scene scene )
		{
			if ( nodeList != null )
			{
				nodeList.Dispose();
				nodeList = null;
			}
		}

		public void Render( RenderParams renderParams )
		{
			shader.BindPipeline( renderParams );
			foreach ( Node node in nodeList.Nodes )
			{
				renderParams.SetModelMatrix( node.transform.Matrix );

				node.geom.Bind();
				shader.BindPerMaterial( node.material );
				shader.BindPerModel( renderParams );
				node.geom.Draw();
				node.geom.Unbind();
			}

			shader.UnbindPipeline();
		}

		public RenderPhase RenderPhase
		{
			get
			{
				return RenderPhase.G;
			}
		}

		public bool Enabled
		{
			get
			{
				return true;
			}
		}
	}
}

