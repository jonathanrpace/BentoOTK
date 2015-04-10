using System;

namespace Kaiga.Core
{
	abstract public class AbstractValidatable : IDisposable
	{
		bool isInvalid = true;

		public virtual void Dispose()
		{
			invalidate();
		}

		protected void invalidate()
		{
			if ( isInvalid )
				return;
			onInvalidate();
			isInvalid = true;
		}

		protected void validate()
		{
			if ( !isInvalid )
				return;
			onValidate();
			isInvalid = false;
		}

		abstract protected void onValidate();
		abstract protected void onInvalidate();
	}
}

