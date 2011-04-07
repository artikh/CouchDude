using System;

namespace CouchDude.Core
{
	/// <summary>CouchDB session interface.</summary>
	public interface ISession: IDisposable
	{
		/// <summary>Attaches entity to the session and saves it to
		/// CouchDB.</summary>
		DocumentInfo Save<TEntity>(TEntity entity) where TEntity : class;

		/// <summary>Loads entity from CouchDB placing in to first level cache.</summary>
		TEntity Load<TEntity>(string docId) where TEntity : class;

		/// <summary>Finds entity using provided view.</summary>
		TEntity Find<TEntity>(ViewInfo view) where TEntity : class;

		/// <summary>Synchronises all changes to CouchDB.</summary>
		void Flush();

		/// <summary>Deletes provided entity form CouchDB.</summary>
		DocumentInfo Delete<TEntity>(TEntity entity) where TEntity : class;
	}
}