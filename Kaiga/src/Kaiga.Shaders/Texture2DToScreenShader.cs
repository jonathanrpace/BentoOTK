using System;
using Kaiga.Geom;
using Kaiga.Core;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class Texture2DToScreenShader : AbstractShader<ScreenQuadVertexShader, SquareTextureFragShader>
	{
		private readonly ScreenQuadGeometry screenQuadGeom;

		public Texture2DToScreenShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		override public void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( int texture )
		{
			BindPerPass();

			fragmentShader.SetTexture( texture );

			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
		}
	}
}

