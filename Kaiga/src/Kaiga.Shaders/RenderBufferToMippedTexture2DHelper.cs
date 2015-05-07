﻿using System;
using Kaiga.Textures;
using Kaiga.Core;
using Kaiga.Util;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders
{
	public class RenderBufferToMippedTexture2DHelper : IDisposable
	{
		readonly ScreenQuadTextureRectShader rectTo2DShader;
		readonly SquareTexture2D output;
		readonly int frameBuffer;


		public SquareTexture2D Output
		{
			get
			{
				return output;
			}
		}

		public RenderBufferToMippedTexture2DHelper()
		{
			rectTo2DShader = new ScreenQuadTextureRectShader();
			output = new SquareTexture2D();
			output.MinFilter = TextureMinFilter.NearestMipmapNearest;
			output.MagFilter = TextureMagFilter.Linear;
			output.WrapModeR = output.WrapModeS = TextureWrapMode.ClampToEdge;

			frameBuffer = GL.GenFramebuffer();
		}

		public void Dispose()
		{
			rectTo2DShader.Dispose();
			output.Dispose();

			GL.DeleteFramebuffer( frameBuffer );
		}

		public void Render( RenderParams renderParams, RectangleTexture source )
		{
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, frameBuffer );

			int outputSize = TextureUtil.GetBestPowerOf2( Math.Min( source.Width, source.Height ) >> 1 );
			if ( output.Width != outputSize || output.InternalFormat != source.InternalFormat )
			{
				output.SetSize( outputSize, outputSize );
				output.InternalFormat = source.InternalFormat;
				
				GL.FramebufferTexture2D( FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, output.Texture, 0 );
			}

			GL.Viewport( 0, 0, outputSize, outputSize );

			rectTo2DShader.BindPipeline( renderParams );
			rectTo2DShader.Render( renderParams, source.Texture );
			rectTo2DShader.UnbindPipeline();
			output.GenerateMipMaps();

			GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 );
		}

	}
}