#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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
using System.IO;
using System.Json;
using System.Linq;
using CouchDude.Impl;
using CouchDude.Utils;

namespace CouchDude.Api
{
	/// <summary>Loads couchdb-lucene request result from provided <see cref="TextReader"/>.</summary>
	internal class LuceneQueryResultParser : QueryResultParserBase
	{
		/// <summary>Loads view request result from provided <see cref="TextReader"/>.</summary>
		public static ILuceneQueryResult Parse(TextReader textReader, LuceneQuery query)
		{
			var response = ParseRawResponse(textReader);
			var totalRows = GetTotalRows(response);
			var offset = GetOffset(response);
			var rawRows = GetRawRows(response);
			var fetchDuration = TimeSpan.FromMilliseconds(response.GetPrimitiveProperty<int>("fetch_duration"));
			var searchDuration = TimeSpan.FromMilliseconds(response.GetPrimitiveProperty<int>("search_duration"));
			var limit = response.GetPrimitiveProperty<int>("limit");
			var skip = response.GetPrimitiveProperty<int>("skip");

			var rows = (
				from rawRow in rawRows
				let id = rawRow.GetPrimitiveProperty<string>("id")
				let fields = rawRow.TryGetValue("fields")
				let score = rawRow.GetPrimitiveProperty<decimal>("score")
				let documentJsonObject = rawRow.TryGetValue("doc") as JsonObject
				let document = documentJsonObject == null ? null : new Document(documentJsonObject)
				select new LuceneResultRow(id, fields, score, id, document)
			).ToArray();

			return new LuceneQueryResult(query, rows, rows.Length, totalRows, offset, fetchDuration, searchDuration, limit, skip);
		}
	}
}