using OpenTK;

namespace Kaiga.Lights
{
	public class PointLight
	{
		public Vector3 Color;
		public float Intensity;
		public float AttenuationRadius;
		public float AttenuationPower;
		public float Radius;

		public PointLight
		(
			Vector3 Color, 
			float Intensity = 1.0f, 
			float Radius = 1.0f,
			float AttenuationRadius = 1.0f,
			float AttenuationPower = 2.0f
		)
		{
			this.Color = Color;
			this.Intensity = Intensity;
			this.Radius = Radius;
			this.AttenuationRadius = AttenuationRadius;
			this.AttenuationPower = AttenuationPower;
		}

		public PointLight
		( 
			float r = 1.0f, 
			float g = 1.0f, 
			float b = 1.0f,
			float Intensity = 1.0f, 
			float Radius = 1.0f,
			float AttenuationRadius = 1.0f,
			float AttenuationPower = 1.0f
		) 
		: this
		( 
			new Vector3( r, g, b ), 
			Intensity,
			Radius,
			AttenuationRadius,
			AttenuationPower
		)
		{
		}
	}
}

