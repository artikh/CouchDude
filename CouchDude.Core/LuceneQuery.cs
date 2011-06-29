﻿using System.Linq;
using System.Text;
using System.Web;

namespace CouchDude.Core
{
	/// <summary>Класс типизирован так как могут возвращаться в том числе объекты из бд</summary>
	public class LuceneQuery<T>:LuceneQuery
	{
		
	}

	/// <summary>Сортировка поля объекта</summary>
	public struct LuceneSort
	{
		/// <summary>Name of feild to sort on</summary>
		public string FieldName;

		/// <summary>Sort order</summary>
		public bool SortDescending;
	}

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

		/// <summary>Array of objects to sort on</summary>
		public LuceneSort[] Sort;

		/// <summary>How many documents will be returned</summary>
		public int Limit;

		/// <summary>How many documents need to be skipped</summary>
		public int Skip;

		/*
		 * UNIMPLEMENTED
		/// <summary></summary>
		public string Callback;

		/// <summary></summary>
		public bool Debug;
		/// <summary></summary>
		public string DefaultOperator;

		/// <summary></summary>
		public bool Stale;
		*/

		/// <constructor/>
		public LuceneQuery()
		{
			IncludeDocs = false;			
			Limit = 100;
			Skip = 0;
			Analyzer = null;
		}

		/// <constructor/>
		public LuceneQuery(string designDocumentName, string indexName, string query): this()
		{
			DesignDocumentName = designDocumentName;
			Query = query;
			IndexName = indexName;
		}		

		/// <summary>Трансформировать запрос в строку запроса</summary>
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
			if (Sort != null)
			{
				uriBuilder.Append("&sort=");
				var oneItemInList = true;
				foreach (var luceneSort in Sort)
				{
					var sortLuceneString = (!oneItemInList ? "," : "") + (luceneSort.SortDescending ? "\\" : "/" + luceneSort.FieldName);
					uriBuilder.Append(sortLuceneString);
					oneItemInList = false;
				}	
			}
			return uriBuilder.ToString();
		}
	}
}
