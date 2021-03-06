﻿using OpenTK.Graphics.OpenGL4;
using System;
using Kaiga.Core;

namespace Kaiga.Textures
{
	public class RectangleTexture : AbstractValidatable, ITexture2D
	{
		#region Properties

		int width = 256;
		public int Width
		{
			get	{ return width; }
			set
			{
				if ( value == width ) return;
				width = value;
				invalidate();
			}
		}

		int height = 256;
		public int Height
		{
			get	{ return height; }
			set
			{
				if ( value == height ) return;
				height = value;
				invalidate();
			}
		}

		PixelInternalFormat internalFormat = PixelInternalFormat.Rgba;
		public PixelInternalFormat InternalFormat
		{
			get	{ return internalFormat; }
			set
			{
				if ( value == internalFormat ) return;
				invalidate();
				internalFormat = value;
			}
		}

		PixelFormat format = PixelFormat.Rgba;
		public PixelFormat Format
		{
			get	{ return format; }
			set
			{
				if ( value == format ) return;
				invalidate();
				format = value;
			}
		}

		PixelType pixelType = PixelType.Float;
		public PixelType PixType
		{
			get	{ return pixelType; }
			set
			{
				if ( value == pixelType ) return;
				invalidate();
				pixelType = value;
			}
		}

		TextureMagFilter magFilter = TextureMagFilter.Linear;
		public TextureMagFilter MagFilter
		{
			get	{ return magFilter;	}
			set
			{
				if ( value == magFilter ) return;
				invalidate();
				magFilter = value;
			}
		}

		TextureMinFilter minFilter = TextureMinFilter.Linear;
		public TextureMinFilter MinFilter
		{
			get	{ return minFilter;	}
			set
			{
				if ( value == minFilter ) return;
				invalidate();
				minFilter = value;
			}
		}

		int texture = -1;
		public int Texture
		{
			get
			{
				validate();
				return texture;
			}
		}

		public void SetSize( int width, int height )
		{
			Width = width;
			Height = height;
		}

		#endregion

		public RectangleTexture( PixelInternalFormat internalFormat = PixelInternalFormat.Rgba, int width = 256, int height = 256, PixelFormat format = PixelFormat.Rgba, PixelType pixelType = PixelType.Float )
		{
			InternalFormat = internalFormat;
			Width = width;
			Height = height;
			Format = format;
			PixType = pixelType;
		}

		override protected void onInvalidate()
		{
			if ( GL.IsTexture( texture ) )
			{
				GL.DeleteTexture( texture );
				texture = -1;
			}
		}

		override protected void onValidate()
		{
			texture = GL.GenTexture();

			GL.BindTexture( TextureTarget.TextureRectangle, texture );

			GL.TexImage2D( TextureTarget.TextureRectangle, 0, internalFormat, width, height, 0, format, 
				pixelType, new IntPtr(0) );
			
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, (int)MagFilter);
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMinFilter, (int)MinFilter );

			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge );
			GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge );

			GL.BindTexture( TextureTarget.TextureRectangle, 0 );
		}
	}
}

