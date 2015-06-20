using System;

namespace Kaiga.Geom
{
	public interface IGeometry : IDisposable
	{
		void Bind();
		void Draw();
	}
}

