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

using System;
using System.ComponentModel;

namespace CouchDude
{
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
	[TypeConverter(typeof(LuceneQueryUriConverter))]
	public class LuceneQuery
	{
		/// <summary>Design document name (id without '_design/' prefix) to use view from.</summary>
		public string DesignDocumentName { get; set; }

		/// <summary>Index view name.</summary>
		public string IndexName { get; set; }

		/// <summary>Search query (see http://lucene.apache.org/java/2_4_0/queryparsersyntax.html) </summary>
		public string Query { get; set; }

		/// <summary>Indicates that we need documents from couchdb in result</summary>
		public bool IncludeDocs { get; set; }

		/// <summary>Name of analizer which is used for this query</summary>
		public string Analyzer { get; set; }

		/// <summary>Array of objects to sort on</summary>
		public LuceneSort[] Sort { get; set; }

		/// <summary>How many documents will be returned</summary>
		public int? Limit { get; set; }

		/// <summary>How many documents need to be skipped</summary>
		public int? Skip { get; set; }

		/*
		 * UNIMPLEMENTED
		/// <summary></summary>
		public string Callback{ get; set; }

		/// <summary></summary>
		public bool Debug{ get; set; }
		/// <summary></summary>
		public string DefaultOperator{ get; set; }

		/// <summary></summary>
		public bool Stale{ get; set; }
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

		/// <summary>Expreses query as relative URL.</summary>
		public Uri ToUri()
		{
			return new Uri(LuceneQueryUriConverter.ToUriString(this), UriKind.Relative);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return LuceneQueryUriConverter.ToUriString(this);
		}
	}
}
