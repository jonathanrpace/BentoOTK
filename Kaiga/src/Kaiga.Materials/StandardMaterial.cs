using System;
using OpenTK;

namespace Kaiga.Materials
{
	public class StandardMaterial
	{
		public float Reflectivity = 1.0f;
		public float Roughness = 1.0f;
		public float Emissive = 0.0f;
		public Vector3 Diffuse;

		public StandardMaterial()
		{
			Diffuse = new Vector3( 1.0f, 1.0f, 1.0f );
		}
	}
}

