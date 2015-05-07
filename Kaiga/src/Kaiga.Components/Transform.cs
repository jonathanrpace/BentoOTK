using System;
using OpenTK;

namespace Kaiga.Components
{
	public class Transform
	{
		public Matrix4 Matrix { get; set; }

		public Transform()
		{
			Matrix = Matrix4.Identity;
		}

		public Transform( Matrix4 matrix )
		{
			Matrix = matrix;
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
	}
}

