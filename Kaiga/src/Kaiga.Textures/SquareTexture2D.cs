using OpenTK.Graphics.OpenGL4;
using System;
using Kaiga.Core;
using System.Diagnostics;
using Kaiga.Util;

namespace Kaiga.Textures
{
	public class SquareTexture2D : AbstractValidatable, ITexture2D
	{
		#region Properties

		int width = 256;
		public int Width
		{
			get	{ return width; }
			set
			{
				if ( value == width ) return;
				Debug.Assert( TextureUtil.IsDimensionValid( value ) );
				width = value;
				height = value;
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
				Debug.Assert( TextureUtil.IsDimensionValid( value ) );
				height = value;
				width = value;
				invalidate();
			}
		}

		public int NumMipMaps
		{
			get
			{
				if ( MinFilter == TextureMinFilter.LinearMipmapLinear ||
					MinFilter == TextureMinFilter.NearestMipmapLinear ||
					MinFilter == TextureMinFilter.NearestMipmapNearest ||
					MinFilter == TextureMinFilter.LinearMipmapNearest )
				{
					return TextureUtil.GetNumMipMaps( Width );
				}
				else
				{
					return 1;
				}
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

		TextureWrapMode wrapModeR = TextureWrapMode.Repeat;
		public TextureWrapMode WrapModeR
		{
			get	{ return wrapModeR;	}
			set
			{
				if ( value == wrapModeR ) return;
				invalidate();
				wrapModeR = value;
			}
		}

		TextureWrapMode wrapModeS = TextureWrapMode.Repeat;
		public TextureWrapMode WrapModeS
		{
			get	{ return wrapModeS;	}
			set
			{
				if ( value == wrapModeS ) return;
				invalidate();
				wrapModeS = value;
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
			Debug.Assert( width == height, "Dimensions must match for SquareTexture2D" );
			Debug.Assert( TextureUtil.IsDimensionValid( width ) );
			Debug.Assert( TextureUtil.IsDimensionValid( height ) );

			Width = width;
			Height = height;
		}

		public void GenerateMipMaps()
		{
			validate();
			GL.BindTexture( TextureTarget.Texture2D, Texture );
			GL.GenerateMipmap( GenerateMipmapTarget.Texture2D );
			GL.BindTexture( TextureTarget.Texture2D, 0 );
		}

		#endregion

		public SquareTexture2D( PixelInternalFormat internalFormat = PixelInternalFormat.Rgba, int size = 256 )
		{
			InternalFormat = internalFormat;
			Width = size;
			Height = size;
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

			GL.BindTexture( TextureTarget.Texture2D, texture );

			int numMipmaps = NumMipMaps;
			int d = Width;
			for ( int i = 0; i < numMipmaps; i++ )
			{
				GL.TexImage2D
				(
					TextureTarget.Texture2D, 
					i, 
					internalFormat, 
					d, 
					d, 
					0, 
					PixelFormat.Rgba, 
					PixelType.Float, new IntPtr(0) 
				);

				d >>= 1;
			}

			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter );

			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)wrapModeR );
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapModeS );

			GL.BindTexture( TextureTarget.Texture2D, 0 );
		}
	}
}

