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

namespace CouchDude.Tests.Unit.Core.Api
{
	public class JsonFragmentTests
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
			var excetpion = Assert.Throws<ArgumentNullException>(() => new JsonFragment(json));
			Assert.Contains("json", excetpion.Message);
		}

		[Theory]
		[InlineData("{")]
		[InlineData("{ \"name\": \"John\" ")]
		[InlineData("[}")]
		public void ShouldThrowParseExceptionOnIncorrectJsonString(string json)
		{
			Assert.Throws<ParseException>(() => new JsonFragment(json));
		}

		[Fact]
		public void ShouldCastToDynamicAsRawJson()
		{
			var jsonFragment = new JsonFragment("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}");
			dynamic dynamicFragment = jsonFragment;

			Assert.Null((string)dynamicFragment.Id);
			Assert.Null((string)dynamicFragment.Revision);
			Assert.Null((string)dynamicFragment.Type);

			Assert.Equal("8A7FD19B",     (string)dynamicFragment._id);
			Assert.Equal("1-42",         (string)dynamicFragment._rev);
			Assert.Equal("simpleEntity", (string)dynamicFragment.type);
			Assert.Equal("John",         (string)dynamicFragment.name);
		}

		[Fact]
		public void ShouldExposeArraysAsDynamic()
		{
			dynamic doc = new JsonFragment("{\"array\": [\"str1\", 42]}");

			Assert.Equal("str1", (string)doc.array[0]);
			Assert.Equal(42, (int)doc.array[1]);
		}

		[Fact]
		public void ShouldLoadJsonFromTextReader()
		{
			using(var textReader = new StringReader("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}"))
			{
				dynamic fragment = new JsonFragment(textReader);

				Assert.Equal("8A7FD19B", (string)fragment._id);
				Assert.Equal("1-42", (string)fragment._rev);
				Assert.Equal("simpleEntity", (string)fragment.type);
				Assert.Equal("John", (string)fragment.name);
			}
		}

		[Fact]
		public void ShouldParseJsonString()
		{
			dynamic fragment = new JsonFragment("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}");

			Assert.Equal("8A7FD19B", (string)fragment._id);
			Assert.Equal("1-42", (string)fragment._rev);
			Assert.Equal("simpleEntity", (string)fragment.type);
			Assert.Equal("John", (string)fragment.name);
		}

		[Fact]
		public void ShouldProduceTextReaderForContent()
		{
			const string jsonString = "{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}";
			var fragment = new JsonFragment(jsonString);
			
			using (var textReader = fragment.Read())
			{
				var producedJsonString = textReader.ReadToEnd();
				Assert.Equal(jsonString, producedJsonString);
			}
		}


		[Fact]
		public void ShouldThrowOnNullArgumentToDeserialize()
		{
			Assert.Throws<ArgumentNullException>(() => new JsonFragment().Deserialize(null));
		}

		[Fact]
		public void ShouldDeserializeStringProperty()
		{
			var fragment = new { name = "John" }.ToJsonFragment();
			var entity = (User)fragment.Deserialize(typeof(User));
			Assert.Equal("John", entity.Name);
		}

		[Fact]
		public void ShouldDeserializeIntProperty()
		{
			var fragment = new { age = 18 }.ToJsonFragment();
			var entity = (User)fragment.Deserialize(typeof(User));
			Assert.Equal(18, entity.Age);
		}

		[Fact]
		public void ShouldDeserializeDateTimeProperty()
		{
			var fragment = new { timestamp = "2011-06-01T12:04:34.444Z" }.ToJsonFragment();
			var entity = (User)fragment.Deserialize(typeof(User));
			Assert.Equal(new DateTime(2011, 06, 01, 12, 04, 34, 444, DateTimeKind.Utc), entity.Timestamp);
		}

		[Fact]
		public void ShouldDeserializeFields()
		{
			var fragment = new { field = "quantum mechanics" }.ToJsonFragment();
			var entity = (User)fragment.Deserialize(typeof(User));
			Assert.Equal("quantum mechanics", entity.Field);
		}

		[Fact]
		public void ShouldDeserializeEnumsAsStrings()
		{
			var fragment = new { sex = "female" }.ToJsonFragment();
			var entity = (User)fragment.Deserialize(typeof(User));
			Assert.Equal(UserSex.Female, entity.Sex);
		}

		[Fact]
		public void ShouldThrowParseExceptionOnDeserializationError()
		{
			var obj = new JsonFragment(@"{ ""age"": ""not an integer"" }");
			Assert.Throws<ParseException>(() => obj.Deserialize(typeof(User)));
		}
		
		[Fact]
		public void ShouldThrowOnNullArgumentToSerialize()
		{
			Assert.Throws<ArgumentNullException>(() => JsonFragment.Serialize(null));
		}

		[Fact]
		public void ShouldSerializeEnumsAsString()
		{
			var entity = new User(sex: UserSex.Female );
			dynamic fragment = JsonFragment.Serialize(entity);
			Assert.Equal("Female", (string)fragment.sex);
		}

		[Fact]
		public void ShouldSerializeDatesAccodingToIso8601()
		{
			var entity = new User(timestamp: new DateTime(2011, 06, 01, 12, 04, 34, 444, DateTimeKind.Utc));
			dynamic fragment = JsonFragment.Serialize(entity);
			Assert.Equal("2011-06-01T12:04:34.444Z", (string)fragment.timestamp);
		}

		[Fact]
		public void ShouldConvertPropertyNameToCamelCase()
		{
			var entity = new User(name: "john");
			dynamic fragment = JsonFragment.Serialize(entity);
			Assert.NotNull((string)fragment.name);
		}

		[Fact]
		public void ShouldSerializePublicFields()
		{
			var entity = new User { Field = "quantum mechanics" };
			dynamic fragment = JsonFragment.Serialize(entity);
			Assert.Equal("quantum mechanics", (string)fragment.field);
		}

		[Fact]
		public void ShouldParseDocumentInfo()
		{
			var obj = new JsonFragment(@"{ ""some_prop"": ""some value"" }");
			Assert.Equal(@"{""some_prop"":""some value""}", obj.ToString());
		}
	}
}