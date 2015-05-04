using System;
using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Geom;
using OpenTK.Graphics.OpenGL4;
using OpenTK;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class AmbientLightShader : AbstractShader<ScreenQuadVertexShader, AmbientLightFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeometry;

		public AmbientLightShader()
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

		public void BindPerLight( RenderParams renderParams, AmbientLight light )
		{
			fragmentShader.BindPerLight( renderParams, light );
		}

		public void Render()
		{
			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeometry.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
		}
	}
}

