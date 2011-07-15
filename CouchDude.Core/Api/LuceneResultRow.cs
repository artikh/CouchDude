using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.Api
{
	/// <summary>CouchDB-lucene query result row.</summary>
	public class LuceneResultRow
	{
		/// <summary>The unique identifier for this match.</summary>
		[JsonProperty("id")]
		public string DocumentId { get; protected internal set; }

		/// <summary>All the fields that were stored with this match</summary>
		[JsonProperty("fields")]
		public JObject Fields { get; protected internal set; }

		/// <summary>The normalized score (0.0-1.0, inclusive) for this match.</summary>
		[JsonProperty("score")]
		public JToken Score { get; protected internal set; }

		/// <summary>Document associated with the row.</summary>
		[JsonProperty("doc")]
		public JObject Document { get; protected internal set; }
	}
}