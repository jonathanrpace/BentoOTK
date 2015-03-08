namespace Kaiga.Core
{
	public interface IGraphicsContextDependant
	{
		void CreateGraphicsContextResources();
		void DisposeGraphicsContextResources();
	}
}

