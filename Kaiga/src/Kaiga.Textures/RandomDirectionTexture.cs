﻿using System;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Textures
{
	public class RandomDirectionTexture : AbstractValidatable
	{
		int texture;
		int width = 256;
		int height = 256;
		int seed = 0;

		public int Texture
		{
			get
			{
				validate();
				return texture;
			}
		}

		public int Width
		{ 
			get
			{
				return width;
			}
			set
			{
				width = value;
				invalidate();
			}
		}

		public int Height
		{ 
			get
			{
				return height;
			}
			set
			{
				height = value;
				invalidate();
			}
		}

		public int Seed
		{
			get
			{
				return seed;
			}
			set
			{
				seed = value;
				invalidate();
			}
		}
			
		override protected void onInvalidate()
		{
			if ( GL.IsTexture( texture ) )
			{
				GL.DeleteTexture( texture );
			}
		}

		override protected void onValidate()
		{
			texture = GL.GenTexture();
			GL.BindTexture( TextureTarget.Texture2D, texture );
			GL.TexStorage2D( TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, width, height );
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest );
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat );
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat );

			int size = width * height * 3;
			var data = new float[size];
			var rand = new Random( seed );
			for ( int i = 0; i < size; i++ )
			{
				data[ i ] = ( (float)rand.NextDouble() - 0.5f ) * 2.0f;
			}

			GL.TexSubImage2D( TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Rgb, PixelType.Float, data );
		}
	}
}
