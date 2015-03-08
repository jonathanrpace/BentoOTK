using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;

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

		bool invalid = true;
		
		public void CreateGraphicsContextResources()
		{
			Invalidate();
		}

		public void DisposeGraphicsContextResources()
		{
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

			invalid = true;
		}

		public void SetSize( int width, int height )
		{
			if ( width == Width && height == Height )
			{
				return;
			}

			Width = width;
			Height = height;

			Invalidate();
		}

		public void Bind()
		{
			if ( invalid )
			{
				Validate();
				invalid = false;
			}
		}

		void Invalidate()
		{
			DisposeGraphicsContextResources();
			invalid = true;
		}

		void Validate()
		{
			DepthBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, DepthBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Width, Height );

			NormalBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, NormalBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, Width, Height );

			PositionBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, PositionBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, Width, Height );

			LBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, LBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, Width, Height );

			MaterialBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, MaterialBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, Width, Height );

			PostBuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, PostBuffer );
			GL.RenderbufferStorage( RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, Width, Height );

			FrameBuffer = GL.GenFramebuffer();
		}
	}
}

