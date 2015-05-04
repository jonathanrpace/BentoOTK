using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Geom;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class LightResolveShader : AbstractShader
	{
		readonly new ScreenQuadVertexShader vertexShader;
		readonly new LightResolveFragShader fragmentShader;
		readonly ScreenQuadGeometry screenQuadGeom;

		public LightResolveShader() : 
		base( new ScreenQuadVertexShader(), new LightResolveFragShader() )
		{
			vertexShader = (ScreenQuadVertexShader)base.vertexShader;
			fragmentShader = (LightResolveFragShader)base.fragmentShader;

			screenQuadGeom = new ScreenQuadGeometry();
		}

		override public void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( RenderParams renderParams )
		{
			BindPerPass( renderParams );

			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindPerPass( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindPerPass( renderParams );

			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
			screenQuadGeom.Unbind();

			UnbindPerPass();
		}
	}
}

