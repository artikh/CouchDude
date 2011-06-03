using System;
using CouchDude.Core;
using CouchDude.Core.Configuration;
using CouchDude.Core.Impl;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Impl
{
	public class EntitySerializerDeserializeTests
	{
		public class Composite
		{
			public string Type { get; set; }
			public string Id { get; set; }
		}

		public class Entity
		{
			public string Name { get; private set; }
			public DateTime Birthday { get; set; }
			public int Age { get; set; }
			public string Type { get; set; }
			public Composite CompositeProperty { get; set; }
			public string Field;
		}

		private class ChildEntity: Entity { }

		private IEntityConfig config;
		private string setId;
		private string setRevision;

		public EntitySerializerDeserializeTests()
		{
			config = MockEntityConfig();
		}

		private IEntityConfig MockEntityConfig(Action<Mock<IEntityConfig>> additionalActions = null)
		{
			var configMock = new Mock<IEntityConfig>();
			configMock
				.Setup(ec => ec.SetId(It.IsAny<object>(), It.IsAny<string>()))
				.Callback<object, string>((e, id) => { setId = id; });
			configMock
				.Setup(ec => ec.SetRevision(It.IsAny<object>(), It.IsAny<string>()))
				.Callback<object, string>((e, rev) => { setRevision = rev; });
			configMock
				.Setup(ec => ec.ConvertDocumentIdToEntityId(It.IsAny<string>()))
				.Returns<string>(docId => "E" + docId);
			configMock.Setup(ec => ec.EntityType).Returns(typeof (Entity));
			configMock.Setup(ec => ec.DocumentType).Returns("entity");
			if (additionalActions != null)
				additionalActions(configMock);
			return configMock.Object;
		}

		private static JObject CreateDoc(object documentObject = null)
		{
			dynamic document = documentObject != null ? documentObject.ToJObject() : new JObject();
			document._id = "doc1";
			document.type = "entity";
			return document;
		}

		[Fact]
		public void ShouldThrowOnNullArguments()
		{
			Assert.Throws<ArgumentNullException>(() => EntitySerializer.Deserialize(CreateDoc(), null));
			Assert.Throws<ArgumentNullException>(() => EntitySerializer.Deserialize(null, config));
		}

		[Fact]
		public void ShouldDeserializeStringProperty()
		{
			var document = CreateDoc(new { name = "John" });
			var entity = (Entity)EntitySerializer.Deserialize(document, config);
			Assert.Equal("John", entity.Name);
		}

		[Fact]
		public void ShouldDeserializeIntProperty()
		{
			var document =  CreateDoc(new { age = 18 });
			var entity = (Entity)EntitySerializer.Deserialize(document, config);
			Assert.Equal(18, entity.Age);
		}

		[Fact]
		public void ShouldDeserializeDateTimeProperty()
		{
			var document =  CreateDoc(new { birthday = "2011-06-01T12:04:34.444Z" });
			var entity = (Entity)EntitySerializer.Deserialize(document, config);
			Assert.Equal(new DateTime(2011, 06, 01, 12, 04, 34, 444, DateTimeKind.Utc), entity.Birthday);
		}

		[Fact]
		public void ShouldDeserializeFields()
		{
			var document =  CreateDoc(new { field = "quantum mechanics" });
			var entity = (Entity)EntitySerializer.Deserialize(document, config);
			Assert.Equal("quantum mechanics", entity.Field);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\t")]
		public void ShouldThrowOnEmptyNullOrWightspaceId(string id)
		{
			dynamic document = new JObject();
			if (id != null)
				document._id = id;

			Assert.Throws<DocumentIdMissingException>(() => EntitySerializer.Deserialize((JObject)document, config));
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
			Assert.Throws<InvalidOperationException>(() => EntitySerializer.Deserialize(CreateDoc(), config));
		}

		[Fact(DisplayName = "hello")]
		public void ShouldNotSetTypeProperty()
		{
			var document = CreateDoc();
			var entity = (Entity)EntitySerializer.Deserialize(document, config);
			Assert.Null(entity.Type);
		}

		[Fact]
		public void ShouldSetTypePropertyOnSubobjects()
		{
			var document = CreateDoc();
			var entity = (Entity)EntitySerializer.Deserialize(document, config);
			Assert.Null(entity.Type);
		}
	}
}
