using System;
using System.Linq;
using System.Reflection;
using CouchDude.Core;
using CouchDude.Core.Configuration;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Configuration
{
	public class SettingsBuilderTests
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
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(SimpleEntityWithoutRevision));
			
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
					.Where(t => t.GetInterfaces().Any(i => i.Name == "IEntity"))
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
					 .InheritedFrom<SimpleEntity>()
					 .WithCustomConfig(t => customConfig)
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));

			Assert.Same(customConfig, simpleEntityConfig);
		}

		[Fact]
		public void ShouldSetDocumentTypePolicy()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .Implementing<IEntity>()
					 .WhenDocumentType(t => "_" + Char.ToLower(t.Name[0]) + t.Name.Substring(1))
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			Assert.Equal("_simpleEntity", simpleEntityConfig.DocumentType);
		}

		[Fact]
		public void ShouldSetEntityIdToDocumentIdConversion()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .Implementing<IEntity>()
					 .TranslatingEntityIdToDocumentIdAs((entityId, entityType, documentType) => "Entity#" + entityId)
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			Assert.Equal("Entity#42", simpleEntityConfig.ConvertEntityIdToDocumentId("42"));
		}

		[Fact]
		public void ShouldSetDocumentIdToEntityIdConversion()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .Implementing<IEntity>()
					 .TranslatingDocumentIdToEntityIdAs((documentId, documentType, entityType) => "Document#" + documentId)
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(SimpleEntity));
			Assert.Equal("Document#42", simpleEntityConfig.ConvertDocumentIdToEntityId("42"));
		}

		[Fact]
		public void ShouldSetIdMemberPolicy()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .Implementing<IEntity>()
					 .WhenIdMember(entityType => (MemberInfo)entityType.GetProperty("Name"))
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
			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.Implementing<IEntity>()
						.WhenIdMember(entityType => (MemberInfo)entityType.GetField("OkResponse"))
					.CreateSettings()
			);
		}

		[Fact]
		public void ShouldThrowOnNonePropertyOrFieldIdMember()
		{
			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.Implementing<IEntity>()
						.WhenIdMember(entityType => (MemberInfo)entityType.GetMethod("DoStuff"))
					.CreateSettings());
		}

		[Fact]
		public void ShouldThrowOnNullIdMember()
		{
			Assert.Throws<ConfigurationException>(() => 
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
						.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.Implementing<IEntity>()
						.WhenIdMember(entityType => null)
					.CreateSettings());
		}

		[Fact]
		public void ShouldSetRevisionMemberPolicy()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<SimpleEntity>()
					 .Implementing<IEntity>()
					 .WhenRevisionMember(entityType => (MemberInfo)entityType.GetProperty("Name"))
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
			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.Implementing<IEntity>()
						.WhenRevisionMember(entityType => (MemberInfo)entityType.GetField("OkResponse"))
					.CreateSettings()
			);
		}

		[Fact]
		public void ShouldThrowOnNonePropertyOrFieldRevisionMember()
		{
			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.Implementing<IEntity>()
						.WhenRevisionMember(entityType => (MemberInfo)entityType.GetMethod("DoStuff"))
					.CreateSettings());
		}

		[Fact]
		public void ShouldNotThrowOnNullRevMember()
		{
			Assert.DoesNotThrow(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DatabaseName("db1")
						.MappingEntities()
						.FromAssemblyOf<SimpleEntity>()
						.Implementing<IEntity>()
						.WhenRevisionMember(entityType => null)
					.CreateSettings());
		}
	}
}