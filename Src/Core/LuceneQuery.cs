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
using CouchDude.Impl;
using JetBrains.Annotations;

namespace CouchDude
{
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
		public IList<LuceneSort> Sort { get; set; }

		/// <summary>How many documents will be returned</summary>
		public int? Limit { get; set; }

		/// <summary>How many documents need to be skipped</summary>
		public int? Skip { get; set; }

		/// <summary>Bypasses caching infrostructure of lucene-couchdb.</summary>
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
		
		/// <summary>Expreses query as relative URL.</summary>
		[Pure]
		public Uri ToUri()
		{
			return LuceneQueryUriConverter.ToUri(this);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return LuceneQueryUriConverter.ToUriString(this);
		}
		
		/// <summary>Cretates copy of current clone.</summary>
		public LuceneQuery Clone()
		{
			// TODO: implement manual clonnign here

			LuceneQuery clone;
			TryParse(ToString(), out clone);
			return clone;
		}

		/// <summary>Parse view query from provided URI.</summary>
		public static LuceneQuery Parse(Uri uri)
		{
			LuceneQuery viewQuery;
			if (!LuceneQueryUriConverter.TryParse(uri, out viewQuery))
				throw new ParseException("Error parsing couchdb-lucene index query URI: {0}", uri);
			return viewQuery;
		}

		/// <summary>Parse view query from provided URI.</summary>
		public static LuceneQuery Parse(string uriString)
		{
			LuceneQuery viewQuery;
			if (!LuceneQueryUriConverter.TryParse(uriString, out viewQuery))
				throw new ParseException("Error parsing couchdb-lucene index query URI string: {0}", uriString);
			return viewQuery;
		}

		/// <summary>Attemps to parse view query from provided URI.</summary>
		public static bool TryParse(Uri uri, out LuceneQuery viewQuery)
		{
			return LuceneQueryUriConverter.TryParse(uri, out viewQuery);
		}

		/// <summary>Attemps to parse view query from provided URI string.</summary>
		public static bool TryParse(string uriString, out LuceneQuery viewQuery)
		{
			return LuceneQueryUriConverter.TryParse(uriString, out viewQuery);
		}
	}
}
