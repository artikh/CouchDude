using System.Text;
using System.Web;

namespace CouchDude.Core
{
	/// <summary>Класс типизирован так как могут возвращаться в том числе объекты из бд</summary>
	public class LuceneQuery<T>:LuceneQuery { }

	/// <summary>Fulltext query to couchdb-lucene</summary>
	public class LuceneQuery
	{
		/// <summary>Design document name (id without '_design/' prefix) to use view from.</summary>
		public string DesignDocumentName;

		/// <summary>Index view name.</summary>
		public string IndexName;

		/// <summary>Search query (see http://lucene.apache.org/java/2_4_0/queryparsersyntax.html) </summary>
		public string Query;

		/// <summary>Indicates that we need documents from couchdb in result</summary>
		public bool IncludeDocs;

		/// <summary>Name of analizer which is used for this query</summary>
		public string Analyzer;

		/*
		 * UNIMPLEMENTED
		/// <summary></summary>
		public string Callback;

		/// <summary></summary>
		public bool Debug;
		/// <summary></summary>
		public string DefaultOperator;

		/// <summary></summary>
		public bool ForceJson;

		 * /// <summary></summary>
		public string Sort;

		/// <summary></summary>
		public bool Stale;
		*/

		/// <summary>How many documents will be returned</summary>
		public int Limit;

		/// <summary>How many documents need to be skipped</summary>
		public int Skip;

		/// <constructor/>
		public LuceneQuery()
		{
			IncludeDocs = false;
			DesignDocumentName = "lucene";
			Limit = 100;
			Skip = 0;
			Analyzer = null;
		}

		/// <constructor/>
		public LuceneQuery(string indexName, string query):this()
		{
			Query = query;
			IndexName = indexName;
		}		

		/// <summary></summary>
		public string ToUri()
		{
			var uriBuilder = new StringBuilder();
			uriBuilder.Append("_fti/_design/" + DesignDocumentName + "/");
			uriBuilder.Append(IndexName);
			uriBuilder.Append("?");
			uriBuilder.Append("q=" + HttpUtility.UrlEncode(Query));
			if (IncludeDocs)
				uriBuilder.Append("&include_docs=true");
			if (Analyzer != null)
				uriBuilder.Append("&analyzer=" + HttpUtility.UrlEncode(Analyzer));
			return uriBuilder.ToString();
		}
	}
}
