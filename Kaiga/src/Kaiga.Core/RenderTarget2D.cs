using System;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Core
{
	public class RenderTarget2D : IRenderTarget
	{
		public int FrameBuffer { get; private set; }

		public int Width { get; private set; }
		public int Height { get; private set; }

		public int DepthBuffer { get; private set; }
		public int NormalBuffer { get; private set; }
		public int PositionBuffer { get; private set; }
		public int LBuffer { get; private set; }
		public int MaterialBuffer { get; private set; }
		public int PostBuffer { get; private set; }

		public RenderTarget2D()
		{

		}

		public void SetSize( int width, int height )
		{
			if ( width == Width && height == Height )
			{
				return;
			}

			Width = width;
			Height = height;

			// Dispose existing frameBuffer
			if ( GL.IsFramebuffer( FrameBuffer ) )
			{
				GL.DeleteFramebuffer( FrameBuffer );

				GL.DeleteRenderbuffer( DepthBuffer );
				GL.DeleteRenderbuffer( NormalBuffer );
				GL.DeleteRenderbuffer( PositionBuffer );
				GL.DeleteRenderbuffer( LBuffer );
				GL.DeleteRenderbuffer( MaterialBuffer );
				GL.DeleteRenderbuffer( PostBuffer );
			}

			DepthBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, DepthBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height );

			NormalBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, NormalBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height );

			PositionBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, PositionBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height );

			LBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, LBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height );

			MaterialBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, MaterialBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height );

			PostBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, PostBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height );

			FrameBuffer = GL.GenFramebuffer();

		}
	}
}

