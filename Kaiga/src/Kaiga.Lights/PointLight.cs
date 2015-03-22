using OpenTK;

namespace Kaiga.Lights
{
	public class PointLight
	{
		public Vector3 Color;
		public float Intensity;
		public float AttenuationLinear;
		public float AttenuationExp;

		public PointLight
		(
			Vector3 Color, 
			float Intensity = 1.0f, 
			float AttenuationLinear = 1.0f, 
			float AttenuationExp = 1.0f )
		{
			this.Color = Color;
			this.Intensity = Intensity;
			this.AttenuationLinear = AttenuationLinear;
			this.AttenuationExp = AttenuationExp;
		}
	}
}

