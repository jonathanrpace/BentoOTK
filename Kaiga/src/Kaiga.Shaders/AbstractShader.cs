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
			pipeline = GL.GenProgramPipeline();

			if ( vertexShader != null )
			{
				vertexShader.CreateGraphicsContextResources();
				GL.UseProgramStages( pipeline, ProgramStageMask.VertexShaderBit, vertexShader.ShaderProgram );
			}
			if ( fragmentShader != null )
			{
				fragmentShader.CreateGraphicsContextResources();
				GL.UseProgramStages( pipeline, ProgramStageMask.FragmentShaderBit, fragmentShader.ShaderProgram );
			}
			
			GL.BindProgramPipeline( 0 );
		}

		public virtual void DisposeGraphicsContextResources()
		{
			if ( vertexShader != null )
			{
				vertexShader.DisposeGraphicsContextResources();
			}
			if ( fragmentShader != null )
			{
				fragmentShader.DisposeGraphicsContextResources();
			}

			if ( GL.IsProgramPipeline( pipeline ) )
			{
				GL.DeleteProgramPipeline( pipeline );
			}
		}

		#endregion

		public void BindPerPass( RenderParams renderParams )
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

		public void UnbindPerPass()
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

