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
			BindPipeline( renderParams );

			BindFragmentShader();
			fragmentShader.SetTexture( texture );

			screenQuadGeom.Bind();
			screenQuadGeom.Draw();

			UnbindPipeline();
		}
	}
}

