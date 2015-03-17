using System;
using Kaiga.Core;
using Kaiga.ShaderStages;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Materials;
using Kaiga.Geom;

namespace Kaiga.Shaders
{
	public class NormalBufferOutputShader : IGraphicsContextDependant
	{
		private readonly ScreenQuadVertexShader vertexShader;
		private readonly NormalBufferFragShader fragmentShader;
		private readonly ScreenQuadGeometry screenQuadGeom;

		int pipeline;
		int sampler;

		public NormalBufferOutputShader()
		{
			vertexShader = new ScreenQuadVertexShader();
			fragmentShader = new NormalBufferFragShader();
			screenQuadGeom = new ScreenQuadGeometry();
		}

		#region IGraphicsContextDependant implementation

		public void CreateGraphicsContextResources()
		{
			vertexShader.CreateGraphicsContextResources();
			fragmentShader.CreateGraphicsContextResources();
			screenQuadGeom.CreateGraphicsContextResources();

			pipeline = GL.GenProgramPipeline();
			GL.UseProgramStages( pipeline, ProgramStageMask.VertexShaderBit, vertexShader.ShaderProgram );
			GL.UseProgramStages( pipeline, ProgramStageMask.FragmentShaderBit, fragmentShader.ShaderProgram );
			GL.BindProgramPipeline( 0 );

			sampler = GL.GenSampler();
			GL.BindSampler( 0, sampler );

		}

		public void DisposeGraphicsContextResources()
		{
			vertexShader.DisposeGraphicsContextResources();
			fragmentShader.DisposeGraphicsContextResources();
			screenQuadGeom.DisposeGraphicsContextResources();

			if ( GL.IsProgramPipeline( pipeline ) )
			{
				GL.DeleteProgramPipeline( pipeline );
			}
		}

		#endregion

		public void Render( RenderParams renderParams )
		{
			GL.BindProgramPipeline( pipeline );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.Bind( renderParams );
			
			screenQuadGeom.Bind();

			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 

			screenQuadGeom.Unbind();
			fragmentShader.Unbind();

			GL.BindProgramPipeline( 0 );
		}
	}
}

