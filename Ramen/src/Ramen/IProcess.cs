using System;

namespace Ramen
{
	public interface IProcess : IDisposable
	{
		void OnAddedToScene( Scene scene );
		void OnRemovedFromScene( Scene scene );
		void Update( double dt );
		string Name { get; }
	}
}

