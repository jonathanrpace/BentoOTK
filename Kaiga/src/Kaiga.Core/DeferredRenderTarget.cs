using System;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using Kaiga.Textures;

namespace Kaiga.Core
{
	public class DeferredRenderTarget : AbstractRenderTarget
	{
		public RectangleTexture NormalBuffer { get; private set; }
		public RectangleTexture PositionBuffer { get; private set; }
		public RectangleTexture AlbedoBuffer { get; private set; }
		public RectangleTexture MaterialBuffer { get; private set; }
		public RectangleTexture OutputBuffer { get; private set; }
		public RectangleTexture PostBuffer { get; private set; }
		
		DrawBuffersEnum[] gPhaseDrawBuffers = { 
			DrawBufferName.Position, 
			DrawBufferName.Normal, 
			DrawBufferName.Albedo, 
			DrawBufferName.Material
		};

		DrawBuffersEnum[] lightPhaseDrawBuffers = { 
			DrawBufferName.Output
		};

		DrawBuffersEnum[] allDrawBuffers = { 
			DrawBufferName.Position, 
			DrawBufferName.Normal, 
			DrawBufferName.Albedo, 
			DrawBufferName.Material,
			DrawBufferName.Output,
			DrawBufferName.Post
		};

		public DeferredRenderTarget() :
		base( PixelInternalFormat.Rgba16f, true, true )
		{
			PositionBuffer = new RectangleTexture( internalFormat );
			NormalBuffer = new RectangleTexture( internalFormat );
			AlbedoBuffer = new RectangleTexture( internalFormat );
			MaterialBuffer = new RectangleTexture( internalFormat );
			OutputBuffer = new RectangleTexture( internalFormat );
			PostBuffer = new RectangleTexture( internalFormat );

			AttachTexture( FBAttachmentName.Position, PositionBuffer );
			AttachTexture( FBAttachmentName.Normal, NormalBuffer );
			AttachTexture( FBAttachmentName.Albedo, AlbedoBuffer );
			AttachTexture( FBAttachmentName.Material, MaterialBuffer );
			AttachTexture( FBAttachmentName.Output, OutputBuffer );
			AttachTexture( FBAttachmentName.Post, PostBuffer );
		}

		public void Clear()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( allDrawBuffers.Length, allDrawBuffers );
			GL.ClearColor( Color.Black );
			GL.ClearDepth( 1.0 );
			GL.ClearStencil( 0 );
			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit );
		}

		public void BindForGPhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( gPhaseDrawBuffers.Length, gPhaseDrawBuffers );
		}

		public void BindForLightPhase()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffers( lightPhaseDrawBuffers.Length, lightPhaseDrawBuffers );
		}

		public void BindForNoDraw()
		{
			validate();

			GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, FrameBuffer );
			GL.DrawBuffer( DrawBufferMode.None );
		}
	}
}

