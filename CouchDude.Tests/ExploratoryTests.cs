using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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

		private static JsonSerializer GetSerializer()
		{
			var contractResolver = new CamelCasePropertyNamesContractResolver();
			contractResolver.DefaultMembersSearchFlags |= BindingFlags.NonPublic;

			var settings = new JsonSerializerSettings
			{
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = contractResolver,
				Converters = { new IsoDateTimeConverter() }
			};
			return Newtonsoft.Json.JsonSerializer.Create(settings);
		}

		private readonly JsonSerializer serializer = GetSerializer();

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

#pragma warning disable 649
		class ClassWithJObjectProperty
		{
			public string Id;
			public IList<JObject> SubObject;
		}
#pragma warning restore 649

		[Fact]
		public void ShouldDeserializeDocumentsWithJObjectProperties()
		{
			var json = new {
				id = "some ID",
				subObject = new[] { 
					new {
						prop1 = "prop 1 value",
						prop2 = "prop 2 value"
					},
					new {
						prop1 = "prop 1 second value",
						prop2 = "prop 2 second value"
					},
				}
			}.ToJObject();

			ClassWithJObjectProperty entity;
			using (var reader = new JTokenReader(json))
				entity = serializer.Deserialize<ClassWithJObjectProperty>(reader);

			Assert.Equal("some ID", entity.Id);
			Assert.Equal(2, entity.SubObject.Count);
			Utils.AssertSameJson(
				new {
					prop1 = "prop 1 value",
					prop2 = "prop 2 value"
				},
				entity.SubObject[0]
			);
		}

		public class PrivatePropertySetterClass
		{
			public string Name { get; private set; }
			public int Age { get; private set; }
		}

		[Fact]
		public void ShouldDeserializePrivatePropertySetter()
		{
			var json = new {
				name = "some Name",
				age = 42
			}.ToJObject();

			PrivatePropertySetterClass obj;
			using (var jTokenReader = new JTokenReader(json))
				obj = serializer.Deserialize<PrivatePropertySetterClass>(jTokenReader);

			Assert.NotNull(obj);
			Assert.Equal("some Name", obj.Name);
			Assert.Equal(42, obj.Age);
		}
	}
}