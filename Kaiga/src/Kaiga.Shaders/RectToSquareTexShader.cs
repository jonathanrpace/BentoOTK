using System;
using Kaiga.Geom;
using Kaiga.Textures;
using Kaiga.Util;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Shaders.Fragment;
using Kaiga.Shaders.Vertex;

namespace Kaiga.Shaders
{
	public class RectToSquareTexShader : AbstractShader
	{
		readonly ScreenQuadGeometry screenQuadGeom;
		new readonly RectToSquareTexFragShader fragmentShader;

		readonly SquareTexture2D output;
		readonly AbstractRenderTarget renderTarget;


		public RectToSquareTexShader()
			: base( new ScreenQuadVertexShader(), new RectToSquareTexFragShader() )
		{
			fragmentShader = (RectToSquareTexFragShader)base.fragmentShader;

			output = new SquareTexture2D( PixelInternalFormat.Rgba16f, 1024 );
			output.MinFilter = TextureMinFilter.LinearMipmapLinear;

			screenQuadGeom = new ScreenQuadGeometry();
			renderTarget = new AbstractRenderTarget( PixelInternalFormat.Rgba16f, false, false );
			renderTarget.AttachTexture( FramebufferAttachment.ColorAttachment0, output );
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
			fragmentShader.Render( texture.Texture, output.Width );

			output.GenerateMipMaps();

			UnbindPerPass();
		}
	}
}

