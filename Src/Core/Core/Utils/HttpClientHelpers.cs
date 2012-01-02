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
		private readonly static Encoding DefaultHttpEncoding = Encoding.GetEncoding(0x6faf);

		public static async Task<Document> ReadAsDocumentAsync(this HttpContent self)
		{
			using (var reader = await self.ReadAsTextReaderAsync().ConfigureAwait(false))
				return new Document(reader);
		}

		public static async Task<JsonArray> ReadAsJsonArrayAsync(this HttpContent self)
		{
			var jsonValue = await self.ReadAsJsonValueAsync().ConfigureAwait(false);
			return jsonValue as JsonArray;
		}

		public static async Task<JsonObject> ReadAsJsonObjectAsync(this HttpContent self)
		{
			var jsonValue = await self.ReadAsJsonValueAsync().ConfigureAwait(false);
			return jsonValue as JsonObject;
		}

		public static async Task<JsonValue> ReadAsJsonValueAsync(this HttpContent self)
		{
			using (var stream = await self.ReadAsStreamAsync().ConfigureAwait(false))
				try
				{
					return JsonValue.Load(stream);
				}
				catch (Exception e)
				{
					throw new ParseException(e, "Error parsing JSON");
				}
		}

		/// <summary>Constructs text reader over HTTP content using response's encoding info.</summary>
		public static async Task<TextReader> ReadAsTextReaderAsync(this HttpContent self)
		{
			if (self == null)
				return null;

			var encoding = GetContentEncoding(self);
			return new StreamReader(await self.ReadAsStreamAsync().ConfigureAwait(false), encoding);
		}

		private static Encoding GetContentEncoding(HttpContent httpContent)
		{
			var encoding = DefaultHttpEncoding;
			var contentType = httpContent.Headers.ContentType;
			if (contentType != null && contentType.CharSet != null)
			{
				try
				{
					encoding = Encoding.GetEncoding(contentType.CharSet);
				}
				catch (ArgumentException exception)
				{
					throw new InvalidOperationException(
						"The character set provided in ContentType is invalid. Cannot read content as string using an invalid character set.",
						exception);
				}
			}
			return encoding;
		}
	}
}
