using System;
using Kaiga.ShaderStages;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders
{
	abstract public class AbstractShader : IGraphicsContextDependant
	{
		protected readonly AbstractShaderStage vertexShader;
		protected readonly AbstractShaderStage fragmentShader;

		protected int pipeline;

		protected AbstractShader( AbstractShaderStage vertexShader, AbstractShaderStage fragmentShader )
		{
			this.vertexShader = vertexShader;
			this.fragmentShader = fragmentShader;
		}

		#region IGraphicsContextDependant implementation

		public virtual void CreateGraphicsContextResources()
		{
			vertexShader.CreateGraphicsContextResources();
			fragmentShader.CreateGraphicsContextResources();

			pipeline = GL.GenProgramPipeline();
			GL.UseProgramStages( pipeline, ProgramStageMask.VertexShaderBit, vertexShader.ShaderProgram );
			GL.UseProgramStages( pipeline, ProgramStageMask.FragmentShaderBit, fragmentShader.ShaderProgram );
			GL.BindProgramPipeline( 0 );
		}

		public virtual void DisposeGraphicsContextResources()
		{
			vertexShader.DisposeGraphicsContextResources();
			fragmentShader.DisposeGraphicsContextResources();

			if ( GL.IsProgramPipeline( pipeline ) )
			{
				GL.DeleteProgramPipeline( pipeline );
			}
		}

		#endregion

		public void BindPerPass()
		{
			GL.BindProgramPipeline( pipeline );
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindPerPass();

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindPerPass();
		}

		public void UnbindPerPass()
		{
			GL.BindProgramPipeline( 0 );
			vertexShader.UnbindPerPass();
			fragmentShader.UnbindPerPass();
		}
	}
}

