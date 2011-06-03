using System;
using CouchDude.Core.Configuration;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;
using Xunit.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global	
// ReSharper disable NotAccessedField.Global
namespace CouchDude.Tests.Unit.Impl
{
	public class EntitySerializerSerializeTests
	{
		public class SampleEntity
		{
			public DateTime Timestamp { get; set; }
			public string Name { get; set; }
			public string Field;
		}

		object entity = new SampleEntity();

		private IEntityConfig config;

		public EntitySerializerSerializeTests()
		{
			config = MockEntityConfig();
		}

		private IEntityConfig MockEntityConfig(
			Action<Mock<IEntityConfig>> additionalActions = null,
			string documentType = "sampleEntity",
			Type entityType = null)
		{
			var configMock = new Mock<IEntityConfig>();
			configMock.Setup(ec => ec.GetId(It.IsAny<object>())).Returns("doc1");
			configMock.Setup(ec => ec.DocumentType).Returns(documentType);
			configMock.Setup(ec => ec.EntityType).Returns(entityType ?? typeof(SampleEntity));
			configMock
				.Setup(ec => ec.ConvertEntityIdToDocumentId(It.IsAny<string>()))
				.Returns<string>(entityId => documentType + "." + entityId);
			if (additionalActions != null)
				additionalActions(configMock);
			return configMock.Object;
		}

		[Fact]
		public void ShouldThrowOnNullArguments()
		{
			Assert.Throws<ArgumentNullException>(() => EntitySerializer.Serialize(entity, null));
			Assert.Throws<ArgumentNullException>(() => EntitySerializer.Serialize(null, config)); 
		}

		[Fact]
		public void ShouldSetIdAndTypePropertiesOnJObject()
		{
			var document = EntitySerializer.Serialize(entity, config);
			Assert.Equal("sampleEntity.doc1", document.Value<string>("_id"));
		}

		[Fact]
		public void ShouldSetRevPropertyOnJObject()
		{
			config = MockEntityConfig(mock => mock.Setup(ec => ec.GetRevision(entity)).Returns("rev.1"));
			var document = EntitySerializer.Serialize(entity, config);
			Assert.Equal("rev.1", document.Value<string>("_rev"));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\t")]
		public void ShouldThrowIfDocumentTypeIsNullEmptyOrWhitespace(string invalidDocumentType)
		{
			config = MockEntityConfig(
				mock => mock.Setup(ec => ec.DocumentType).Returns(invalidDocumentType));
			Assert.Throws<InvalidOperationException>(() => EntitySerializer.Serialize(entity, config));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\t")]
		public void ShouldThrowIfDocumentIdIsNullEmptyOrWhitespace(string invalidEntityId)
		{
			config = MockEntityConfig(
				mock => mock.Setup(ec => ec.GetId(entity)).Returns(invalidEntityId));
			Assert.Throws<ArgumentException>(() => EntitySerializer.Serialize(entity, config));
		}

		[Fact]
		public void ShouldSerializeDatesAccodingToIso8601()
		{
			entity = new SampleEntity {Timestamp = new DateTime(2011, 06, 01, 12, 04, 34, 444, DateTimeKind.Utc)};
			var document = EntitySerializer.Serialize(entity, config);
			Assert.Equal("2011-06-01T12:04:34.444Z", document.Value<string>("timestamp"));
		}

		[Fact]
		public void ShouldConvertPropertyNameToCamelCase()
		{
			entity = new SampleEntity {Name = "john"};
			var document = EntitySerializer.Serialize(entity, config);
			Assert.NotNull(document.Property("name"));
		}

		public void ShouldSerializePublicFields()
		{
			entity = new SampleEntity {Field = "quantum mechanics"};
			var document = EntitySerializer.Serialize(entity, config);
			Assert.Equal("quantum mechanics", document.Value<string>("field"));
		}

		public class EntityA
		{
			public string One { get; set; }
			public string Two;
		}

		public class EntityB
		{
			public string One { get; set; }
			public string Two;
		}

		public void ShouldIgnoreMembersIfConfiguredInEntityConfig()
		{
			var entityA = new EntityA {One = "one", Two = "two"};
			var entityB = new EntityB {One = "one", Two = "two"};

			dynamic documentA = EntitySerializer.Serialize(entityA, MockEntityConfig(
				mock => mock.Setup(ec => ec.IgnoredMembers).Returns(typeof (EntityA).GetMember("One")),
				documentType: "entityA",
				entityType: typeof(EntityA)
			));
			dynamic documentB = EntitySerializer.Serialize(entityB, MockEntityConfig(
				mock => mock.Setup(ec => ec.IgnoredMembers).Returns(typeof (EntityA).GetMember("Two")),
				documentType: "entityB",
				entityType: typeof(EntityB)
			));

			Assert.Null(documentA.one);
			Assert.Equal("two", documentA.two);

			Assert.Equal("one", documentB.one);
			Assert.Null(documentB.two);
		}

		public class SelfReferencingComponent
		{
			public SelfReferencingEntity SubEntity { get; set; }
		}

		public class SelfReferencingEntity
		{
			public SelfReferencingComponent Component { get; set; }
		}

		[Fact]
		public void ShouldThrowIfSelfReferencingEntityDetected()
		{
			entity = new SelfReferencingEntity {
					Component = new SelfReferencingComponent {
					SubEntity = new SelfReferencingEntity()
				}
			};
			config = MockEntityConfig(
				documentType: "selfReferencingEntity",
				entityType: typeof(SelfReferencingEntity)
			);
			var exception = 
				Assert.Throws<InvalidOperationException>(() => EntitySerializer.Serialize(entity, config));
			Assert.Contains(typeof(SelfReferencingEntity).Name, exception.Message);
		}

		[Fact]
		public void ShouldThrowOnUncompatibleEntityAndEntityConfig()
		{
			entity = new SampleEntity();
			config = MockEntityConfig(
				documentType: "simpleEntity",
				entityType: typeof(SimpleEntity)
			);
			Assert.Throws<InvalidOperationException>(() => EntitySerializer.Serialize(entity, config));
		}
	}
}
