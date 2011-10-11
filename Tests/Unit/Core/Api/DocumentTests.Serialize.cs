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
using CouchDude.Api;
using CouchDude.Configuration;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;
using Xunit.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global	
// ReSharper disable NotAccessedField.Global
namespace CouchDude.Tests.Unit.Core.Api
{
	public class DocumentTestsSerialize
	{
		public enum UserSex
		{
			Male,
			Female
		}

		public class User
		{
			public DateTime Timestamp { get; set; }
			public string Name { get; set; }
			public string Field;
			public UserSex Sex { get; set; }
		}

		object entity = new User();

		private IEntityConfig config;

		public DocumentTestsSerialize()
		{
			config = MockEntityConfig();
		}

		private static IEntityConfig MockEntityConfig(
			Action<Mock<IEntityConfig>> additionalActions = null,
			string documentType = "sampleEntity",
			Type entityType = null)
		{
			var configMock = new Mock<IEntityConfig>();
			configMock.Setup(ec => ec.GetId(It.IsAny<object>())).Returns("doc1");
			configMock.Setup(ec => ec.DocumentType).Returns(documentType);
			configMock.Setup(ec => ec.EntityType).Returns(entityType ?? typeof(User));
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
			Assert.Throws<ArgumentNullException>(() => Document.Serialize(entity, null));
			Assert.Throws<ArgumentNullException>(() => Document.Serialize(null, config)); 
		}

		[Fact]
		public void ShouldSetIdAndTypePropertiesOnJObject()
		{
			dynamic document = Document.Serialize(entity, config);
			Assert.Equal("sampleEntity.doc1", (string)document._id);
		}

		[Fact]
		public void ShouldSerializeSpecialPropertiesFirst()
		{
			var document = Document.Serialize(Entity.CreateStandard(), Default.Settings.GetConfig(typeof (Entity)));
			Assert.Equal(Entity.CreateDocWithRevision(), document);
		}
		
		[Fact]
		public void ShouldSetRevPropertyOnJObject()
		{
			config = MockEntityConfig(mock => {
			  mock.Setup(
			    ec => ec.GetRevision(entity)).Returns("rev.1");
			  mock.Setup(ec => ec.IsRevisionPresent).Returns(true);
			});
			var document = Document.Serialize(entity, config);
			Assert.Equal("rev.1", document.Revision);
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
			Assert.Throws<InvalidOperationException>(() => Document.Serialize(entity, config));
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
			Assert.Throws<ArgumentException>(() => Document.Serialize(entity, config));
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

			dynamic documentA = Document.Serialize(entityA, MockEntityConfig(
				mock => mock.Setup(ec => ec.IgnoredMembers).Returns(typeof (EntityA).GetMember("One")),
				documentType: "entityA",
				entityType: typeof(EntityA)
			));
			dynamic documentB = Document.Serialize(entityB, MockEntityConfig(
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
				Assert.Throws<InvalidOperationException>(() => Document.Serialize(entity, config));
			Assert.Contains(typeof(SelfReferencingEntity).Name, exception.Message);
		}

		[Fact]
		public void ShouldThrowOnUncompatibleEntityAndEntityConfig()
		{
			entity = new User();
			config = MockEntityConfig(
				documentType: "simpleEntity",
				entityType: typeof(Entity)
			);
			Assert.Throws<InvalidOperationException>(() => Document.Serialize(entity, config));
		}
	}
}
