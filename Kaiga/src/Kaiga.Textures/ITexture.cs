using System;

namespace Kaiga.Textures
{
	public interface ITexture
	{
		int Texture{ get; }
		void SetSize( int width, int height );
	}
}

