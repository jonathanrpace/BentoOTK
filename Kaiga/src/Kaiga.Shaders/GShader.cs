using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Materials;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class GShader : AbstractShader<GVertexShader, GFragShader>
	{
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
	}
}

