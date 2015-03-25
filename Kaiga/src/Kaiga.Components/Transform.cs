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

		public void TranslateBy( float x, float y, float z )
		{
			Matrix *= Matrix4.CreateTranslation( x, y, z );
		}
	}
}

