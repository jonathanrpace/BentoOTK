using System;
using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Geom;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class PointLightShader : AbstractShader
	{
		readonly new PointLightVertexShader vertexShader;
		readonly new PointLightFragShader fragmentShader;

		public PointLightShader() : base( new PointLightVertexShader(), new PointLightFragShader() )
		{
			vertexShader = (PointLightVertexShader)base.vertexShader;
			fragmentShader = (PointLightFragShader)base.fragmentShader;
		}

		public void BindPerLight( RenderParams renderParams, PointLight pointLight )
		{
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindPerLight( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindPerLight( renderParams, pointLight );
		}

		public void Render( RenderParams renderParams, Geometry geom )
		{
			GL.DrawElements( PrimitiveType.Triangles, geom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
		}
	}
}

