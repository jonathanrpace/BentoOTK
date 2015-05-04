using System;
using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Geom;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class PointLightStencilShader : AbstractShader<PointLightVertexShader, NullFragShader>
	{
		public void BindPerLight( RenderParams renderParams, PointLight pointLight )
		{
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindPerLight( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
		}
	}
}

