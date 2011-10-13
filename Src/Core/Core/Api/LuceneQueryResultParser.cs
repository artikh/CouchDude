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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CouchDude.Impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Api
{
	/// <summary>Loads couchdb-lucene request result from provided <see cref="TextReader"/>.</summary>
	public class LuceneQueryResultParser
	{
		private static readonly JsonSerializer Serializer = JsonSerializer.Create(JsonFragment.CreateSerializerSettings());
		
#pragma warning disable 0649
		// ReSharper disable UnassignedField.Local
		// ReSharper disable InconsistentNaming
		// ReSharper disable ClassNeverInstantiated.Local
		private class RawViewResultRow
		{
			public JObject doc;
			public string id;
			public decimal score;
			public JToken fields;
		}

		private class RawViewResult
		{
			public int total_rows;
			public int offset;
			public int skip;
			public int limit;
			public int search_duration;
			public int fetch_duration;

			public IList<RawViewResultRow> rows;
		}
		// ReSharper restore ClassNeverInstantiated.Local
		// ReSharper restore InconsistentNaming
		// ReSharper restore UnassignedField.Local
#pragma warning restore 0649

		/// <summary>Loads view request result from provided <see cref="TextReader"/>.</summary>
		public static ILuceneQueryResult Parse(TextReader textReader, LuceneQuery query)
		{
			RawViewResult rawResult;
			try
			{
				using (var reader = new JsonTextReader(textReader) {CloseInput = false})
					rawResult = Serializer.Deserialize<RawViewResult>(reader);
			}
			catch (Exception e)
			{
				if (e is JsonReaderException || e is JsonSerializationException)
					throw new ParseException(e, e.Message);
				throw;
			}

			if (rawResult == null)
				return LuceneQueryResult.Empty;

			var rows = (
				from rawRow in rawResult.rows ?? new RawViewResultRow[0]
				let fields = rawRow.fields != null ? new JsonFragment(rawRow.fields) : null
				let document = rawRow.doc != null ? new Document(rawRow.doc) : null
				select new LuceneResultRow(rawRow.id, fields, rawRow.score, rawRow.id, document)
			).ToList();

			return new LuceneQueryResult(
				query, 
				rows, 
				count: rows.Count,
				totalCount: rawResult.total_rows, 
				offset: rawResult.offset, 
				fetchDuration: TimeSpan.FromMilliseconds(rawResult.fetch_duration), 
				searchDuration: TimeSpan.FromMilliseconds(rawResult.search_duration), 
				limit: rawResult.limit, 
				skip: rawResult.skip
			);
		}
	}
}