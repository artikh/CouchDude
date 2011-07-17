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

using System.IO;
using CouchDude.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace CouchDude.Tests
{
	internal static class TestUtils
	{
		private static JsonSerializerSettings GetJsonSerializerSettings()
		{
			return new JsonSerializerSettings
			{
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
		}

		public static string ToJson(this object self)
		{
			return JsonConvert.SerializeObject(self, Formatting.None, GetJsonSerializerSettings());
		}

		public static JToken ToJToken(this object self)
		{
			return JToken.FromObject(self, JsonSerializer.Create(GetJsonSerializerSettings()));
		}

		public static JObject ToJObject(this object self)
		{
			return JObject.FromObject(self, JsonSerializer.Create(GetJsonSerializerSettings()));
		}

		public static Document ToDocument(this object self)
		{
			return new Document(self.ToJson());
		}

		public static JsonFragment ToJsonFragment(this object self)
		{
			return new JsonFragment(self.ToJson());
		}

		public static TextReader ToJsonTextReader(this object self)
		{
			return self.ToJson().ToTextReader();
		}

		public static TextReader ToTextReader(this string text)
		{
			return new StringReader(text);
		}

		public static void AssertSameJson(object jsonObject, string jsonString)
		{
			if (ReferenceEquals(jsonObject, null) && ReferenceEquals(jsonString, null))
				return;
			else
				Assert.False(ReferenceEquals(jsonObject, null) || ReferenceEquals(jsonString, null));

			Assert.Equal(jsonObject.ToJson(), jsonString, new JTokenStringCompairer());
		}

		public static void AssertSameJson(
			object jsonObject, JToken jsonToken)
		{
			Assert.Equal(jsonObject.ToJToken(), jsonToken, new JTokenEqualityComparer());
		}
	}
}
