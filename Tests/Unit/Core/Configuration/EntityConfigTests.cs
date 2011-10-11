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
	public class EntityConfigTests
	{
		public class EntityWithoutDefaultConstructor
		{
			private readonly int id;

			public EntityWithoutDefaultConstructor(int id)
			{
				this.id = id;
			}

			public int ID
			{
				get { return id; }
			}
		}

		public class EntityWithPrivateDefaultConstructor : EntityWithoutDefaultConstructor
		{
			public EntityWithPrivateDefaultConstructor(int id) : base(id) { }

			// ReSharper disable UnusedMember.Local
			private EntityWithPrivateDefaultConstructor(): this(42) { }
			// ReSharper restore UnusedMember.Local
		}

		[Fact]
		public void ShouldThrowConfigurationExceptionOnEntityWithoutDefaultConstructor()
		{
			Assert.Throws<ConfigurationException>(() => new EntityConfig(typeof(EntityWithoutDefaultConstructor)));
		}

		[Fact]
		public void ShouldNotThrowOnEntityWithoutPrivateDefaultConstructor()
		{
			Assert.DoesNotThrow(() => new EntityConfig(typeof(EntityWithPrivateDefaultConstructor)));
		}

		[Fact]
		public void ShouldDelegateDocumentTypeGeneration()
		{
			var entityConfig = new EntityConfig(typeof(Entity), entityTypeToDocumentType: entityType => "docType1");
			Assert.Equal("docType1", entityConfig.DocumentType);
		}

		[Fact]
		public void ShouldThrowOnNullInputToConstructor()
		{
			Assert.Throws<ArgumentNullException>(() => new EntityConfig(null));
		}

		[Fact]
		public void ShouldDelegateSetEnityId()
		{
			string setId = null;
			object settingEntity = null;

			var idMemberMock = new Mock<ISpecialMember>();
			idMemberMock
				.Setup(m => m.SetValue(It.IsAny<object>(), It.IsAny<string>()))
				.Callback<object, string>((e, id) => { setId = id; settingEntity = e; });

			var entityConfig = new EntityConfig(typeof(Entity), idMember: idMemberMock.Object); 

			object entity = Entity.CreateStandard();
			entityConfig.SetId(entity, "doc1");

			Assert.Equal("doc1", setId);
			Assert.Equal(entity, settingEntity);
			
		}

		[Fact]
		public void ShouldThrowOnNullInputToSetId()
		{
			var entityConfig = new EntityConfig(typeof(Entity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.SetId(null, "entity1"));
			Assert.Throws<ArgumentNullException>(() => entityConfig.SetId(Entity.CreateStandard(), null));
			Assert.Throws<ArgumentNullException>(() => entityConfig.SetId(Entity.CreateStandard(), string.Empty));
		}

		[Fact]
		public void ShouldDelegateGetEntityId()
		{
			object gettingEntity = null;

			var idMemberMock = new Mock<ISpecialMember>();
			idMemberMock
				.Setup(m => m.GetValue(It.IsAny<object>()))
				.Returns<object>(e => { gettingEntity = e; return "doc1"; });

			var entityConfig = new EntityConfig(typeof(Entity), idMember: idMemberMock.Object);

			object entity = Entity.CreateStandard();
			string id = entityConfig.GetId(entity);

			Assert.Equal("doc1", id);
			Assert.Equal(entity, gettingEntity);
		}

		[Fact]
		public void ShouldThrowOnNullInputToGetId()
		{
			var entityConfig = new EntityConfig(typeof(Entity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.GetId(null));
		} 

		[Fact]
		public void ShouldDelegateSetEnityRevision()
		{
			string setRev = null;
			object settingEntity = null;

			var revisionMemberMock = new Mock<ISpecialMember>();
			revisionMemberMock
			.Setup(m => m.SetValue(It.IsAny<object>(), It.IsAny<string>()))
			.Callback<object, string>((e, rev) => { setRev = rev; settingEntity = e; });

			var entityConfig = new EntityConfig(typeof(Entity), revisionMember: revisionMemberMock.Object);

			object entity = Entity.CreateStandard();
			entityConfig.SetRevision(entity, "rev1");

			Assert.Equal("rev1", setRev);
			Assert.Equal(entity, settingEntity);
		}

		[Fact]
		public void ShouldThrowOnNullInputToSetEnityRevision()
		{
			var entityConfig = new EntityConfig(typeof(Entity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.SetRevision(null, "rev1"));
			Assert.Throws<ArgumentNullException>(() => entityConfig.SetRevision(Entity.CreateStandard(), null));
			Assert.Throws<ArgumentNullException>(() => entityConfig.SetRevision(Entity.CreateStandard(), string.Empty));
		}

		[Fact]
		public void ShouldDelegateGetEntityRevision()
		{
			object gettingEntity = null;

			var revisionMemberMock = new Mock<ISpecialMember>();
			revisionMemberMock
				.Setup(m => m.GetValue(It.IsAny<object>()))
				.Returns<object>(e => { gettingEntity = e; return "rev1"; });

			var entityConfig = new EntityConfig(typeof(Entity), revisionMember: revisionMemberMock.Object);

			object entity = Entity.CreateStandard();
			var revision = entityConfig.GetRevision(entity);

			Assert.Equal("rev1", revision);
			Assert.Equal(entity, gettingEntity);
		}

		[Fact]
		public void ShouldThrowOnNullInputToGetRevision()
		{
			var entityConfig = new EntityConfig(typeof(Entity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.GetRevision(null));
		}

		[Fact]
		public void ShouldThrowOnIncorrectEntityTypeOnSetterAndGetterMethods()
		{
			var entityConfig = new EntityConfig(typeof(Entity));

			Assert.Throws<ArgumentException>(() => entityConfig.GetRevision(new EntityWithoutRevision()));
			Assert.Throws<ArgumentException>(() => entityConfig.GetId(new EntityWithoutRevision()));
			Assert.Throws<ArgumentException>(() => entityConfig.SetRevision(new EntityWithoutRevision(), "rev1"));
			Assert.Throws<ArgumentException>(() => entityConfig.SetId(new EntityWithoutRevision(), "entity1"));
		}

		[Fact]
		public void ShouldDelegateConvertEntityIdToDocumentId()
		{
			string providedEntityId = null;
			Type providedEntityType = null;

			var entityConfig = new EntityConfig(
				typeof(Entity),
				entityIdToDocumentId: (entityId, entityType, documentType) => {
					providedEntityId = entityId;
					providedEntityType = entityType;
					return "doc1";
				}
			);

			var returnedDocId = entityConfig.ConvertEntityIdToDocumentId("entity1");

			Assert.Equal(returnedDocId, "doc1");
			Assert.Equal("entity1", providedEntityId);
			Assert.Equal(typeof(Entity), providedEntityType);
		}

		[Fact]
		public void ShouldThrowOnNullInputToConvertEntityIdToDocumentId()
		{
			var entityConfig = new EntityConfig(typeof(Entity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.ConvertEntityIdToDocumentId(null));
			Assert.Throws<ArgumentNullException>(() => entityConfig.ConvertEntityIdToDocumentId(string.Empty));
		}

		[Fact]
		public void ShouldDelegateConvertDocumentIdToEntityId()
		{
			string providedDocumentId = null;
			Type providedEntityType = null;

			var entityConfig = new EntityConfig(
				typeof(Entity),
				documentIdToEntityId: (documentId, documentType, entityType) => {
					providedDocumentId = documentId;
					providedEntityType = entityType;
					return "entity1";
				}
			);

			var returnedEntityId = entityConfig.ConvertDocumentIdToEntityId("doc1");

			Assert.Equal(returnedEntityId, "entity1");
			Assert.Equal("doc1", providedDocumentId);
			Assert.Equal(typeof(Entity), providedEntityType);
		}

		[Fact]
		public void ShouldThrowOnNullInputToConvertDocumentIdToEntityId()
		{
			var entityConfig = new EntityConfig(typeof(Entity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.ConvertEntityIdToDocumentId(null));
			Assert.Throws<ArgumentNullException>(() => entityConfig.ConvertEntityIdToDocumentId(string.Empty));
		}

		[Fact]
		public void ShouldDelegateGetIgnoredMembers()
		{
			var thisMethodInfo = GetType().GetMethod("ShouldDelegateGetIgnoredMembers");
			var idMember = Mock.Of<ISpecialMember>(sm => sm.IsDefined == true && sm.RawMemberInfo == thisMethodInfo);
			var revMember = Mock.Of<ISpecialMember>(sm => sm.IsDefined == false);

			var entityConfig = new EntityConfig(typeof(Entity), idMember: idMember, revisionMember: revMember);

			Assert.Contains(thisMethodInfo, entityConfig.IgnoredMembers);
		}

		[Fact]
		public void ShouldReturnEmptyIgnoredMembersOnNullReturnedFromConvention()
		{
			var idMember = Mock.Of<ISpecialMember>(sm => sm.IsDefined == true && sm.RawMemberInfo == null);
			var revMember = Mock.Of<ISpecialMember>(sm => sm.IsDefined == true && sm.RawMemberInfo == null);

			var entityConfig = new EntityConfig(typeof(Entity), idMember: idMember, revisionMember: revMember);
			Assert.NotNull(entityConfig.IgnoredMembers);
			Assert.Equal(0, entityConfig.IgnoredMembers.Count());
		}
	}
}
