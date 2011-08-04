namespace CouchDude.Core
{
	/// <summary>Common interface of query result row.</summary>
	public interface IQueryResultRow
	{
		/// <summary>Query row main value.</summary>
		IJsonFragment Value { get; }

		/// <summary>Document ID associated with row.</summary>
		string DocumentId { get; }

		/// <summary>Document associated with the row.</summary>
		IDocument Document { get; }
	}
}