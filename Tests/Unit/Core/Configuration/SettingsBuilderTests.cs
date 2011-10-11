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
using System.Linq;
using System.Reflection;

using CouchDude.Configuration;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Configuration
{
	public class SettingsBuilderTests
	{
		[Fact]
		public void ShouldSetServerUriAndDatabaseName()
		{
			var settings = ConfigureCouchDude.With().ServerUri("http://example.com").DefaultDatabaseName("db1").CreateSettings();

			Assert.Equal(new Uri("http://example.com"), settings.ServerUri);
			Assert.Equal("db1", settings.DefaultDatabaseName);
		}

		[Fact]
		public void ShouldThrowOnIncompleteInitialization()
		{
			Assert.Throws<ConfigurationException>(
				() => ConfigureCouchDude.With().ServerUri("http://example.com").CreateSettings());
			Assert.Throws<ConfigurationException>(() => ConfigureCouchDude.With().DefaultDatabaseName("db1").CreateSettings());
		}

		[Fact]
		public void ShouldIterateOverAssemblyTypesRegisteringUsingInterface()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DefaultDatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<Entity>()
					.Implementing<IEntity>()
				.CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(EntityWithoutRevision));
			
			Assert.NotNull(entityConfig);
			Assert.NotNull(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldIterateOverAssemblyTypesRegisteringUsingBaseType()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DefaultDatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<Entity>()
					.InheritedFrom<Entity>()
				.CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(EntityWithoutRevision));
			
			Assert.NotNull(entityConfig);
			Assert.Null(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldExplictlyRegisterTypes()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DefaultDatabaseName("db1")
				.MappingEntitiy<Entity>()
				.CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(EntityWithoutRevision));
			
			Assert.NotNull(entityConfig);
			Assert.Null(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldIterateOverAssemblyTypesRegisteringUsingProvidedPredicate()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DefaultDatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<Entity>()
					.Where(t => t.Name.StartsWith("Entity"))
				.CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(EntityWithoutRevision));

			Assert.NotNull(entityConfig);
			Assert.NotNull(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldProvideAbilityToPointToAssemblyByName()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DefaultDatabaseName("db1")
				.MappingEntities()
					.FromAssembly("CouchDude.Tests")
					.Where(t => t.GetInterfaces().Any(i => i.Name == "IEntity"))
				.CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(EntityWithoutRevision));

			Assert.NotNull(entityConfig);
			Assert.NotNull(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldDirectlySetCustomEntityConfig()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(Entity) && c.DocumentType == "simpleEntity");

			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DefaultDatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .InheritedFrom<Entity>()
					 .WithCustomConfig(t => customConfig)
				 .CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));

			Assert.Same(customConfig, entityConfig);
		}

		[Fact]
		public void ShouldSetDocumentTypePolicy()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DefaultDatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .Implementing<IEntity>()
					 .WhenDocumentType(t => "_" + Char.ToLower(t.Name[0]) + t.Name.Substring(1))
				 .CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));
			Assert.Equal("_entity", entityConfig.DocumentType);
		}

		[Fact]
		public void ShouldSetEntityIdToDocumentIdConversion()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DefaultDatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .Implementing<IEntity>()
					 .TranslatingEntityIdToDocumentIdAs((entityId, entityType, documentType) => "Entity#" + entityId)
				 .CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));
			Assert.Equal("Entity#42", entityConfig.ConvertEntityIdToDocumentId("42"));
		}

		[Fact]
		public void ShouldSetDocumentIdToEntityIdConversion()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DefaultDatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .Implementing<IEntity>()
					 .TranslatingDocumentIdToEntityIdAs((documentId, documentType, entityType) => "Document#" + documentId)
				 .CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));
			Assert.Equal("Document#42", entityConfig.ConvertDocumentIdToEntityId("42"));
		}

		[Fact]
		public void ShouldSetIdMemberPolicy()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DefaultDatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .InheritedFrom<Entity>()
					 .WhenIdMember(entityType => (MemberInfo)entityType.GetProperty("Name"))
				 .CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntity = new Entity {Name = "Alex"};

			Assert.True(entityConfig.IsIdMemberPresent);
			Assert.Equal("Alex", entityConfig.GetId(simpleEntity));
			entityConfig.SetId(simpleEntity, "John");
			Assert.Equal("John", simpleEntity.Name);
		}

		[Fact]
		public void ShouldThrowOnNoneInstanceIdMember()
		{
			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DefaultDatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<Entity>()
						.InheritedFrom<Entity>()
						.WhenIdMember(entityType => (MemberInfo)entityType.GetField("OkResponse"))
					.CreateSettings()
			);
		}

		[Fact]
		public void ShouldThrowOnNonePropertyOrFieldIdMember()
		{
			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DefaultDatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<Entity>()
						.Implementing<IEntity>()
						.WhenIdMember(entityType => (MemberInfo)entityType.GetMethod("DoStuff"))
					.CreateSettings());
		}

		[Fact]
		public void ShouldThrowOnNullIdMember()
		{
			Assert.Throws<ConfigurationException>(() => 
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DefaultDatabaseName("db1")
						.MappingEntities()
						.FromAssemblyOf<Entity>()
						.Implementing<IEntity>()
						.WhenIdMember(entityType => null)
					.CreateSettings());
		}

		[Fact]
		public void ShouldSetRevisionMemberPolicy()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DefaultDatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .Implementing<IEntity>()
					 .WhenRevisionMember(entityType => (MemberInfo)entityType.GetProperty("Name"))
				 .CreateSettings();

			var entityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntity = new Entity { Name = "Alex" };

			Assert.True(entityConfig.IsRevisionPresent);
			Assert.Equal("Alex", entityConfig.GetRevision(simpleEntity));
			entityConfig.SetRevision(simpleEntity, "John");
			Assert.Equal("John", simpleEntity.Name);
		}

		[Fact]
		public void ShouldThrowOnNoneInstanceRevisionMember()
		{
			Assert.Throws<ConfigurationException>(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DefaultDatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<Entity>()
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
					.ServerUri("http://example.com").DefaultDatabaseName("db1")
					.MappingEntities()
						.FromAssemblyOf<Entity>()
						.Implementing<IEntity>()
						.WhenRevisionMember(entityType => (MemberInfo)entityType.GetMethod("DoStuff"))
					.CreateSettings());
		}

		[Fact]
		public void ShouldNotThrowOnNullRevMember()
		{
			Assert.DoesNotThrow(() =>
				ConfigureCouchDude.With()
					.ServerUri("http://example.com").DefaultDatabaseName("db1")
						.MappingEntities()
						.FromAssemblyOf<Entity>()
						.Implementing<IEntity>()
						.WhenRevisionMember(entityType => null)
					.CreateSettings());
		}

		[Fact]
		public void ShouldPickLastEntityConfigIfMultiplyHaveRegistred()
		{
			var customConfigA = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(Entity) && c.DocumentType == "simpleEntity");
			var customConfigB = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(Entity) && c.DocumentType == "simpleEntity");

			var settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DefaultDatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<Entity>()
					.Where(t => t.Name == "Entity")
					.WithCustomConfig(t => customConfigA)
				.MappingEntities()
					.FromAssembly("CouchDude.Tests")
					.InheritedFrom<Entity>()
					.WithCustomConfig(t => customConfigB)
				.CreateSettings();

			var entityConfig = settings.GetConfig(typeof (Entity));
			Assert.Same(customConfigB, entityConfig);
		}

		[Fact]
		public void ShouldSemiInheritAspectsOfConfigurationFromPreviousDeclaration()
		{
			var settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DefaultDatabaseName("db1")
				.MappingEntities()
					.FromAssembly("CouchDude.Tests")
					.Implementing<IEntity>()
					.WhenDocumentType(t => "__" + t.Name.ToLower())
				.MappingEntities()
					.FromAssembly("CouchDude.Tests")
					.Where(t => t.Name.StartsWith("Entity"))
					.WhenDocumentType(t => "__" + t.Name.ToLower() + "[{}]")
				.MappingEntities()
					.FromAssemblyOf<Entity>()
					.Where(t => t.Name == "Entity")
					.WhenIdMember(t => t.GetProperty("Name"))
				.MappingEntities()
					.FromAssembly("CouchDude.Tests")
					.InheritedFrom<Entity>()
					.WhenRevisionMember(t => t.GetProperty("Id"))
				.CreateSettings();

			var entityConfig = settings.GetConfig(typeof (Entity));

			Assert.Equal("__entity[{}]", entityConfig.DocumentType);
			Assert.Equal("e0b8400a23158b046a", entityConfig.GetId(new Entity { Name = "e0b8400a23158b046a" }));
			Assert.Equal("4a227e99a2de41689", entityConfig.GetRevision(new Entity { Id = "4a227e99a2de41689" }));
		}
	}
}