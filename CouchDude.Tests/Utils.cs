using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace CouchDude.Tests
{
	internal static class Utils
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
