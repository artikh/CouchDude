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
using System.Json;
using CouchDude.Configuration;
using CouchDude.Serialization;
using CouchDude.Tests.SampleData;
using JetBrains.Annotations;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Serialization
{
	public class NewtonsoftSerializerConvertFromJsonTests
	{
		public class Composite
		{
			public string Type { get; set; }
			public string Id { get; set; }
		}

		public class TestEntity
		{
			public string Name { get; [UsedImplicitly] private set; }
			public DateTime Birthday { get; set; }
			public int Age { get; set; }
			public string Type { get; set; }
			public Composite CompositeProperty { get; set; }
			public string Field;
			public ChildTestEntity Child;
		}

		public class ChildTestEntity : TestEntity { }

		private IEntityConfig config = MockEntityConfig();
		private readonly ISerializer serializer = new NewtonsoftSerializer();

		private static IEntityConfig MockEntityConfig(Action<Mock<IEntityConfig>> additionalActions = null)
		{
			var configMock = new Mock<IEntityConfig>();
			configMock.Setup(ec => ec.SetId(It.IsAny<object>(), It.IsAny<string>()));
			configMock.Setup(ec => ec.SetRevision(It.IsAny<object>(), It.IsAny<string>()));
			configMock
				.Setup(ec => ec.ConvertDocumentIdToEntityId(It.IsAny<string>())).Returns<string>(docId => "E" + docId);
			configMock.Setup(ec => ec.EntityType).Returns(typeof (TestEntity));
			configMock.Setup(ec => ec.DocumentType).Returns("entity");
			if (additionalActions != null)
				additionalActions(configMock);
			return configMock.Object;
		}

		private static JsonObject CreateDoc(object documentObject = null)
		{
			dynamic jsonObject = documentObject != null ? documentObject.ToJObject() : new JsonObject();
			jsonObject._id = "doc1";
			jsonObject._rev = "1-42";
			jsonObject.type = "entity";
			jsonObject.child = new JsonObject();
			jsonObject.child.type = "childType";
			return (JsonObject)jsonObject;
		}

		[Fact]
		public void ShouldThrowOnIncompatibleDocumentType()
		{
			Assert.Throws<InvalidOperationException>(
				() => serializer.ConvertFromJson(
					new EntityConfig(typeof (EntityWithoutRevision)),
					Entity.CreateDocWithRevision().RawJsonObject, 
					throwOnError: true
			));
		}

		[Fact]
		public void ShouldThrowOnNullArguments()
		{
			Assert.Throws<ArgumentNullException>(() => serializer.ConvertFromJson<TestEntity>(CreateDoc(), true));
		}
		
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\t")]
		public void ShouldThrowOnEmptyNullOrWightspaceId(string id)
		{
			var doc = JsonValue.Parse(id == null ? new { type = "entity" }.ToJsonString() : new { _id = id, type = "entity" }.ToJsonString());

			Assert.Throws<DocumentIdMissingException>(() => serializer.ConvertFromJson<TestEntity>(doc, true));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\t")]
		public void ShouldThrowIfConvertDocumentIdToEntityIdReturnsNullEmptyOrWightspaceString(string invalidEntityId)
		{
			config = MockEntityConfig(
				mock => mock.Setup(ec => ec.ConvertDocumentIdToEntityId(It.IsAny<string>())).Returns(invalidEntityId));
			Assert.Throws<InvalidOperationException>(() => serializer.ConvertFromJson(config, CreateDoc(), true));
		}

		[Fact]
		public void ShouldThrowDocumentParseExceptionOnDocumentWithoutId()
		{
			Assert.Throws<DocumentIdMissingException>(
				() =>
					serializer.ConvertFromJson(
						Default.Settings.GetConfig(typeof (Entity)),
						new {
							_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
							type = "entity",
							name = "John Smith"
						}.ToJObject(),
						true)
				);
		}


		[Fact]
		public void ShouldThrowDocumentParseExceptionOnDocumentWithoutRevision()
		{
			Assert.Throws<DocumentRevisionMissingException>(
				() => 
					serializer.ConvertFromJson(
						Default.Settings.GetConfig(typeof (Entity)),
						new { _id = "entity.doc1", type = "entity", name = "John Smith" }.ToJObject(),
						true)
			);
		}
		
		[Fact]
		public void ShouldThrowParseExceptionOnDeserializationError()
		{
			var obj = (JsonObject)JsonValue.Parse(
				@"{ ""_id"": ""entity.doc1"", ""_rev"": ""123"", ""type"": ""entity"", ""age"": ""not an integer"" }");
			Assert.Throws<ParseException>(() => 
				serializer.ConvertFromJson(Default.Settings.GetConfig(typeof (Entity)), obj, true)
			);
		}
		
		[Fact]
		public void ShouldNotSetTypeProperty()
		{
			var document = CreateDoc();
			var entity = (TestEntity) serializer.ConvertFromJson(config, document, true);
			Assert.Null(entity.Type);
		}

		[Fact]
		public void ShouldSetTypePropertyOnSubobjects()
		{
			var document = CreateDoc();
			var entity = (TestEntity)serializer.ConvertFromJson(config, document, true);
			Assert.NotNull(entity.Child.Type);
		}
	}
}
