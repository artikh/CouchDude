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
	/// <summary>Loads view request result from provided <see cref="TextReader"/>.</summary>
	public class ViewQueryResultParser
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
			public JToken key;
			public JToken value;
		}

		private class RawViewResult
		{
			public int total_rows;
			public int offset;
			public IList<RawViewResultRow> rows;
		}
		// ReSharper restore ClassNeverInstantiated.Local
		// ReSharper restore InconsistentNaming
		// ReSharper restore UnassignedField.Local
		#pragma warning restore 0649
		
		/// <summary>Loads view request result from provided <see cref="TextReader"/>.</summary>
		public static IViewQueryResult Parse(TextReader textReader, ViewQuery viewQuery)
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
				return ViewQueryResult.Empty;

			var rows = (
				from rawRow in rawResult.rows ?? new RawViewResultRow[0]
				let viewKey = rawRow.key != null ? new JsonFragment(rawRow.key) : null
				let documentId = rawRow.id
				let value = rawRow.value != null ? new JsonFragment(rawRow.value) : null
				let document = rawRow.doc != null ? new Document(rawRow.doc) : null
				select new ViewResultRow(viewKey, value, documentId, document)
			).ToList();

			return new ViewQueryResult(viewQuery, rows, rawResult.total_rows, rawResult.offset);
		}
	}
}
