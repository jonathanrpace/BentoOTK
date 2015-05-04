using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Geom;
using Kaiga.Core;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class ScreenQuadTextureShader : AbstractShader<ScreenQuadVertexShader, SquareTextureFragShader>
	{
		private readonly ScreenQuadGeometry screenQuadGeom;

		public ScreenQuadTextureShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		override public void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( RenderParams renderParams, int texture )
		{
			BindPerPass( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.SetTexture( texture );

			screenQuadGeom.Bind();

			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 


			UnbindPerPass();
		}
	}
}

