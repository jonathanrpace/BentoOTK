using System;
using Kaiga.Textures;

namespace Kaiga.Lights
{
	public class ImageLight
	{
		public ICubeTexture Texture;
		public float Intensity;

		public ImageLight( float Intensity = 1.0f )
		{
			this.Intensity = Intensity;
		}
	}
}

