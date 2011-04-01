using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Tests
{
	public class JTokenStringCompairer: IEqualityComparer<string>
	{
		public bool Equals(string x, string y)
		{
			var xToken = Parse(x);
			var yToken = Parse(y);
			return JToken.DeepEquals(xToken, yToken);
		}

		private static JToken Parse(string str)
		{
			using (var reader = new StringReader(str))
			using (var jsonReader = new JsonTextReader(reader))
				return JToken.ReadFrom(jsonReader);
		}

		public int GetHashCode(string obj)
		{
			return JObject.Parse(obj).GetHashCode();
		}
	}
}