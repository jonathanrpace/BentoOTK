using System;
using Kaiga.ShaderStages;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders
{
	public class BoxBlurShader : AbstractShader
	{
		new readonly BoxBlurFragShader fragmentShader;

		int bufferTextureWidth;
		int bufferTextureHeight;

		int bufferTexture;

		public BoxBlurShader() : base( new ScreenQuadVertexShader(), new BoxBlurFragShader() )
		{
			fragmentShader = (BoxBlurFragShader)base.fragmentShader;
			bufferTextureWidth = 0;
			bufferTextureHeight = 0;
		}

		public void Render( int texture )
		{
			int newBufferWidth;
			int newBufferHeight;

			GL.BindTexture( TextureTarget.TextureRectangle, texture );
			GL.GetTexParameterI( TextureTarget.TextureRectangle, GetTextureParameter.TextureWidth, out newBufferWidth );
			GL.GetTexParameterI( TextureTarget.TextureRectangle, GetTextureParameter.TextureWidth, out newBufferHeight );

			if ( newBufferWidth != bufferTextureWidth || newBufferHeight != bufferTextureHeight )
			{
				if ( GL.IsTexture( bufferTexture ) )
				{
					GL.DeleteTexture( bufferTexture );
				}

				bufferTextureWidth = newBufferWidth;
				bufferTextureHeight = newBufferHeight;
				
				bufferTexture = GL.GenTexture();
				GL.BindTexture( TextureTarget.TextureRectangle, bufferTexture );
				GL.TexImage2D( TextureTarget.TextureRectangle, 0, PixelInternalFormat.R8, bufferTextureWidth, bufferTextureHeight, 0, PixelFormat.Rgba, PixelType.Float, new IntPtr(0) );
				GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
				GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge );
				GL.TexParameter( TextureTarget.TextureRectangle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge );
			}


		}
	}
}

