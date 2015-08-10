using System;
using OpenTK;

namespace Kaiga.Materials
{
	public class StandardMaterial
	{
		public float Reflectivity = 0.8f;
		public float Roughness = 0.5f;
		public float Emissive = 0.0f;
		public Vector3 Diffuse;

		public StandardMaterial()
		{
			Diffuse = new Vector3( 0.5f, 0.5f, 0.5f );
		}
	}
}

