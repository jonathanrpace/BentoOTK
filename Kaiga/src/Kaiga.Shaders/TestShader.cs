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
		public void Begin()
		{
			GL.BindProgramPipeline( pipeline );
		}

		public void BindPerModel( RenderParams renderParams )
		{
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindPerModel( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
		}

		public void End()
		{
			GL.BindProgramPipeline( 0 );
		}
	}
}

