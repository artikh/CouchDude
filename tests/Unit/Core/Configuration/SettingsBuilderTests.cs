#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov																					
																																					
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
					.FromAssemblyOf<Entity>()
					.Implementing<IEntity>()
				.CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(EntityWithoutRevision));
			
			Assert.NotNull(simpleEntityConfig);
			Assert.NotNull(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldIterateOverAssemblyTypesRegisteringUsingBaseType()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<Entity>()
					.InheritedFrom<Entity>()
				.CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(EntityWithoutRevision));
			
			Assert.NotNull(simpleEntityConfig);
			Assert.Null(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldExplictlyRegisterTypes()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DatabaseName("db1")
				.MappingEntitiy<Entity>()
				.CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(EntityWithoutRevision));
			
			Assert.NotNull(simpleEntityConfig);
			Assert.Null(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldIterateOverAssemblyTypesRegisteringUsingProvidedPredicate()
		{
			Settings settings = ConfigureCouchDude.With()
				.ServerUri("http://example.com").DatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<Entity>()
					.Where(t => t.Name.StartsWith("Entity"))
				.CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(EntityWithoutRevision));

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

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntityWithoutRevisionConfig = settings.TryGetConfig(typeof(EntityWithoutRevision));

			Assert.NotNull(simpleEntityConfig);
			Assert.NotNull(simpleEntityWithoutRevisionConfig);
		}

		[Fact]
		public void ShouldDirectlySetCustomEntityConfig()
		{
			var customConfig = Mock.Of<IEntityConfig>(
				c => c.EntityType == typeof(Entity) && c.DocumentType == "simpleEntity");

			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .InheritedFrom<Entity>()
					 .WithCustomConfig(t => customConfig)
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));

			Assert.Same(customConfig, simpleEntityConfig);
		}

		[Fact]
		public void ShouldSetDocumentTypePolicy()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .Implementing<IEntity>()
					 .WhenDocumentType(t => "_" + Char.ToLower(t.Name[0]) + t.Name.Substring(1))
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));
			Assert.Equal("_entity", simpleEntityConfig.DocumentType);
		}

		[Fact]
		public void ShouldSetEntityIdToDocumentIdConversion()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .Implementing<IEntity>()
					 .TranslatingEntityIdToDocumentIdAs((entityId, entityType, documentType) => "Entity#" + entityId)
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));
			Assert.Equal("Entity#42", simpleEntityConfig.ConvertEntityIdToDocumentId("42"));
		}

		[Fact]
		public void ShouldSetDocumentIdToEntityIdConversion()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .Implementing<IEntity>()
					 .TranslatingDocumentIdToEntityIdAs((documentId, documentType, entityType) => "Document#" + documentId)
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));
			Assert.Equal("Document#42", simpleEntityConfig.ConvertDocumentIdToEntityId("42"));
		}

		[Fact]
		public void ShouldSetIdMemberPolicy()
		{
			Settings settings = ConfigureCouchDude.With()
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .InheritedFrom<Entity>()
					 .WhenIdMember(entityType => (MemberInfo)entityType.GetProperty("Name"))
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntity = new Entity {Name = "Alex"};

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
					.ServerUri("http://example.com").DatabaseName("db1")
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
					.ServerUri("http://example.com").DatabaseName("db1")
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
				 .ServerUri("http://example.com").DatabaseName("db1")
				 .MappingEntities()
					 .FromAssemblyOf<Entity>()
					 .Implementing<IEntity>()
					 .WhenRevisionMember(entityType => (MemberInfo)entityType.GetProperty("Name"))
				 .CreateSettings();

			var simpleEntityConfig = settings.TryGetConfig(typeof(Entity));
			var simpleEntity = new Entity { Name = "Alex" };

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
					.ServerUri("http://example.com").DatabaseName("db1")
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
					.ServerUri("http://example.com").DatabaseName("db1")
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
				.ServerUri("http://example.com").DatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<Entity>()
					.Where(t => t.Name == "Entity")
					.WithCustomConfig(t => customConfigA)
				.MappingEntities()
					.FromAssembly("CouchDude.Tests")
					.InheritedFrom<Entity>()
					.WithCustomConfig(t => customConfigB)
				.CreateSettings();

			var simpleEntityConfig = settings.GetConfig(typeof (Entity));
			Assert.Same(customConfigB, simpleEntityConfig);
		}
	}
}