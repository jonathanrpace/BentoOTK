using System;
using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Geom;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class ImageLightShader : AbstractShader
	{
		readonly new ScreenQuadVertexShader vertexShader;
		readonly new ImageLightFragShader fragmentShader;

		readonly ScreenQuadGeometry screenQuadGeometry;

		public ImageLightShader() : base( new ScreenQuadVertexShader(), new ImageLightFragShader() )
		{
			vertexShader = (ScreenQuadVertexShader)base.vertexShader;
			fragmentShader = (ImageLightFragShader)base.fragmentShader;

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

