using Kaiga.Components;
using OpenTK;
using Kaiga.Textures;

namespace Kaiga.Core
{
	static public class RenderParams
	{
		// Render targets
		static readonly public DeferredRenderTarget RenderTarget;
		static readonly public LightTransportRenderTarget LightTransportRenderTarget;

		static public int BackBufferWidth;
		static public int BackBufferHeight;
		static public int LightTransportBufferWidth;
		static public int LightTransportBuffferHeight;

		// Rectangular buffers
		static readonly public RectangleTexture NormalTextureRect;
		static readonly public RectangleTexture PositionTextureRect;
		static readonly public RectangleTexture AlbedoTextureRect;
		static readonly public RectangleTexture MaterialTextureRect;
		static readonly public RectangleTexture DirectLightTextureRect;
		static readonly public RectangleTexture IndirectLightTextureRect;
		static readonly public RectangleTexture OutputTextureRect;

		// Square buffers
		static public SquareTexture2D DirectLightTexture2D;
		static public SquareTexture2D IndirectLightTexture2D;
		static public SquareTexture2D PositionTexture2D;
		static public SquareTexture2D NormalTexture2D;

		// Transforms
		static public ILens CameraLens;
		static public Vector3 CameraPosition;
		static public Vector3 CameraForward;
		static public Matrix4 ViewMatrix;
		static public Matrix4 InvViewMatrix;
		static public Matrix3 NormalInvViewMatrix;
		static public Matrix4 NormalViewMatrix;
		static public Matrix4 ProjectionMatrix;
		static public Matrix4 InvProjectionMatrix;
		static public Matrix4 ViewProjectionMatrix;
		static public Matrix4 InvViewProjectionMatrix;
		static public Matrix4 ModelMatrix;
		static public Matrix4 ModelViewMatrix;
		static public Matrix4 ModelViewProjectionMatrix;
		static public Matrix3 NormalModelMatrix;
		static public Matrix3 NormalModelViewMatrix;
		static public Matrix3 InvNormalModelViewMatrix;
		static public Matrix4 NormalViewProjectionMatrix;

		// Other
		static public float LightTransportResolutionScalar = 0.5f;

		static RenderParams()
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

		static public void SetModelMatrix( Matrix4 modelMatrix )
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

