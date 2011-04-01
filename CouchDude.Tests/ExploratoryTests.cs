using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests
{
	public class ExploratoryTests
	{
		private const string SomeNestedJson = @"{
				""str"": ""some string"",
				""number"": 42,
				""boolean"": true,
				""array"": [
					""some strange string"",
					42,
					true
				],
				""object"": {
					""str"": ""some string"",
					""number"": 42,
					""boolean"": true
				}
			}";

		private const string SomeAltNestedJson = @"{
				""str"": ""some string"",
				""number"": 42,
				""boolean"": true,
				""array"": [
					""some strange string"",
					42,
					true
				],
				""object"": {
					""str"": ""some string"",
					""number"": 42,
					""boolean"": false
				}
			}";

		[Fact]
		public void SouldCompareTwoJObjectsCorrectly()
		{
			var obj1 = JObject.Parse(SomeNestedJson);
			var obj2 = JObject.Parse(SomeNestedJson + "   ");

			Assert.True(new JTokenEqualityComparer().Equals(obj1, obj2));
		}

		[Fact]
		public void ShouldDetectDeepNestedDifferences()
		{
			var obj1 = JObject.Parse(SomeNestedJson);
			var obj2 = JObject.Parse(SomeAltNestedJson);

			Assert.False(new JTokenEqualityComparer().Equals(obj1, obj2));
		}
	}
}