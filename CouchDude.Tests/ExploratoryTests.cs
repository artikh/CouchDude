using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

using JsonSerializer = CouchDude.Core.Utils.JsonSerializer;


// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
#pragma warning disable 649

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

		class ClassWithJObjectProperty
		{
			public string Id;
			public IList<JObject> SubObject;
		}

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
					}
				}
			}.ToJObject();

			ClassWithJObjectProperty entity;
			using (var reader = new JTokenReader(json))
				entity = JsonSerializer.Instance.Deserialize<ClassWithJObjectProperty>(reader);

			Assert.Equal("some ID", entity.Id);
			Assert.Equal(2, entity.SubObject.Count);
			TestUtils.AssertSameJson(
				new {
					prop1 = "prop 1 value",
					prop2 = "prop 2 value"
				},
				entity.SubObject[0]
			);
		}

		public class PrivatePropertySetterClass
		{
			private string name = "name name";

			public PrivatePropertySetterClass(string name, int age)
			{
				this.name = name;
				Age = age;
			}

			public string Name { get { return name; } private set { name = value; } }

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
				obj = JsonSerializer.Instance.Deserialize<PrivatePropertySetterClass>(jTokenReader);

			Assert.NotNull(obj);
			Assert.Equal("some Name", obj.Name);
			Assert.Equal(42, obj.Age);
		}

		[Fact]
		public void ShouldNotSerializePrivateFields()
		{
			var instance = new PrivatePropertySetterClass("John Don", 42);

			var writer = new JTokenWriter();
			JsonSerializer.Instance.Serialize(writer, instance);
			var json = (JObject)writer.Token;

			Assert.Null(json.Property("_name"));
		}

		[Fact]
		public void ShouldSuppressUnescaping()
		{
			var uri = new Uri("http://www.example.com/value%2fvalue");
			uri.LeaveDotsAndSlashesEscaped();

			Assert.Equal("http://www.example.com/value%2fvalue", uri.ToString());
		}
	}

	public static class UriFix
	{
		private const int UnEscapeDotsAndSlashes = 0x2000000;
		private const int SimpleUserSyntax = 0x20000;

		public static void LeaveDotsAndSlashesEscaped(this Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			FieldInfo fieldInfo = uri.GetType().GetField("m_Syntax", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
				throw new MissingFieldException("'m_Syntax' field not found");

			object uriParser = fieldInfo.GetValue(uri);
			fieldInfo = typeof (UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
				throw new MissingFieldException("'m_Flags' field not found");

			object uriSyntaxFlags = fieldInfo.GetValue(uriParser);

			// Clear the flag that we don't want
			uriSyntaxFlags = (int) uriSyntaxFlags & ~UnEscapeDotsAndSlashes;
			uriSyntaxFlags = (int) uriSyntaxFlags & ~SimpleUserSyntax;
			fieldInfo.SetValue(uriParser, uriSyntaxFlags);
		}
	}
}