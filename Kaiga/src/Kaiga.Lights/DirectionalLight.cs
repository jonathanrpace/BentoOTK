using OpenTK;

namespace Kaiga.Lights
{
	public class DirectionalLight
	{
		public Vector3 Color;
		public float Intensity;
		public float Radius;

		public DirectionalLight
		(
			Vector3 Color, 
			float Intensity = 1.0f, 
			float Radius = 1.0f
		)
		{
			this.Color = Color;
			this.Intensity = Intensity;
			this.Radius = Radius;
		}

		public DirectionalLight
		( 
			float r = 1.0f, 
			float g = 1.0f, 
			float b = 1.0f,
			float Intensity = 1.0f, 
			float Radius = 1.0f
		) 
		: this
		( 
			new Vector3( r, g, b ), 
			Intensity,
			Radius
		)
		{
		}
	}
}

