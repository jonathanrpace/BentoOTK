using Kaiga.Core;
using Kaiga.ShaderStages;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Materials;

namespace Kaiga.Shaders
{
	public class GShader : AbstractShader
	{
		private readonly new GVertexShader vertexShader;
		private readonly new GFragShader fragmentShader;
		
		public GShader() : base( new GVertexShader(), new GFragShader() )
		{
			vertexShader = (GVertexShader)base.vertexShader;
			fragmentShader = (GFragShader)base.fragmentShader;
		}
		
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

