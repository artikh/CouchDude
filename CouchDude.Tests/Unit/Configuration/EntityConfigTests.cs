using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using CouchDude.Core.Configuration;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Unit.Configuration
{
	public class EntityConfigTests
	{
		[Fact]
		public void ShouldDelegateDocumentTypeGeneration()
		{
			using (new Replacer<EntityTypeToDocumentTypeConvention>(
				() => EntityConfig.EntityTypeToDocumentType, 
				newValue: entityType => "docType1"))
			{
				var entityConfig = new EntityConfig(typeof(SimpleEntity));
				Assert.Equal("docType1", entityConfig.DocumentType);
			}
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
			Type entityType = null;
			using (new Replacer<TrySetEntityIdConvention>(
				() => EntityConfig.TrySetEntityId,
				newValue: (id, e, t) => { setId = id; settingEntity = e; entityType = t; return true; }))
			{
				var entityConfig = new EntityConfig(typeof(SimpleEntity)); 

				object entity = SimpleEntity.CreateStd();
				var result = entityConfig.TrySetId(entity, "doc1");

				Assert.True(result);
				Assert.Equal("doc1", setId);
				Assert.Equal(entity, settingEntity);
				Assert.Equal(typeof(SimpleEntity), entityType);
			}
		}

		[Fact]
		public void ShouldThrowOnNullInputToSetId()
		{
			var entityConfig = new EntityConfig(typeof(SimpleEntity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.TrySetId(null, "entity1"));
			Assert.Throws<ArgumentNullException>(() => entityConfig.TrySetId(SimpleEntity.CreateStd(), null));
			Assert.Throws<ArgumentNullException>(() => entityConfig.TrySetId(SimpleEntity.CreateStd(), string.Empty));
		}

		[Fact]
		public void ShouldDelegateGetEntityId()
		{
			object gettingEntity = null;
			Type entityType = null;
			using (new Replacer<TryGetEntityIdConvention>(
				() => EntityConfig.TryGetEntityId,
				newValue: (object e, Type t, out string id) => { entityType = t; gettingEntity = e; id = "doc1"; return true; }))
			{
				var entityConfig = new EntityConfig(typeof(SimpleEntity));

				object entity = SimpleEntity.CreateStd();
				string id; 
				var result = entityConfig.TryGetId(entity, out id);

				Assert.True(result);
				Assert.Equal("doc1", id);
				Assert.Equal(entity, gettingEntity);
				Assert.Equal(typeof(SimpleEntity), entityType);
			}
		}

		[Fact]
		public void ShouldThrowOnNullInputToGetId()
		{
			var entityConfig = new EntityConfig(typeof(SimpleEntity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.GetId(null));
		} 

		[Fact]
		public void ShouldDelegateSetEnityRevision()
		{
			string setRev = null;
			object settingEntity = null;
			Type entityType = null;
			using (new Replacer<SetEntityRevisionConvention>(
				() => EntityConfig.SetEntityRevision,
				newValue: (rev, e, t) => { setRev = rev; entityType = t; settingEntity = e; }))
			{
				var entityConfig = new EntityConfig(typeof(SimpleEntity));

				object entity = SimpleEntity.CreateStd();
				entityConfig.SetRevision(entity, "rev1");

				Assert.Equal("rev1", setRev);
				Assert.Equal(entity, settingEntity);
				Assert.Equal(typeof(SimpleEntity), entityType);
			}
		}

		[Fact]
		public void ShouldThrowOnNullInputToSetEnityRevision()
		{
			var entityConfig = new EntityConfig(typeof(SimpleEntity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.SetRevision(null, "rev1"));
			Assert.Throws<ArgumentNullException>(() => entityConfig.SetRevision(SimpleEntity.CreateStd(), null));
			Assert.Throws<ArgumentNullException>(() => entityConfig.SetRevision(SimpleEntity.CreateStd(), string.Empty));
		}

		[Fact]
		public void ShouldDelegateGetEntityRevision()
		{
			object gettingEntity = null;
			Type entityType = null;
			using (new Replacer<GetEntityRevisionConvention>(
				() => EntityConfig.GetEntityRevision,
				newValue: (e, t) => { gettingEntity = e; entityType = t; return "rev1"; }))
			{
				var entityConfig = new EntityConfig(typeof(SimpleEntity));

				object entity = SimpleEntity.CreateStd();
				var revision = entityConfig.GetRevision(entity);

				Assert.Equal("rev1", revision);
				Assert.Equal(entity, gettingEntity);
				Assert.Equal(typeof(SimpleEntity), entityType);
			}
		}

		[Fact]
		public void ShouldThrowOnNullInputToGetRevision()
		{
			var entityConfig = new EntityConfig(typeof(SimpleEntity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.GetRevision(null));
		}

		[Fact]
		public void ShouldThrowOnIncorrectEntityTypeOnSetterAndGetterMethods()
		{
			var entityConfig = new EntityConfig(typeof(SimpleEntity));

			Assert.Throws<ArgumentException>(() => entityConfig.GetRevision(new SimpleEntityWithoutRevision()));
			Assert.Throws<ArgumentException>(() => entityConfig.GetId(new SimpleEntityWithoutRevision()));
			Assert.Throws<ArgumentException>(() => entityConfig.SetRevision(new SimpleEntityWithoutRevision(), "rev1"));
			Assert.Throws<ArgumentException>(() => entityConfig.TrySetId(new SimpleEntityWithoutRevision(), "entity1"));
		}

		[Fact]
		public void ShouldDelegateConvertEntityIdToDocumentId()
		{
			string providedEntityId = null;
			Type providedEntityType = null;

			using (new Replacer<EntityIdToDocumentIdConvention>(
				() => EntityConfig.EntityIdToDocumentId,
				newValue: (entityId, entityType, documentType) =>
							    {
										providedEntityId = entityId;
							      providedEntityType = entityType;
										return "doc1";
							    }))
			{
				var entityConfig = new EntityConfig(typeof(SimpleEntity));

				var returnedDocId = entityConfig.ConvertEntityIdToDocumentId("entity1");

				Assert.Equal(returnedDocId, "doc1");
				Assert.Equal("entity1", providedEntityId);
				Assert.Equal(typeof(SimpleEntity), providedEntityType);
			}
		}

		[Fact]
		public void ShouldThrowOnNullInputToConvertEntityIdToDocumentId()
		{
			var entityConfig = new EntityConfig(typeof(SimpleEntity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.ConvertEntityIdToDocumentId(null));
			Assert.Throws<ArgumentNullException>(() => entityConfig.ConvertEntityIdToDocumentId(string.Empty));
		}

		[Fact]
		public void ShouldDelegateConvertDocumentIdToEntityId()
		{
			string providedDocumentId = null;
			Type providedEntityType = null;

			using (new Replacer<DocumentIdToEntityIdConvention>(
				() => EntityConfig.DocumentIdToEntityId,
				newValue: (documentId, documentType, entityType) =>
							    {
										providedDocumentId = documentId;
							      providedEntityType = entityType;
										return "entity1";
							    }))
			{
				var entityConfig = new EntityConfig(typeof(SimpleEntity));

				var returnedEntityId = entityConfig.ConvertDocumentIdToEntityId("doc1");

				Assert.Equal(returnedEntityId, "entity1");
				Assert.Equal("doc1", providedDocumentId);
				Assert.Equal(typeof(SimpleEntity), providedEntityType);
			}
		}

		[Fact]
		public void ShouldThrowOnNullInputToConvertDocumentIdToEntityId()
		{
			var entityConfig = new EntityConfig(typeof(SimpleEntity));

			Assert.Throws<ArgumentNullException>(() => entityConfig.ConvertEntityIdToDocumentId(null));
			Assert.Throws<ArgumentNullException>(() => entityConfig.ConvertEntityIdToDocumentId(string.Empty));
		}

		[Fact]
		public void ShouldDelegateGetIgnoredMembers()
		{
			Type providedEntityType = null;

			var returnedIgnoredMembers = new MemberInfo[0];

			using (new Replacer<GetIgnoredMembersConvention>(
				() => EntityConfig.GetIgnoredMembers,
				newValue: entityType =>
				{
					providedEntityType = entityType;
					return returnedIgnoredMembers;
				}))
			{
				var entityConfig = new EntityConfig(typeof(SimpleEntity));
				
				Assert.Equal(returnedIgnoredMembers, entityConfig.IgnoredMembers);
				Assert.Equal(typeof(SimpleEntity), providedEntityType);
			}
		}

		[Fact]
		public void ShouldReturnEmptyIgnoredMembersOnNullReturnedFromConvention()
		{
			using (new Replacer<GetIgnoredMembersConvention>(
				() => EntityConfig.GetIgnoredMembers, newValue: entityType => null))
			{
				var entityConfig = new EntityConfig(typeof(SimpleEntity));
				Assert.NotNull(entityConfig.IgnoredMembers);
				Assert.Equal(0, entityConfig.IgnoredMembers.Count());
			}
		}

		private class Replacer<T>: IDisposable
		{
			private readonly Action<T> setter;
			private readonly T oldValue;

			public Replacer(Expression<Func<T>> getterExpression, T newValue)
			{
				Monitor.Enter(typeof(EntityConfig));

				var getter = getterExpression.Compile();
				oldValue = getter();

				var member = (MemberExpression)getterExpression.Body;
				var newValueParam = Expression.Parameter(typeof(T), "value");
				var setterExpression = Expression.Lambda<Action<T>>(
					Expression.Assign(member, newValueParam), 
					newValueParam);

				setter = setterExpression.Compile();
				setter(newValue);
			}

			public void Dispose()
			{
				setter(oldValue);
				try
				{
					Monitor.Exit(typeof(EntityConfig));
				}
				catch (SynchronizationLockException) { }
			}

			~Replacer()
			{
				Dispose();
			}
		}
	}
}
