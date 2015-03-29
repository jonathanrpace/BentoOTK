using System;
using Kaiga.ShaderStages;
using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Geom;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace Kaiga.Shaders
{
	public class PointLightStencilShader : AbstractShader
	{
		readonly new PointLightVertexShader vertexShader;

		public PointLightStencilShader() : base( new PointLightVertexShader(), new NullFragShader() )
		{
			vertexShader = (PointLightVertexShader)base.vertexShader;
		}

		public void BindPerLight( RenderParams renderParams, PointLight pointLight )
		{
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindPerLight( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
		}

		public void Render( RenderParams renderParams, Geometry geom )
		{
			GL.DrawElements( PrimitiveType.Triangles, geom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
		}
	}
}

