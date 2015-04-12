using System;
using Kaiga.ShaderStages;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders
{
	abstract public class AbstractShader : IDisposable
	{
		protected readonly AbstractShaderStage vertexShader;
		protected readonly AbstractShaderStage fragmentShader;

		protected int pipeline;

		protected AbstractShader( AbstractShaderStage vertexShader, AbstractShaderStage fragmentShader )
		{
			this.vertexShader = vertexShader;
			this.fragmentShader = fragmentShader;

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

