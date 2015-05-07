using System;

namespace Kaiga.Components
{
	public class EmissivePulser
	{
		public float Frequency;
		public float Min;
		public float Max;
		public float Offset;

		public EmissivePulser( float Frequency = 1.0f, float Min = 0.0f, float Max = 1.0f, float Offset = 0.0f )
		{
			this.Frequency = Frequency;
			this.Min = Min;
			this.Max = Max;
			this.Offset = Offset;
		}
	}
}

