using System;
using Kaiga.Core;
using Kaiga.ShaderStages;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Materials;

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

		public void BindPerMaterial( StandardMaterial material )
		{
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			//vertexShader.BindPerMaterial( material );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindPerMaterial( material );
		}

		public void BindPerModel( RenderParams renderParams )
		{
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindPerModel( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			//fragmentShader.BindPerModel( renderParams );
		}

		public void End()
		{
			GL.BindProgramPipeline( 0 );
		}
	}
}

