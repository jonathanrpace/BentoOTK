using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Geom;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class BoxBlurShader : AbstractShader<ScreenQuadVertexShader, BoxBlurFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		public BoxBlurShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( RenderParams renderParams, int texture, float radiusU, float radiusV )
		{
			BindPerPass( renderParams );

			fragmentShader.Bind( renderParams, texture, radiusU, radiusV );

			screenQuadGeom.Bind();

			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 

			UnbindPerPass();
		}
	}
}

