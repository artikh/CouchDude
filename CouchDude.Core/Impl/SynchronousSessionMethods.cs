using CouchDude.Core.Utils;

namespace CouchDude.Core.Impl
{
	/// <summary>Synchronous query methods for <see cref="ISession"/>.</summary>
	public class SynchronousSessionMethods: ISynchronousSessionMethods
	{
		private readonly ISession session;

		/// <constructor />
		public SynchronousSessionMethods(ISession session)
		{
			this.session = session;
		}

		/// <inheritdoc/>
		public IPagedList<T> Query<T>(ViewQuery<T> query)
		{
			return session.Query(query).WaitForResult();
		}

		/// <inheritdoc/>
		public IPagedList<T> FulltextQuery<T>(LuceneQuery<T> query) where T : class
		{
			return session.FulltextQuery(query).WaitForResult();
		}

		/// <inheritdoc/>
		public TEntity Load<TEntity>(string entityId) where TEntity : class
		{
			return session.Load<TEntity>(entityId).WaitForResult();
		}
	}
}