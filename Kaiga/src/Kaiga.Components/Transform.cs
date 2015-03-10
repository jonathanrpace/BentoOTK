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
	}
}

