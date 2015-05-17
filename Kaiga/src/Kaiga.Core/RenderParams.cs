using System;
using Kaiga.Components;
using OpenTK;
using Kaiga.Textures;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Core
{
	public class RenderParams : IDisposable
	{
		// Render targets
		readonly public DeferredRenderTarget RenderTarget;
		readonly public LightTransportRenderTarget LightTransportRenderTarget;

		public int BackBufferWidth;
		public int BackBufferHeight;
		public int LightTransportBufferWidth;
		public int LightTransportBuffferHeight;

		// Rectangular buffers
		readonly public RectangleTexture NormalTextureRect;
		readonly public RectangleTexture PositionTextureRect;
		readonly public RectangleTexture AlbedoTextureRect;
		readonly public RectangleTexture MaterialTextureRect;
		readonly public RectangleTexture DirectLightTextureRect;
		readonly public RectangleTexture IndirectLightTextureRect;
		readonly public RectangleTexture OutputTextureRect;

		// Square buffers
		public SquareTexture2D DirectLightTexture2D;
		public SquareTexture2D IndirectLightTexture2D;
		public SquareTexture2D PositionTexture2D;
		public SquareTexture2D NormalTexture2D;

		// Transforms
		public ILens CameraLens;
		public Vector3 CameraPosition;
		public Vector3 CameraForward;
		public Matrix4 ViewMatrix;
		public Matrix4 InvViewMatrix;
		public Matrix3 NormalInvViewMatrix;
		public Matrix4 NormalViewMatrix;
		public Matrix4 ProjectionMatrix;
		public Matrix4 InvProjectionMatrix;
		public Matrix4 ViewProjectionMatrix;
		public Matrix4 InvViewProjectionMatrix;
		public Matrix4 ModelMatrix;
		public Matrix4 ModelViewMatrix;
		public Matrix4 ModelViewProjectionMatrix;
		public Matrix3 NormalModelMatrix;
		public Matrix3 NormalModelViewMatrix;
		public Matrix3 InvNormalModelViewMatrix;
		public Matrix4 NormalViewProjectionMatrix;

		// Other
		public float LightTransportResolutionScalar = 0.5f;

		public RenderParams()
		{
			RenderTarget = new DeferredRenderTarget();
			LightTransportRenderTarget = new LightTransportRenderTarget();

			NormalTextureRect = RenderTarget.NormalBuffer;
			PositionTextureRect = RenderTarget.PositionBuffer;
			AlbedoTextureRect = RenderTarget.AlbedoBuffer;
			MaterialTextureRect = RenderTarget.MaterialBuffer;
			DirectLightTextureRect = RenderTarget.DirectLightBuffer;
			IndirectLightTextureRect = RenderTarget.IndirectLightBuffer;
			OutputTextureRect = RenderTarget.OutputBuffer;
		}

		public void Dispose()
		{
			RenderTarget.Dispose();
			LightTransportRenderTarget.Dispose();
		}

		public void SetModelMatrix( Matrix4 modelMatrix )
		{
			ModelMatrix = modelMatrix;
			ModelViewMatrix = modelMatrix * ViewMatrix;
			ModelViewProjectionMatrix = ModelViewMatrix * ProjectionMatrix;

			NormalModelMatrix = new Matrix3( ModelMatrix );
			NormalModelMatrix.Transpose();

			NormalModelViewMatrix = new Matrix3( ModelViewMatrix );
			NormalModelViewMatrix.Transpose();

			InvNormalModelViewMatrix = NormalModelViewMatrix.Inverted();
		}

	}
}

