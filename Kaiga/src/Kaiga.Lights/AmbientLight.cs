using OpenTK;

namespace Kaiga.Lights
{
	public class AmbientLight
	{
		public Vector3 Color;
		public float Intensity;

		public AmbientLight
		(
			Vector3 Color, 
			float Intensity = 1.0f
		)
		{
			this.Color = Color;
			this.Intensity = Intensity;
		}

		public AmbientLight
		( 
			float r = 1.0f, 
			float g = 1.0f, 
			float b = 1.0f,
			float Intensity = 1.0f
		) 
		: this( new Vector3( r, g, b ), Intensity	)
		{
			
		}
	}
}

