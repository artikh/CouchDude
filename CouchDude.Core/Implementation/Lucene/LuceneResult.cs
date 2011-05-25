using System.Collections.Generic;
using Newtonsoft.Json;

namespace CouchDude.Core.Implementation.Lucene
{
	/// <summary>Result of CouchDB view query.</summary>
	public class LuceneResult
	{
		/// <constructor />
		public LuceneResult()
		{
			Rows = new List<LuceneResultRow>();
		}

		/// <summary>Query used to produce result.</summary>
		[JsonIgnore]
		public LuceneQuery Query { get; internal set; }

		/// <summary>Total rows in requested range.</summary>
		[JsonProperty("total_rows")]
		public int TotalRows { get; internal set; }
		
		/// <summary>Selected rows.</summary>
		[JsonProperty("rows")]
		public IList<LuceneResultRow> Rows { get; protected set; }
	}
}
