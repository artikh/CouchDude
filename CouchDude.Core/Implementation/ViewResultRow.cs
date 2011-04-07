using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.Implementation
{
	/// <summary>CouchDB query result row.</summary>
	public class ViewResultRow
	{
		/// <summary>View key.</summary>
		[JsonProperty("key")]
		public JToken Key { get; protected internal set; }

		/// <summary>View value.</summary>
		[JsonProperty("value")]
		public JToken Value { get; protected internal set; }

		/// <summary>Document ID associated with view row.</summary>
		[JsonProperty("id")]
		public string DocumentId { get; protected internal set; }

		/// <summary>Document associated with the row.</summary>
		[JsonProperty("doc")]
		public JObject Document { get; protected internal set; }
	}
}