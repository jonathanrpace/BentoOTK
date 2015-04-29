using System;
using Kaiga.Textures;
using Kaiga.Util;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using System.Collections.Generic;

namespace Kaiga.Shaders
{
	public class RenderBufferDownsampler : IDisposable
	{
		readonly ScreenQuadTextureRectShader rectTo2DShader;
		readonly SquareTexture2D output;
		readonly List<AbstractRenderTarget> renderTargets;

		public RenderBufferDownsampler()
		{
			rectTo2DShader = new ScreenQuadTextureRectShader();

			output = new SquareTexture2D( PixelInternalFormat.Rgba16f, 1024 );
			output.MinFilter = TextureMinFilter.LinearMipmapLinear;

			renderTargets = new List<AbstractRenderTarget>();
		}

		public void Dispose()
		{
			output.Dispose();

			foreach ( var renderTarget in renderTargets )
			{
				renderTarget.Dispose();
			}
			renderTargets.Clear();
		}

		public void Render( RenderParams renderParams, RectangleTexture source )
		{
			int outputSize = TextureUtil.GetBestPowerOf2( Math.Min( source.Width, source.Height ) );
			output.SetSize( outputSize, outputSize );
			
			if ( renderTargets.Count != output.NumMipMaps )
			{
				foreach ( var renderTarget in renderTargets )
				{
					renderTarget.Dispose();
				}
				renderTargets.Clear();

				for ( int i = 0; i < output.NumMipMaps; i++ )
				{
					var renderTarget = new AbstractRenderTarget( PixelInternalFormat.Rgba16f, false, false );
					renderTarget.SetSize( outputSize, outputSize );
					renderTarget.AttachTexture( FramebufferAttachment.ColorAttachment0, output, i );
				}
			}

			renderTargets[ 0 ].Bind();
			rectTo2DShader.Render( renderParams, source.Texture );

			for ( int i = 1; i < renderTargets.Count; i++ )
			{
				var renderTarget = renderTargets[ i ];
				renderTarget.Bind();
			}

			output.GenerateMipMaps();
		}

		public SquareTexture2D Output
		{
			get
			{
				return Output;
			}
		}
	}
}

