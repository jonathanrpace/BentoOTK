using System;
using Kaiga.Geom;
using Kaiga.Textures;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Shaders.Fragment;
using Kaiga.Shaders.Vertex;

namespace Kaiga.Shaders
{
	public class RenderBufferDownsampleShader : AbstractShader
	{
		readonly ScreenQuadGeometry screenQuadGeom;
		new readonly ScreenQuadVertexShader vertexShader;
		new readonly RenderBufferDownsampleFragShader fragmentShader;

		public RenderBufferDownsampleShader()
			: base( new ScreenQuadVertexShader(), new RenderBufferDownsampleFragShader() )
		{
			vertexShader = (ScreenQuadVertexShader)base.vertexShader;
			fragmentShader = (RenderBufferDownsampleFragShader)base.fragmentShader;

			screenQuadGeom = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( RenderParams renderParams, ITexture2D source, int level )
		{
			BindPerPass( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.SetTexture( source.Texture, level, (float)renderParams.RenderTarget.OutputBuffer.Width / renderParams.RenderTarget.OutputBuffer.Height );

			screenQuadGeom.Bind();
			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 

			UnbindPerPass();
		}
	}
}

