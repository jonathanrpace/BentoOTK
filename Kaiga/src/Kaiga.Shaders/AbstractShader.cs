using System;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders
{
	abstract public class AbstractShader<T,U> 
		: IDisposable 
		where T : AbstractShaderStage, new()
		where U : AbstractShaderStage, new()
	{
		protected readonly T vertexShader;
		protected readonly U fragmentShader;

		protected int pipeline;

		protected AbstractShader()
		{
			vertexShader = new T();
			fragmentShader = new U();

			pipeline = GL.GenProgramPipeline();

			if ( vertexShader != null )
			{
				GL.UseProgramStages( pipeline, ProgramStageMask.VertexShaderBit, vertexShader.ShaderProgram );
			}
			if ( fragmentShader != null )
			{
				GL.UseProgramStages( pipeline, ProgramStageMask.FragmentShaderBit, fragmentShader.ShaderProgram );
			}

			GL.BindProgramPipeline( 0 );
		}

		public virtual void Dispose()
		{
			if ( vertexShader != null )
			{
				vertexShader.Dispose();
			}
			if ( fragmentShader != null )
			{
				fragmentShader.Dispose();
			}

			if ( GL.IsProgramPipeline( pipeline ) )
			{
				GL.DeleteProgramPipeline( pipeline );
			}
		}

		public virtual void BindPerPass( RenderParams renderParams )
		{
			GL.BindProgramPipeline( pipeline );

			if ( vertexShader != null )
			{
				GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
				vertexShader.BindPerPass( renderParams );
			}

			if ( fragmentShader != null )
			{
				GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
				fragmentShader.BindPerPass( renderParams );
			}
		}

		protected void ActivateVertexShader()
		{
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
		}

		protected void ActivateFragmentShader()
		{
			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
		}
		
		public virtual void UnbindPerPass()
		{
			GL.BindProgramPipeline( 0 );
			if ( vertexShader != null )
			{
				vertexShader.UnbindPerPass();
			}
			if ( fragmentShader != null )
			{
				fragmentShader.UnbindPerPass();
			}
		}
	}
}

