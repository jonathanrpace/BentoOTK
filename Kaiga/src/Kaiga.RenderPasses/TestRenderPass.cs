using System;
using Ramen;
using Kaiga.Geom;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders;

namespace Kaiga.RenderPasses
{
	class Node : Ramen.Node
	{
		public Geometry geom;

		public Node()
		{
			geom = null;
		}
	}

	public class TestRenderPass : IRenderPass
	{
		private NodeList<Node> nodeList;

		private TestVertexShader vertexShader;
		private TestFragShader fragmentShader;

		private int pipeline;

		public TestRenderPass()
		{
			vertexShader = new TestVertexShader();
			fragmentShader = new TestFragShader();
		}

		public void OnAddedToScene( Ramen.Scene scene )
		{
			nodeList = new NodeList<Node>( scene );

			pipeline = GL.GenProgramPipeline();
			GL.BindProgramPipeline( pipeline );
			GL.UseProgramStages( pipeline, ProgramStageMask.VertexShaderBit, vertexShader.ShaderProgram );
			GL.UseProgramStages( pipeline, ProgramStageMask.FragmentShaderBit, fragmentShader.ShaderProgram );
			GL.BindProgramPipeline( 0 );
		}

		public void OnRemovedFromScene( Ramen.Scene scene )
		{
			GL.DeleteProgramPipeline( pipeline );

			nodeList.Dispose();
			nodeList = null;
		}

		public void Render( RenderParams renderParams )
		{
			GL.BindProgramPipeline( pipeline );

			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindShader( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindShader( renderParams );

			foreach ( Node node in nodeList.Nodes )
			{
				node.geom.Bind();
				GL.DrawElements( PrimitiveType.Triangles, node.geom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
				node.geom.Unbind();
			}

			GL.BindProgramPipeline( 0 );
		}

		public RenderPhase RenderPhase
		{
			get
			{
				return RenderPhase.Material;
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

