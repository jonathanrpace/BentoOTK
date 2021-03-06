using System;
using Kaiga.Textures;
using Kaiga.Util;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kaiga.Shaders
{
	public class LightBufferConvolver : IDisposable
	{
		readonly TextureRectToScreenShader rectTo2DShader;
		readonly LightBufferConvolutionShader downsampleShader;
		readonly SquareTexture2D output;

		readonly List<int> frameBuffers;

		public LightBufferConvolver()
		{
			rectTo2DShader = new TextureRectToScreenShader();
			downsampleShader = new LightBufferConvolutionShader();

			output = new SquareTexture2D( PixelInternalFormat.Rgba16f, 4 );
			output.MinFilter = TextureMinFilter.LinearMipmapLinear;
			output.WrapModeR = output.WrapModeS = TextureWrapMode.ClampToEdge;

			frameBuffers = new List<int>();
		}

		public void Dispose()
		{
			output.Dispose();

			downsampleShader.Dispose();
			rectTo2DShader.Dispose();

			foreach ( var frameBuffer in frameBuffers )
			{
				GL.DeleteFramebuffer( frameBuffer );
			}
			frameBuffers.Clear();
		}

		public void Render( RectangleTexture source )
		{
			int outputSize = TextureUtil.GetBestPowerOf2( Math.Min( source.Width, source.Height ) >> 1 );

			if ( output.Width != outputSize )
			{
				output.SetSize( outputSize, outputSize );
				output.GenerateMipMaps();
			}

			GL.Viewport( 0, 0, outputSize, outputSize );

			int numLevels = Math.Max( output.NumMipMaps, 1 );
			if ( frameBuffers.Count != numLevels )
			{
				foreach ( int frameBuffer in frameBuffers )
				{
					GL.DeleteFramebuffer( frameBuffer );
				}
				frameBuffers.Clear();

				for ( int i = 0; i < numLevels; i++ )
				{
					int frameBuffer = GL.GenFramebuffer();
					frameBuffers.Add( frameBuffer );
				}
			}
				
			// Writing to first mip level requires a shader that converts from rect to square texture.
			{
				GL.BindFramebuffer( FramebufferTarget.Framebuffer, frameBuffers[ 0 ] );
				GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, output.Texture, 0 );
				GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, frameBuffers[ 0 ] );
				GL.DrawBuffers( 1, new [] { DrawBuffersEnum.ColorAttachment0 } );
				CheckFrameBuffer();
				rectTo2DShader.Render( source.Texture );
			}

			// Remaining levels can all use same shader, with built in blur at each stage to produce progressively
			// more convoluted downsamples.
			GL.BindTexture( TextureTarget.Texture2D, output.Texture );

			for ( int i = 1; i < numLevels; i++ )
			{
				var frameBuffer = frameBuffers[ i ];

				GL.BindFramebuffer( FramebufferTarget.Framebuffer, frameBuffer );
				GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, output.Texture, i );
				
				GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, frameBuffer );
				GL.DrawBuffers( 1, new [] { DrawBuffersEnum.ColorAttachment0 } );

				CheckFrameBuffer();

				GL.Viewport( 0, 0, outputSize >> i, outputSize >> i );
				downsampleShader.Render( output, i-1 );
			}

			// Reset the mip state of the output texture
			{
				//GL.BindTexture( TextureTarget.Texture2D, output.Texture );
				//int min = 0;
				//int max = numLevels;
				//GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, ref min );
				//GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, ref max );
			}

			GL.BindTexture( TextureTarget.Texture2D, 0 );
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 );
		}

		public SquareTexture2D Output
		{
			get
			{
				return output;
			}
		}

		static void CheckFrameBuffer()
		{
			var status = GL.CheckFramebufferStatus( FramebufferTarget.Framebuffer );
			if ( status != FramebufferErrorCode.FramebufferComplete )
			{
				Debug.WriteLine( status );
			}
		}
	}
}

