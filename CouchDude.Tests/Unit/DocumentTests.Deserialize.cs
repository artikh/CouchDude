using System;
using CouchDude.Core;
using CouchDude.Core.Configuration;
using CouchDude.Tests.SampleData;
using JetBrains.Annotations;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit
{
	public class DocumentTestsDeserialize
	{
		public class Composite
		{
			public string Type { get; set; }
			public string Id { get; set; }
		}

		public class Entity
		{
			public string Name { get; [UsedImplicitly] private set; }
			public DateTime Birthday { get; set; }
			public int Age { get; set; }
			public string Type { get; set; }
			public Composite CompositeProperty { get; set; }
			public string Field;
			public ChildEntity Child;
		}

		public class ChildEntity : Entity { }

		private IEntityConfig config;

		public DocumentTestsDeserialize()
		{
			config = MockEntityConfig();
		}

		private IEntityConfig MockEntityConfig(Action<Mock<IEntityConfig>> additionalActions = null)
		{
			var configMock = new Mock<IEntityConfig>();
			configMock
				.Setup(ec => ec.SetId(It.IsAny<object>(), It.IsAny<string>()));
			configMock
				.Setup(ec => ec.SetRevision(It.IsAny<object>(), It.IsAny<string>()));
			configMock
				.Setup(ec => ec.ConvertDocumentIdToEntityId(It.IsAny<string>()))
				.Returns<string>(docId => "E" + docId);
			configMock.Setup(ec => ec.EntityType).Returns(typeof (Entity));
			configMock.Setup(ec => ec.DocumentType).Returns("entity");
			if (additionalActions != null)
				additionalActions(configMock);
			return configMock.Object;
		}

		private static Document CreateDoc(object documentObject = null)
		{
			dynamic document = documentObject != null ? documentObject.ToJObject() : new JObject();
			document._id = "doc1";
			document._rev = "1-42";
			document.type = "entity";
			document.child = new JObject();
			document.child.type = "childType";
			return new Document(((JObject)document).ToString());
		}

		[Fact]
		public void ShouldThrowOnIncompatibleDocumentType()
		{
			Assert.Throws<InvalidOperationException>(
				() => SimpleEntity.DocWithRevision.Deserialize(new EntityConfig(typeof (SimpleEntityWithoutRevision))));
		}

		[Fact]
		public void ShouldThrowOnNullArguments()
		{
			Assert.Throws<ArgumentNullException>(() => CreateDoc().Deserialize(null));
		}


		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\t")]
		public void ShouldThrowOnEmptyNullOrWightspaceId(string id)
		{
			var doc = new Document(id == null ? new { type = "entity" }.ToJson() : new { _id = id, type = "entity" }.ToJson());

			Assert.Throws<DocumentIdMissingException>(() => doc.Deserialize(config));
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
			Assert.Throws<InvalidOperationException>(() => CreateDoc().Deserialize(config));
		}

		[Fact]
		public void ShouldThrowDocumentParseExceptionOnDocumentWithoutId()
		{
			Assert.Throws<DocumentIdMissingException>(
				() => new {
						_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
						type = "simpleEntity",
						name = "John Smith"
					}
					.ToDocument()
					.Deserialize(Default.Settings.GetConfig(typeof (SimpleEntity))
				)
			);
		}


		[Fact]
		public void ShouldThrowDocumentParseExceptionOnDocumentWithoutRevision()
		{
			Assert.Throws<DocumentRevisionMissingException>(
				() => new { _id = "simpleEntity.doc1", type = "simpleEntity", name = "John Smith" }.ToDocument()
					.Deserialize(Default.Settings.GetConfig(typeof (SimpleEntity))
				)
			);
		}


		[Fact]
		public void ShouldNotSetTypeProperty()
		{
			var document = CreateDoc();
			var entity = (Entity) document.Deserialize(config);
			Assert.Null(entity.Type);
		}

		[Fact]
		public void ShouldSetTypePropertyOnSubobjects()
		{
			var document = CreateDoc();
			var entity = (Entity) document.Deserialize(config);
			Assert.NotNull(entity.Child.Type);
		}
	}
}
