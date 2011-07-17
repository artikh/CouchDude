#region Licence Info 
/*
  Copyright 2011 · Artem Tikhomirov																					
 																																					
  Licensed under the Apache License, Version 2.0 (the "License");					
  you may not use this file except in compliance with the License.					
  You may obtain a copy of the License at																	
 																																					
      http://www.apache.org/licenses/LICENSE-2.0														
 																																					
  Unless required by applicable law or agreed to in writing, software			
  distributed under the License is distributed on an "AS IS" BASIS,				
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
  See the License for the specific language governing permissions and			
  limitations under the License.																						
*/
#endregion

using System.Text;
using System.Web;

namespace CouchDude.Core
{
	/// <summary>Класс типизирован так как могут возвращаться в том числе объекты из бд</summary>
	public class LuceneQuery<T>:LuceneQuery { }

	/// <summary>Сортировка поля объекта</summary>
	public struct LuceneSort
	{
		/// <summary>Name of feild to sort on</summary>
		public string FieldName;

		/// <summary>Sort order</summary>
		public bool SortDescending;

		/// <contructor/>
		public LuceneSort(string fieldName, bool sortDescending = false)
		{
			FieldName = fieldName;
			SortDescending = sortDescending;
		}
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
			DesignDocumentName = "lucene";
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
					var sortLuceneString = (!oneItemInList ? "," : "") + (luceneSort.SortDescending ? "\\" : "/") + luceneSort.FieldName;
					uriBuilder.Append(sortLuceneString);
					oneItemInList = false;
				}	
			}
			return uriBuilder.ToString();
		}
	}
}
