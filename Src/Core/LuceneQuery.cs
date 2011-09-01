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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using CouchDude.Impl;

namespace CouchDude
{
	/// <summary>Types supported by Lucene engine.</summary>
	public enum LuceneType
	{
		/// <summary>Single persision fload-point number.</summary>
		Float,
		/// <summary>Double persision fload-point number.</summary>
		Double,
		/// <summary>Single persision integer number.</summary>
		Int,
		/// <summary>Double persision integer number.</summary>
		Long,
		/// <summary>Date/time number.</summary>
		Date
	}

	/// <summary>Lucene query result sort order.</summary>
	public struct LuceneSort
	{
		/// <summary>Name of feild to sort on</summary>
		public readonly string FieldName;

		/// <summary>Sort order</summary>
		public readonly bool SortDescending;

		/// <summary>Sort ordering type</summary>
		public readonly LuceneType? Type;

		/// <contructor/>
		public LuceneSort(string fieldName, bool sortDescending = false, LuceneType? type = null)
		{
			FieldName = fieldName;
			SortDescending = sortDescending;
			Type = type;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			var result = new StringBuilder();
			if (SortDescending)
				result.Append("\\");
			if (result.Length == 0 && Type == null)
				return FieldName;
			result.Append(FieldName);
			if (Type != null)
				result.Append("<").Append(Type.Value.ToString().ToLower()).Append(">");
			return result.ToString();
		}
	}

	/// <summary>Fulltext query to couchdb-lucene</summary>
	[TypeConverter(typeof(LuceneQueryUriConverter))]
	public class LuceneQuery: IQuery
	{
		/// <summary>Design document name (id without '_design/' prefix) to use view from.</summary>
		public string DesignDocumentName { get; set; }

		/// <summary>Index view name.</summary>
		public string IndexName { get; set; }

		/// <summary>Search query (see http://lucene.apache.org/java/2_4_0/queryparsersyntax.html) </summary>
		public string Query { get; set; }

		/// <summary>Name of analizer which is used for this query</summary>
		public string Analyzer { get; set; }

		/// <summary>Array of objects to sort on</summary>
		public LuceneSort[] Sort { get; set; }

		/// <summary>How many documents will be returned</summary>
		public int? Limit { get; set; }

		/// <summary>How many documents need to be skipped</summary>
		public int? Skip { get; set; }

		/// <summary>Query executed every type submitted.</summary>
		public bool SuppressCaching { get; set; }
		
		/// <summary>Sets default operator for boolean queries to AND insted of OR.</summary>
		public bool UseConjunctionSematics { get; set; }

		/// <summary>Indicates that we need documents from couchdb in result</summary>
		public bool IncludeDocs { get; set; }

		/// <summary>Stored fields to retrive. If <c>null</c> defaults to all.</summary>
		public ICollection<string> Fields { get; set; }
		
		/// <summary>If <c>true</c> couchdb-lucene will not block if the index is not up to date and it will immediately return results. 
		/// Therefore searches may be faster as Lucene caches important data (especially for sorting). 
		/// couchdb-lucene will trigger an index update unless one is already running.</summary>
		public bool DoNotBlockIfStale { get; set; }

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
			return LuceneQueryUriConverter.ToUri(this);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return LuceneQueryUriConverter.ToUriString(this);
		}

		/// <summary>Parses relative URI string as lucene-couchdb query.</summary>
		public static LuceneQuery Parse(string uriString)
		{
			return LuceneQueryUriConverter.Parse(uriString);
		}

		/// <summary>Parses relative URI as lucene-couchdb query.</summary>
		public static LuceneQuery Parse(Uri uri)
		{
			return LuceneQueryUriConverter.Parse(uri);
		}

		/// <summary>Clones query object.</summary>
		public LuceneQuery Clone()
		{
			//TODO: add manual cloning
			return Parse(ToString());
		}
	}
}
