namespace Kaiga.Textures
{
	public interface ITexture2D : ITexture
	{
		void SetSize( int width, int height );
		int Width
		{
			get;
		}
		int Height
		{
			get;
		}
	}
}

