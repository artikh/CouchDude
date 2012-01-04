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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using CouchDude.Serialization;
using System.Json;
using Xunit;

namespace CouchDude.Tests
{
	internal static class TestUtils
	{
		private const int TaskTimeout = 5000;

		private static readonly NewtonsoftSerializer Serializer = new NewtonsoftSerializer();

		public static Task<T> ToTask<T>(this T dataItemToReturn)
		{
			return Task.Factory.StartNew(() => dataItemToReturn);
		}

		public static void WaitOrThrowOnTimeout(this Task task)
		{
			if (!task.Wait(TaskTimeout))
				throw new TimeoutException("Task wait timeout expired");
		}

		public static void WaitOrThrowOnTimeout(this WaitHandle task)
		{
			if (!task.WaitOne(TaskTimeout))
				throw new TimeoutException("Wait handle wait timeout expired");
		}

		public static string ToJsonString(this object self) { return self.ToJsonValue().ToString(JsonSaveOptions.None); }

		public static JsonValue ToJsonValue(this object self) { return Serializer.ConvertToJson(self, throwOnError: true); }

		public static JsonObject ToJsonObject(this object self) { return (JsonObject) self.ToJsonValue(); }

		public static Document ToDocument(this object self) { return new Document(self.ToJsonString()); }

		public static JsonValue ToJsonFragment(this object self) { return JsonValue.Parse(self.ToJsonString()); }

		public static TextReader ToJsonTextReader(this object self) { return self.ToJsonString().ToTextReader(); }

		public static TextReader ToTextReader(this string text) { return new StringReader(text); }

		public static string ReadAsUtf8String(this Stream stream) 
		{
			using (stream)
			using (var reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}

		public static string GetBodyString(this HttpResponseMessage response)
		{
			return response.Content.ReadAsStringAsync().Result;
		}

		public static void AssertSameJson(object jsonObject, string jsonString)
		{
			if (ReferenceEquals(jsonObject, null) && ReferenceEquals(jsonString, null))
				return;
			
			Assert.False(ReferenceEquals(jsonObject, null) || ReferenceEquals(jsonString, null));
			Assert.Equal(jsonObject.ToJsonString(), jsonString);
		}

		public static void AssertSameJson(object jsonObject, JsonValue jsonToken)
		{
			Assert.Equal(jsonObject.ToJsonString(), jsonToken.ToString());
		}

		public static void AssertSameJson(Document document, JsonValue jsonToken)
		{
			Assert.Equal(document.RawJsonObject.ToString(), jsonToken.ToString());
		}

		public static void AssertSameJson(JsonValue jsonValue, JsonValue jsonToken)
		{
			Assert.Equal(jsonValue.ToString(), jsonToken.ToString());
		}
	}
}
