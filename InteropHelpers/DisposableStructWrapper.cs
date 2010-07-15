using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xiph.InteropHelpers
{
	public abstract class DisposableStructWrapper<T> : StructWrapper<T>, IDisposable
		where T : struct
	{
		public bool IsDisposed { get; private set; }

		protected abstract void DisposeOverride();

		public void Dispose()
		{
			DisposeOverride();
			IsDisposed = true;
		}
	}
}
