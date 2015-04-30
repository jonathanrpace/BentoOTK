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
		readonly RenderBufferDownsampleShader downsampleShader;
		readonly SquareTexture2D output;
		readonly List<AbstractRenderTarget> renderTargets;

		public RenderBufferDownsampler()
		{
			rectTo2DShader = new ScreenQuadTextureRectShader();
			downsampleShader = new RenderBufferDownsampleShader();

			output = new SquareTexture2D( PixelInternalFormat.Rgba16f, 1024 );
			output.MinFilter = TextureMinFilter.LinearMipmapLinear;

			renderTargets = new List<AbstractRenderTarget>();
		}

		public void Dispose()
		{
			output.Dispose();

			downsampleShader.Dispose();
			rectTo2DShader.Dispose();

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

			int numLevels = Math.Max( output.NumMipMaps, 1 );
			if ( renderTargets.Count != numLevels )
			{
				foreach ( var renderTarget in renderTargets )
				{
					renderTarget.Dispose();
				}
				renderTargets.Clear();

				for ( int i = 0; i < numLevels; i++ )
				{
					var renderTarget = new AbstractRenderTarget( PixelInternalFormat.Rgba16f, false, false );
					renderTarget.SetSize( outputSize >> i, outputSize >> i );
					renderTarget.AttachTexture( FramebufferAttachment.ColorAttachment0, output, i );
					renderTargets.Add( renderTarget );
				}
			}

			renderTargets[ 0 ].Bind();
			rectTo2DShader.Render( renderParams, source.Texture );

			// Don't bother using AbstractRenderBuffer - just roll it manually - then I can be sure the TexParameter stuff really works.

			for ( int i = 1; i < renderTargets.Count; i++ )
			{
				var renderTarget = renderTargets[ i ];

				GL.BindTexture( TextureTarget.Texture2D, output.Texture );
				int minMax = i - 1;
				GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, ref minMax );
				GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, ref minMax );
				GL.BindTexture( TextureTarget.Texture2D, 0 );

				renderTarget.Bind();
				downsampleShader.Render( renderParams, output );
			}


			GL.BindTexture( TextureTarget.Texture2D, output.Texture );
			int min = 0;
			int max = output.NumMipMaps;
			GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, ref min );
			GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, ref max );
			GL.BindTexture( TextureTarget.Texture2D, 0 );

			//output.GenerateMipMaps();
		}

		public SquareTexture2D Output
		{
			get
			{
				return output;
			}
		}
	}
}

