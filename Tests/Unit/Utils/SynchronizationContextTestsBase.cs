using System;
using System.Threading;
using CouchDude.Utils;
using Xunit;

namespace CouchDude.Tests.Unit.Utils
{
	/// <summary>Special tests ensuring nothing is posted to current concurrency on CouchDude async methods invocation.</summary>
	public abstract class SynchronizationContextTestsBase: IDisposable
	{
		protected Action<SendOrPostCallback> HandlePostOrSend;
		protected int Send;
		protected int Posted;
		protected bool HavePosted { get { return Posted > 0; } }
		protected bool HaveSend { get { return Send > 0; } }
		protected bool HaveSendOrPosted { get { return HavePosted || HaveSend; } }
		protected int SendAndPosted { get { return Posted + Send; } }
		protected SynchronizationContext TestContext;

		protected SynchronizationContextTestsBase()
		{
			SynchronizationContext.SetSynchronizationContext(TestContext = new TestSynchronizationContext(this));
		}
		
		/// <inheritdoc />
		public void Dispose()
		{
			SynchronizationContext.SetSynchronizationContext(null);
		}

		class TestSynchronizationContext: SynchronizationContext
		{
			readonly SynchronizationContextTestsBase parent;
			public TestSynchronizationContext(SynchronizationContextTestsBase parent) { this.parent = parent; }
			
			/// <summary>Dispatches an asynchronous message to a synchronization context.</summary>
			/// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback"/> delegate to call.</param>
			/// <param name="state">The object passed to the delegate.</param>
			public override void Post(SendOrPostCallback d, object state)
			{
				Interlocked.Increment(ref parent.Posted);
				if (parent.HandlePostOrSend != null)
					parent.HandlePostOrSend(d);
				using (SyncContext.SwitchTo(this))
					d(state);
			}

			/// <summary>Dispatches a synchronous message to a synchronization context.</summary>
			/// <param name="d">The <see cref="SendOrPostCallback"/> delegate to call.</param>
			/// <param name="state">The object passed to the delegate.</param>
			public override void Send(SendOrPostCallback d, object state)
			{
				Interlocked.Increment(ref parent.Send);
				if (parent.HandlePostOrSend != null)
					parent.HandlePostOrSend(d);
				using (SyncContext.SwitchTo(this))
					d(state);
			}
		}

		protected void AssertNonePosted()
		{
			Assert.Same(TestContext, SynchronizationContext.Current);
			Assert.Equal(0, SendAndPosted);
		}
	}
}
