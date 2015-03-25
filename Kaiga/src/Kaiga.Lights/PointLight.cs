﻿using OpenTK;

namespace Kaiga.Lights
{
	public class PointLight
	{
		public Vector3 Color;
		public float Intensity;
		public float AttenuationConstant;
		public float AttenuationLinear;
		public float AttenuationExp;

		public PointLight
		(
			Vector3 Color, 
			float Intensity = 1.0f, 
			float AttenuationConstant = 0.0f,
			float AttenuationLinear = 0.0f, 
			float AttenuationExp = 1.0f )
		{
			this.Color = Color;
			this.Intensity = Intensity;
			this.AttenuationConstant = AttenuationConstant;
			this.AttenuationLinear = AttenuationLinear;
			this.AttenuationExp = AttenuationExp;
		}

		public PointLight
		( 
			float r = 1.0f, float g = 1.0f, float b = 1.0f,
			float Intensity = 1.0f, 
			float AttenuationConstant = 0.0f,
			float AttenuationLinear = 0.0f, 
			float AttenuationExp = 1.0f
		) : 
		this( new Vector3( r, g, b ), 
			Intensity,
			AttenuationConstant,
			AttenuationLinear,
			AttenuationExp
		)
		{ }
	}
}
