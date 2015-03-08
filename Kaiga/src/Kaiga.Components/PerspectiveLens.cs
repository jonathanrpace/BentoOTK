using System;
using OpenTK;
using Ramen;

namespace Kaiga.Components
{
	public class PerspectiveLens : ILens, IMultiTypeObject
	{
		private bool matrixInvalid = true;
		private bool invMatrixInvalid = true;

		private float _verticalFOV;
		public float VerticalFOV
		{
			get
			{
				return _verticalFOV;
			}
			set
			{
				_verticalFOV = value;
				matrixInvalid = true;
				invMatrixInvalid = true;
			}
		}

		private float _aspectRatio = 1.0f;
		public float AspectRatio
		{
			get
			{
				return _aspectRatio;
			}
			set
			{
				_aspectRatio = value;
				matrixInvalid = true;
				invMatrixInvalid = true;
			}
		}

		public float HorizontalFOV
		{
			get
			{
				return _verticalFOV * AspectRatio;
			}
		}
		
		private float _near = 0.1f;
		public float Near
		{
			get
			{
				return _near;
			}
			set
			{
				_near = value;
				matrixInvalid = true;
				invMatrixInvalid = true;
			}
		}

		private float _far = 30f;
		public float Far
		{
			get
			{
				return _far;
			}
			set
			{
				_far = value;
				matrixInvalid = true;
				invMatrixInvalid = true;
			}
		}

		private Matrix4 _projectionMatrix;
		public OpenTK.Matrix4 ProjectionMatrix
		{
			get
			{
				if ( matrixInvalid )
				{
					Matrix4.CreatePerspectiveFieldOfView
					( 
						_verticalFOV, _aspectRatio, _near, _far, 
						out _projectionMatrix 
					);
					matrixInvalid = false;
				}
				return _projectionMatrix;
			}
		}

		private Matrix4 _invProjectionMatrix;
		public OpenTK.Matrix4 InvProjectionMatrix
		{
			get
			{
				if ( invMatrixInvalid )
				{
					_invProjectionMatrix = _projectionMatrix.Inverted();
					invMatrixInvalid = false;
				}
				return _invProjectionMatrix;
			}
		}

		public PerspectiveLens()
		{
			_verticalFOV = MathHelper.DegreesToRadians( 45 );
			_projectionMatrix = new Matrix4();
			_invProjectionMatrix = new Matrix4();
		}

		#region ITypedComponent implementation

		public Type[] Types
		{
			get
			{
				return new Type[] { typeof(PerspectiveLens), typeof(ILens) };
			}
		}

		#endregion
	}
}

