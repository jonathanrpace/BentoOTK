using System;
using Kaiga.Components;
using OpenTK;

namespace Kaiga.Core
{
	public class RenderParams
	{
		public IRenderTarget RenderTarget;
		public ILens CameraLens;
		public Vector3 CameraPosition;
		public Vector3 CameraForward;
		public Matrix4 ViewMatrix;
		public Matrix4 NormalViewMatrix;
		public Matrix4 ProjectionMatrix;
		public Matrix4 InvProjectionMatrix;
		public Matrix4 ViewProjectionMatrix;
		public Matrix4 InvViewProjectionMatrix;
		public Matrix4 ModelMatrix;
		public Matrix4 ModelViewMatrix;
		public Matrix4 ModelViewProjectionMatrix;
		public Matrix4 NormalMatrix;
		public Matrix4 NormalModelViewMatrix;
		public Matrix4 InvViewMatrix;
		public Matrix4 InvNormalViewMatrix;




		public RenderParams()
		{

		}
	}
}

