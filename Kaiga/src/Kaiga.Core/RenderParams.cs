using System;
using Kaiga.Components;
using OpenTK;

namespace Kaiga.Core
{
	public class RenderParams
	{
		public DeferredRenderTarget RenderTarget;
		public ILens CameraLens;
		public Vector3 CameraPosition;
		public Vector3 CameraForward;
		public Matrix4 ViewMatrix;
		public Matrix4 InvViewMatrix;
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

		public RenderParams()
		{

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

