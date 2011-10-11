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

using CouchDude.Configuration;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Configuration
{
	public class EntityRegistryTests
	{
		readonly EntityRegistry registry = new EntityRegistry();
		readonly IEntityConfig entityConifg = Mock.Of<IEntityConfig>(
			c => c.DocumentType == "simpleEntity" && c.EntityType == typeof(Entity));
		
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

			Assert.Equal(entityConifg, registry[typeof(Entity)]);
		}

		[Fact]
		public void ShouldThrowOnDuplicateDocumentType()
		{
			registry.Register(entityConifg);

			Assert.Throws<ConfigurationException>(
				() =>
				registry.Register(
					Mock.Of<IEntityConfig>(
						c => c.DocumentType == "simpleEntity" && c.EntityType == typeof (EntityWithoutRevision))));
		}

		[Fact]
		public void ShouldThrowOnDuplicateEntityType()
		{
			registry.Register(entityConifg);

			Assert.Throws<ConfigurationException>(
				() =>
				registry.Register(
					Mock.Of<IEntityConfig>(c => c.DocumentType == "simpleEntity2" && c.EntityType == typeof (Entity))));
		}

		[Fact]
		public void ShouldThrowOnUnknownEntityType()
		{
			var exception = Assert.Throws<EntityTypeNotRegistredException>(() => registry[typeof(Entity)]);
			Assert.Contains(typeof(Entity).FullName, exception.Message);
		}

		[Fact]
		public void ShouldThrowOnUnknownDocumentType()
		{
			var exception = Assert.Throws<DocumentTypeNotRegistredException>(() => registry["simpleEntity"]);
			Assert.Contains("simpleEntity", exception.Message);
		}
	}
}
