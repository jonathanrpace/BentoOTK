using System;
using OpenTK;

namespace Kaiga.Components
{
	public class SwarmMember
	{
		public Vector3 Radius;
		public Vector3 Speed;

		public SwarmMember() : this
		(
			new Vector3( 1.0f, 1.0f, 1.0f ),
			new Vector3( 0.1f, 0.1f, 0.1f )
		) 
		{}

		public SwarmMember( Vector3 Radius, Vector3 Speed )
		{
			this.Radius = Radius;
			this.Speed = Speed;
		}
	}
}

