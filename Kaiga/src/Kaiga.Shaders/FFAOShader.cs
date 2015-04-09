using System;
using Kaiga.ShaderStages;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Geom;

namespace Kaiga.Shaders
{
	public class FFAOShader : AbstractShader
	{
		readonly new ScreenQuadVertexShader vertexShader;
		readonly new FFAOFragShader fragmentShader;
		readonly ScreenQuadGeometry screenQuadGeom;

		public FFAOShader() : base( new ScreenQuadVertexShader(), new FFAOFragShader() )
		{
			vertexShader = (ScreenQuadVertexShader)base.vertexShader;
			fragmentShader = (FFAOFragShader)base.fragmentShader;

			screenQuadGeom = new ScreenQuadGeometry();
		}

		#region IGraphicsContextDependant implementation

		override public void CreateGraphicsContextResources()
		{
			base.CreateGraphicsContextResources();
			screenQuadGeom.CreateGraphicsContextResources();
		}

		override public void DisposeGraphicsContextResources()
		{
			base.DisposeGraphicsContextResources();
			screenQuadGeom.DisposeGraphicsContextResources();
		}

		#endregion
		
		public void Render( RenderParams renderParams )
		{
			BindPerPass( renderParams );

			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			//vertexShader.BindPerPass( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindPerPass( renderParams );

			screenQuadGeom.Bind();
			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
			screenQuadGeom.Unbind();

			UnbindPerPass();
		}
	}
}

