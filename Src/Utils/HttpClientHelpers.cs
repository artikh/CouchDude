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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CouchDude.Utils
{
	/// <summary>Extension methods for <see cref="System.Net.Http"/> API.</summary>
	internal static class HttpClientHelpers
	{
		private readonly static Encoding Utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

		public static async Task<JsonArray> ReadAsJsonArrayAsync(this HttpContent self)
		{
			var jsonValue = await self.ReadAsJsonValueAsync();
			return jsonValue as JsonArray;
		}

		public static async Task<JsonObject> ReadAsJsonObjectAsync(this HttpContent self)
		{
			var jsonValue = await self.ReadAsJsonValueAsync();
			return jsonValue as JsonObject;
		}

		public static async Task<JsonValue> ReadAsJsonValueAsync(this HttpContent self)
		{
			using (var reader = await self.ReadAsUtf8TextReaderAsync())
				try
				{
					return JsonValue.Load(reader);
				}
				catch (Exception e)
				{
					throw new ParseException(e, "Error parsing JSON");
				}
		}

		/// <summary>Constructs text reader over HTTP content using response's encoding info.</summary>
		public static async Task<TextReader> ReadAsUtf8TextReaderAsync(this HttpContent self)
		{
			if (self == null)
				return null;

			var stream = await self.ReadAsStreamAsync();
			return new StreamReader(stream, Utf8Encoding);
		}
	}
}
