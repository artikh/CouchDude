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

using System.IO;
using System.Json;
using System.Linq;
using CouchDude.Impl;
using CouchDude.Utils;

namespace CouchDude.Api
{
	/// <summary>Loads view request result from provided <see cref="TextReader"/>.</summary>
	internal class ViewQueryResultParser : QueryResultParserBase
	{
		/// <summary>Loads view request result from provided <see cref="TextReader"/>.</summary>
		public static IViewQueryResult Parse(TextReader textReader, ViewQuery viewQuery)
		{
			var response = ParseRawResponse(textReader);
			var totalRows = GetTotalRows(response);
			var offset = GetOffset(response);
			var rawRows = GetRawRows(response);

			var rows = (
				from rawRow in rawRows
				let viewKey = rawRow.TryGetValue("key")
				let documentId = rawRow.GetPrimitiveProperty<string>("id")
				let value = rawRow.TryGetValue("value")
				let documentJsonObject = rawRow.TryGetValue("doc") as JsonObject
				let document = documentJsonObject == null ? null : new Document(documentJsonObject)
				select new ViewResultRow(viewKey, value, documentId, document)
			).ToArray();

			return new ViewQueryResult(viewQuery, rows, totalRows, offset);
		}
	}
}
