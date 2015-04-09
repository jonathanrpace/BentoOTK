using System;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Textures
{
	public class RandomDirectionTexture : IGraphicsContextDependant
	{
		int texture;
		int width = 256;
		int height = 256;
		int seed = 0;
		bool isInvalid = false;
		bool hasContext = false;

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
				isInvalid = true;
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
				isInvalid = true;
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
				isInvalid = true;
			}
		}

		public RandomDirectionTexture()
		{
			
		}

		#region IGraphicsContextDependant implementation

		public void CreateGraphicsContextResources()
		{
			hasContext = true;

			texture = GL.GenTexture();
			GL.BindTexture( TextureTarget.Texture2D, texture );
			GL.ActiveTexture( TextureUnit.Texture0 );
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

		public void DisposeGraphicsContextResources()
		{
			if ( GL.IsTexture( texture ) )
			{
				GL.DeleteTexture( texture );
			}

			hasContext = false;
		}

		#endregion

		private void validate()
		{
			if ( !isInvalid )
				return;

			if ( hasContext )
			{
				DisposeGraphicsContextResources();
				CreateGraphicsContextResources();
			}

			isInvalid = false;
		}
	}
}

