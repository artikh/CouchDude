using CouchDude.Core;
using CouchDude.Core.Configuration;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Configuration
{
	public class EntityRegistryTests
	{
		readonly EntityRegistry registry = new EntityRegistry();
		readonly IEntityConfig entityConifg = Mock.Of<IEntityConfig>(
			c => c.DocumentType == "simpleEntity" && c.EntityType == typeof(SimpleEntity));
		
		[Fact]
		public void ShouldStoreEntityConfigAndRetriveByDocumentType()
		{
			registry.Register(entityConifg);
			Assert.Equal(entityConifg, registry["simpleEntity"]);
		}

		[Fact]
		public void ShouldStoreEntityConfigAndRetriveByEntityType()
		{
			registry.Register(entityConifg);

			Assert.Equal(entityConifg, registry[typeof(SimpleEntity)]);
		}

		[Fact]
		public void ShouldThrowOnDuplicateDocumentType()
		{
			registry.Register(entityConifg);

			Assert.Throws<ConfigurationException>(
				() =>
				registry.Register(
					Mock.Of<IEntityConfig>(
						c => c.DocumentType == "simpleEntity" && c.EntityType == typeof (SimpleEntityWithoutRevision))));
		}

		[Fact]
		public void ShouldThrowOnDuplicateEntityType()
		{
			registry.Register(entityConifg);

			Assert.Throws<ConfigurationException>(
				() =>
				registry.Register(
					Mock.Of<IEntityConfig>(c => c.DocumentType == "simpleEntity2" && c.EntityType == typeof (SimpleEntity))));
		}

		[Fact]
		public void ShouldThrowOnUnknownEntityType()
		{
			var exception = Assert.Throws<EntityTypeNotRegistredException>(() => registry[typeof(SimpleEntity)]);
			Assert.Contains(typeof(SimpleEntity).FullName, exception.Message);
		}

		[Fact]
		public void ShouldThrowOnUnknownDocumentType()
		{
			var exception = Assert.Throws<DocumentTypeNotRegistredException>(() => registry["simpleEntity"]);
			Assert.Contains("simpleEntity", exception.Message);
		}
	}
}
