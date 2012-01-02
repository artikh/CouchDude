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

using CouchDude.Api;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Api
{
	public class JsonObjectTests
	{
		public enum UserSex
		{
			Male,
			Female
		}
		
		// ReSharper disable UnusedAutoPropertyAccessor.Local
		public class User
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="T:System.Object"/> class.
			/// </summary>
			public User(DateTime? timestamp = null, string name = null, int? age = null, UserSex? sex = null)
			{
				if(timestamp.HasValue)
					Timestamp = timestamp.Value;
				Name = name;
				if(age.HasValue)
					Age = age.Value;
				if (sex.HasValue)
					Sex = sex.Value;
			}

			public DateTime Timestamp { get; private set; }
			public string Name { get; private set; }
			public int Age { get; private set; }
			public string Field;
			public UserSex Sex { get; private set; }
		}
		// ReSharper restore UnusedAutoPropertyAccessor.Local


		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		[InlineData("\t")]
		public void ShouldThrowOnNullEmptyOrWhiteSpaceJsonString(string json)
		{
			var excetpion = Assert.Throws<ArgumentNullException>(() => new JsonObject(json));
			Assert.Contains("json", excetpion.Message);
		}

		[Theory]
		[InlineData("{")]
		[InlineData("{ \"name\": \"John\" ")]
		[InlineData("[}")]
		public void ShouldThrowParseExceptionOnIncorrectJsonString(string json)
		{
			Assert.Throws<ParseException>(() => new JsonObject(json));
		}

		[Fact]
		public void ShouldCastToDynamicAsRawJson()
		{
			var jsonObject = new JsonObject("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}");
			dynamic dynamicObject = jsonObject;

			Assert.Null((string)dynamicObject.Id);
			Assert.Null((string)dynamicObject.Revision);
			Assert.Null((string)dynamicObject.Type);

			Assert.Equal("8A7FD19B",     (string)dynamicObject._id);
			Assert.Equal("1-42",         (string)dynamicObject._rev);
			Assert.Equal("simpleEntity", (string)dynamicObject.type);
			Assert.Equal("John",         (string)dynamicObject.name);
		}

		[Fact]
		public void ShouldExposeArraysAsDynamic()
		{
			dynamic doc = new JsonObject("{\"array\": [\"str1\", 42]}");

			Assert.Equal("str1", (string)doc.array[0]);
			Assert.Equal(42, (int)doc.array[1]);
		}

		[Fact]
		public void ShouldLoadJsonFromTextReader()
		{
			using(var textReader = new StringReader("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}"))
			{
				dynamic obj = new JsonObject(textReader);

				Assert.Equal("8A7FD19B", (string)obj._id);
				Assert.Equal("1-42", (string)obj._rev);
				Assert.Equal("simpleEntity", (string)obj.type);
				Assert.Equal("John", (string)obj.name);
			}
		}

		[Fact]
		public void ShouldParseJsonString()
		{
			dynamic obj = new JsonObject("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}");

			Assert.Equal("8A7FD19B", (string)obj._id);
			Assert.Equal("1-42", (string)obj._rev);
			Assert.Equal("simpleEntity", (string)obj.type);
			Assert.Equal("John", (string)obj.name);
		}

		[Fact]
		public void ShouldProduceTextReaderForContent()
		{
			const string jsonString = "{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}";
			var obj = new JsonObject(jsonString);
			
			using (var textReader = obj.Read())
			{
				var producedJsonString = textReader.ReadToEnd();
				Assert.Equal(jsonString, producedJsonString);
			}
		}


		[Fact]
		public void ShouldThrowOnNullArgumentToDeserialize()
		{
			Assert.Throws<ArgumentNullException>(() => new JsonObject().Deserialize(null));
		}

		[Fact]
		public void ShouldDeserializeStringProperty()
		{
			var jsonObject = new { name = "John" }.ToJsonObject();
			var entity = (User)jsonObject.Deserialize(typeof(User));
			Assert.Equal("John", entity.Name);
		}

		[Fact]
		public void ShouldDeserializeIntProperty()
		{
			var jsonObject = new { age = 18 }.ToJsonObject();
			var entity = (User)jsonObject.Deserialize(typeof(User));
			Assert.Equal(18, entity.Age);
		}

		[Fact]
		public void ShouldDeserializeDateTimeProperty()
		{
			var jsonObject = new { timestamp = "2011-06-01T12:04:34.444Z" }.ToJsonObject();
			var entity = (User)jsonObject.Deserialize(typeof(User));
			Assert.Equal(new DateTime(2011, 06, 01, 12, 04, 34, 444, DateTimeKind.Utc), entity.Timestamp);
		}

		[Fact]
		public void ShouldDeserializeFields()
		{
			var jsonObject = new { field = "quantum mechanics" }.ToJsonObject();
			var entity = (User)jsonObject.Deserialize(typeof(User));
			Assert.Equal("quantum mechanics", entity.Field);
		}

		[Fact]
		public void ShouldDeserializeEnumsAsStrings()
		{
			var jsonObject = new { sex = "female" }.ToJsonObject();
			var entity = (User)jsonObject.Deserialize(typeof(User));
			Assert.Equal(UserSex.Female, entity.Sex);
		}

		[Fact]
		public void ShouldThrowParseExceptionOnDeserializationError()
		{
			var obj = new JsonObject(@"{ ""age"": ""not an integer"" }");
			Assert.Throws<ParseException>(() => obj.Deserialize(typeof(User)));
		}

		[Fact]
		public void ShouldParseDocumentInfo()
		{
			var obj = new JsonObject(@"{ ""some_prop"": ""some value"" }");
			Assert.Equal(@"{""some_prop"":""some value""}", obj.ToString());
		}
	}
}