using System;
using Kaiga.Geom;
using Kaiga.Textures;
using Kaiga.Util;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Shaders.Fragment;
using Kaiga.Shaders.Vertex;
using System.Collections.Generic;

namespace Kaiga.Shaders
{
	public class RenderBufferDownsampleShader : AbstractShader
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		new readonly ScreenQuadVertexShader vertexShader;
		new readonly RectangleTextureFragShader fragmentShader;

		readonly SquareTexture2D output;
		readonly AbstractRenderTarget renderTarget;

		readonly List<AbstractRenderTarget> renderTargets;


		public RenderBufferDownsampleShader()
			: base( new ScreenQuadVertexShader(), new RectangleTextureFragShader() )
		{
			vertexShader = (ScreenQuadVertexShader)base.vertexShader;
			fragmentShader = (RectangleTextureFragShader)base.fragmentShader;

			output = new SquareTexture2D( PixelInternalFormat.Rgba16f, 1024 );
			output.MinFilter = TextureMinFilter.LinearMipmapLinear;

			screenQuadGeom = new ScreenQuadGeometry();
			renderTarget = new AbstractRenderTarget( PixelInternalFormat.Rgba16f, false, false );
			renderTarget.AttachTexture( FramebufferAttachment.ColorAttachment0, output );

			renderTargets = new List<AbstractRenderTarget>();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
			output.Dispose();
			renderTarget.Dispose();
		}

		public void Render( RenderParams renderParams, RectangleTexture texture )
		{
			BindPerPass( renderParams );

			int outputSize = TextureUtil.GetBestPowerOf2( Math.Min( texture.Width, texture.Height ) );
			renderTarget.SetSize( outputSize, outputSize );
			renderTarget.Bind();

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.SetTexture( texture.Texture );


			screenQuadGeom.Bind();
			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 

			output.GenerateMipMaps();

			UnbindPerPass();
		}

		public int OutputTexture
		{
			get
			{
				return output.Texture;
			}
		}
	}
}

