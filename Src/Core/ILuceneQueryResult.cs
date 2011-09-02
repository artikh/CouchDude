using System;

namespace CouchDude
{
	/// <summary>Typed CouchDB view query result.</summary>
	public interface ILuceneQueryResult<out T> : ILuceneQueryResult, IQueryResult<T, LuceneResultRow> { }

	/// <summary>CouchDB view query result.</summary>
	public interface ILuceneQueryResult : IQueryResult<LuceneResultRow>
	{
		/// <summary>Time spent retrieving documents.</summary>
		TimeSpan FetchDuration { get; }

		/// <summary>Time spent performing the search.</summary>
		TimeSpan SearchDuration { get; }

		/// <summary>Effective limit revised by couchdb-lucene.</summary>
		int Limit { get; }

		/// <summary>Number of initial matches skipped.</summary>
		int Skip { get; }

		/// <summary>Query used to produce current results set.</summary>
		LuceneQuery Query { get; }

		/// <summary>Returns next page query or <c>null</c> if instance represents last page of results.</summary>
		LuceneQuery NextPageQuery { get; }
	}
}