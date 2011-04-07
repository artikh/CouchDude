using System.Collections.Generic;
using Newtonsoft.Json;

namespace CouchDude.Core.Implementation
{
	/// <summary>Result of CouchDB view query.</summary>
	public class ViewResult
	{
		/// <constructor />
		public ViewResult()
		{
			Rows = new List<ViewResultRow>();
		}

		/// <summary>Query used to produce result.</summary>
		[JsonIgnore]
		public ViewQuery Query { get; internal set; }

		/// <summary>Total rows in requested range.</summary>
		[JsonProperty("total_rows")]
		public int TotalRows { get; internal set; }

		/// <summary>Offset in requested range.</summary>
		[JsonProperty("offset")]
		public int Offset { get; internal set; }
		
		/// <summary>Selected rows.</summary>
		[JsonProperty("rows")]
		public IList<ViewResultRow> Rows { get; protected set; }
	}
}
