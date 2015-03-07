using System;
using OpenTK;

namespace Kaiga.Components
{
	public interface ILens
	{
		float AspectRatio { get; set; }
		Matrix4 ProjectionMatrix { get; }
		Matrix4 InvProjectionMatrix { get; }
	}
}

