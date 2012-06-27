using System;
using System.Threading;

namespace CouchDude.Utils
{
	/// <summary>Switches threads context and back.</summary>
	public struct SyncContext: IDisposable
	{
		readonly SynchronizationContext oldContext;

		/// <summary>Switches <see cref="SynchronizationContext"/> of current thread and returns
		/// <see cref="IDisposable"/> implementation returning everything back on <see cref="Dispose"/>
		/// method call.</summary>
		public static SyncContext SwitchToDefault() { return new SyncContext(null); }

		/// <summary>Switches <see cref="SynchronizationContext"/> of current thread and returns
		/// <see cref="IDisposable"/> implementation returning everything back on <see cref="Dispose"/>
		/// method call.</summary>
		public static SyncContext SwitchTo(SynchronizationContext newContext = null) { return new SyncContext(); }
		
		/// <constructor />
		private SyncContext(SynchronizationContext newContext)
		{
			oldContext = SynchronizationContext.Current;
			if(oldContext == SynchronizationContext.Current)
				SynchronizationContext.SetSynchronizationContext(newContext);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			SynchronizationContext.SetSynchronizationContext(oldContext);
		}
	}
}