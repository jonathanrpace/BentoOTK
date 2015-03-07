using System;

namespace Ramen
{
	public interface IProcess
	{
		void OnAddedToScene( Scene scene );
		void OnRemovedFromScene( Scene scene );
		void Update( double dt );
		string Name { get; }
	}
}

