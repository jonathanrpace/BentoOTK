using System;
using Kaiga.Geom;
using Kaiga.Textures;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Shaders.Fragment;
using Kaiga.Shaders.Vertex;

namespace Kaiga.Shaders
{
	public class RenderBufferDownsampleShader : AbstractShader<ScreenQuadVertexShader, RenderBufferDownsampleFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		public RenderBufferDownsampleShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( RenderParams renderParams, ITexture2D source, int level )
		{
			BindPipeline( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.SetTexture( source.Texture, level, (float)renderParams.RenderTarget.OutputBuffer.Width / renderParams.RenderTarget.OutputBuffer.Height );

			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
			UnbindPipeline();
		}
	}
}

