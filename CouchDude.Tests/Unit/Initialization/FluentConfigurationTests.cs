using System;
using System.Linq;
using System.Reflection;
using CouchDude.Core;
using CouchDude.Core.Configuration;
using CouchDude.Core.Initialization;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Initialization
{
	public class FluentConfigurationTests
	{
		[Fact]
		public void ShouldSetServerUriAndDatabaseName()
		{
			var settings = ConfigureCouchDude.With().ServerUri("http://example.com").DatabaseName("db1").CreateSettings();

			Assert.Equal(new Uri("http://example.com"), settings.ServerUri);
			Assert.Equal("db1", settings.DatabaseName);
		}

		[Fact]
		public void ShouldThrowOnIncompleteInitialization()
		{
			Assert.Throws<ConfigurationException>(
				() => ConfigureCouchDude.With().ServerUri("http://example.com").CreateSettings());
			Assert.Throws<ConfigurationException>(() => ConfigureCouchDude.With().DatabaseName("db1").CreateSettings());
		}

		[Fact]
		public void ShouldIterateOverAssemblyTypesRegisteringUsingInterface()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<SimpleEntity>()
					.Implementing<IEntity>()
				.CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(SimpleEntityWithoutRevision));
			
			Assert.NotNull(simpleEntityConfig);
			Assert.NotNull(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldIterateOverAssemblyTypesRegisteringUsingBaseType()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<SimpleEntity>()
					.InheritedFrom<SimpleEntity>()
				.CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(SimpleEntityWithoutRevision));
			
			Assert.NotNull(simpleEntityConfig);
			Assert.Null(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldExplictlyRegisterTypes()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DatabaseName("db1")
				.MappingEntitiy<SimpleEntity>()
				.CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			
			Assert.NotNull(simpleEntityConfig);
			Assert.Null(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldIterateOverAssemblyTypesRegisteringUsingProvidedPredicate()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<SimpleEntity>()
					.Where(t => t.Name.StartsWith("SimpleEntity"))
				.CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(SimpleEntityWithoutRevision));

			Assert.NotNull(simpleEntityConfig);
			Assert.NotNull(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldProvideAbilityToPointToAssemblyByName()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DatabaseName("db1")
				.MappingEntities()
					.FromAssembly("CouchDude.Tests")
					.Where((Type t) => t.GetInterfaces().Any(i => i.Name == "IEntity"))
				.CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(SimpleEntityWithoutRevision));

			Assert.NotNull(simpleEntityConfig);
			Assert.NotNull(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldDirectlySetCustomEntityConfig()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .InheritedFrom<IEntity>()
					 .WithCustomConfig(t => customConfig)
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));

			Assert.Same(customConfig, simpleEntityConfig);
		}

		[Fact]
		public void ShouldSetDocumentTypePolicy()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .InheritedFrom<IEntity>()
					 .WhenDocumentType(t => "_" + Char.ToLower(t.Name[0]) + t.Name.Substring(1))
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			Assert.Same("_simpleEntity", simpleEntityConfig.DocumentType);
		}

		[Fact]
		public void ShouldSetEntityIdToDocumentIdConversion()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .InheritedFrom<IEntity>()
					 .TranslatingDocumentIdToEntityIdAs(entityId => "Document#" + entityId)
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			Assert.Same("Document#42", simpleEntityConfig.ConvertEntityIdToDocumentId("42"));
		}

		[Fact]
		public void ShouldSetDocumentIdToEntityIdConversion()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .InheritedFrom<IEntity>()
					 .TranslatingEntityIdToDocumentIdAs(docId => "Entity#" + docId)
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			Assert.Same("Entity#42", simpleEntityConfig.ConvertDocumentIdToEntityId("42"));
		}

		[Fact]
		public void ShouldSetIdMemberPolicy()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .InheritedFrom<IEntity>()
					 .WhenIdMember((Type entityType) => (MemberInfo)entityType.GetProperty("Name"))
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			var simpleEntity = new SimpleEntity {Name = "Alex"};

			Assert.True(simpleEntityConfig.IsIdMemberPresent);
			Assert.Equal("Alex", simpleEntityConfig.GetId(simpleEntity));
			simpleEntityConfig.SetId(simpleEntity, "John");
			Assert.Equal("John", simpleEntity.Name);
		}

		[Fact]
		public void ShouldThrowOnNoneInstanceIdMember()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.InheritedFrom<IEntity>()
						.WhenIdMember((Type entityType) => (MemberInfo)entityType.GetField("OkResponse"))
					.CreateSettings()
			);
		}

		[Fact]
		public void ShouldThrowOnNonePropertyOrFieldIdMember()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.InheritedFrom<IEntity>()
						.WhenIdMember((Type entityType) => (MemberInfo)entityType.GetMethod("DoStuff"))
					.CreateSettings());
		}

		[Fact]
		public void ShouldThrowOnNullIdMember()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Assert.Throws<ConfigurationException>(() => 
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
						.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.InheritedFrom<IEntity>()
						.WhenIdMember((Type entityType) => null)
					.CreateSettings());
		}

		[Fact]
		public void ShouldSetRevisionMemberPolicy()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .InheritedFrom<IEntity>()
					 .WhenRevisionMember((Type entityType) => (MemberInfo)entityType.GetProperty("Name"))
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			var simpleEntity = new SimpleEntity { Name = "Alex" };

			Assert.True(simpleEntityConfig.IsRevisionPresent);
			Assert.Equal("Alex", simpleEntityConfig.GetRevision(simpleEntity));
			simpleEntityConfig.SetRevision(simpleEntity, "John");
			Assert.Equal("John", simpleEntity.Name);
		}

		[Fact]
		public void ShouldThrowOnNoneInstanceRevisionMember()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.InheritedFrom<IEntity>()
						.WhenRevisionMember((Type entityType) => (MemberInfo)entityType.GetField("OkResponse"))
					.CreateSettings()
			);
		}

		[Fact]
		public void ShouldThrowOnNonePropertyOrFieldRevisionMember()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.InheritedFrom<IEntity>()
						.WhenRevisionMember((Type entityType) => (MemberInfo)entityType.GetMethod("DoStuff"))
					.CreateSettings());
		}

		[Fact]
		public void ShouldNotThrowOnNullRevMember()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(SimpleEntity) && c.DocumentType == "simpleEntity");

			Assert.DoesNotThrow(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
						.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.InheritedFrom<IEntity>()
						.WhenRevisionMember((Type entityType) => null)
					.CreateSettings());
		}
	}
}