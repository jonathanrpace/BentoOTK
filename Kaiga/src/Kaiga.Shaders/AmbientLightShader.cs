using System;
using Kaiga.ShaderStages;
using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Geom;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace Kaiga.Shaders
{
	public class AmbientLightShader : AbstractShader
	{
		readonly new ScreenQuadVertexShader vertexShader;
		readonly new AmbientLightFragShader fragmentShader;

		readonly ScreenQuadGeometry screenQuadGeometry;

		public AmbientLightShader() : base( new ScreenQuadVertexShader(), new AmbientLightFragShader() )
		{
			vertexShader = (ScreenQuadVertexShader)base.vertexShader;
			fragmentShader = (AmbientLightFragShader)base.fragmentShader;

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

