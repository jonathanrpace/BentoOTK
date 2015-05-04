using System;
using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Geom;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class ImageLightShader : AbstractShader<ScreenQuadVertexShader, ImageLightFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeometry;

		public ImageLightShader()
		{
			screenQuadGeometry = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeometry.Dispose();
		}

		public override void BindPerPass(RenderParams renderParams)
		{
			base.BindPerPass(renderParams);
			screenQuadGeometry.Bind();
		}

		public void BindPerLight( RenderParams renderParams, ImageLight light )
		{
			fragmentShader.BindPerLight( renderParams, light );
		}

		public void Render()
		{
			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeometry.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
		}
	}
}

