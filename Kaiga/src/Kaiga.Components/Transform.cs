using System;
using OpenTK;

namespace Kaiga.Components
{
	public class Transform
	{
		public Matrix4 Matrix { get; set; }

		public Transform()
		{
			Identity();
		}

		public Transform( Matrix4 matrix )
		{
			Matrix = matrix;
		}

		public void Identity()
		{
			Matrix = Matrix4.Identity;
		}

		public void Scale( float value )
		{
			Matrix *= Matrix4.CreateScale( value );
		}

		public void Translate( float x, float y, float z )
		{
			Matrix *= Matrix4.CreateTranslation( x, y, z );
		}

		public void RotateX( float radians )
		{
			Matrix *= Matrix4.CreateRotationX( radians );
		}

		public void RotateY( float radians )
		{
			Matrix *= Matrix4.CreateRotationY( radians );
		}

		public void RotateZ( float radians )
		{
			Matrix *= Matrix4.CreateRotationZ( radians );
		}

		public Vector3 Forward()
		{
			var forward = new Vector4( 0.0f, 0.0f, 1.0f, 1.0f );
			forward = Vector4.Transform( forward, Matrix );
			return forward.Xyz;
		}
	}
}

