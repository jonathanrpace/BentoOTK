using System;
using Kaiga.Core;
using Kaiga.ShaderStages;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders
{
	public class TestShader : IGraphicsContextDependant
	{
		private readonly TestVertexShader vertexShader;
		private readonly TestFragShader fragmentShader;

		int pipeline;

		public TestShader()
		{
			vertexShader = new TestVertexShader();
			fragmentShader = new TestFragShader();
		}

		#region IGraphicsContextDependant implementation

		public void CreateGraphicsContextResources()
		{
			vertexShader.CreateGraphicsContextResources();
			fragmentShader.CreateGraphicsContextResources();

			pipeline = GL.GenProgramPipeline();
			GL.UseProgramStages( pipeline, ProgramStageMask.VertexShaderBit, vertexShader.ShaderProgram );
			GL.UseProgramStages( pipeline, ProgramStageMask.FragmentShaderBit, fragmentShader.ShaderProgram );
			GL.BindProgramPipeline( 0 );
		}

		public void DisposeGraphicsContextResources()
		{
			vertexShader.DisposeGraphicsContextResources();
			fragmentShader.DisposeGraphicsContextResources();

			if ( GL.IsProgramPipeline( pipeline ) )
			{
				GL.DeleteProgramPipeline( pipeline );
			}
		}

		#endregion

		public void Bind( RenderParams renderParams )
		{
			GL.BindProgramPipeline( pipeline );

			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindShader( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindShader( renderParams );
		}

		public void Unbind()
		{
			GL.BindProgramPipeline( 0 );
		}
	}
}

