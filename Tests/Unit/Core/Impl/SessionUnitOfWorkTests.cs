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
using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class SessionUnitOfWorkTests
	{
		readonly SessionUnitOfWork unitOfWork = new SessionUnitOfWork(Default.Settings);
		private readonly DbUriConstructor dbUriConstructor = new UriConstructor("http://example.com").Db("testdb");

		[Fact]
		public void ShouldIdicateEmptyIfNoWorkDone() 
		{
			var bulkUpdateBatch = new BulkUpdateBatch(dbUriConstructor);
			Assert.False(unitOfWork.ApplyChanges(bulkUpdateBatch));
			Assert.True(bulkUpdateBatch.IsEmpty);
		}

		[Fact]
		public void ShouldIdicateEmptyEvenIfSeveralEntitiesHaveAttached() 
		{
			unitOfWork.Attach(SimpleEntity.CreateStandard(), markAsUnchanged: true);
			unitOfWork.Attach(Entity.CreateStandard(), markAsUnchanged: true);

			var bulkUpdateBatch = new BulkUpdateBatch(dbUriConstructor);
			Assert.False(unitOfWork.ApplyChanges(bulkUpdateBatch));
			Assert.True(bulkUpdateBatch.IsEmpty);
		}

		[Fact]
		public void ShouldNotIndicateEmptyIfDelete() 
		{
			var entity = SimpleEntity.CreateStandard();
			unitOfWork.Attach(entity);
			unitOfWork.MarkAsRemoved(entity);

			var bulkUpdateBatch = new BulkUpdateBatch(dbUriConstructor);
			Assert.True(unitOfWork.ApplyChanges(bulkUpdateBatch));
			Assert.False(bulkUpdateBatch.IsEmpty);
		}

		[Fact]
		public void ShouldBeEmptyAfterUpdatingRevision()
		{
			var entity = SimpleEntity.CreateStandard();
			unitOfWork.Attach(entity);
			unitOfWork.MarkAsRemoved(entity);
			unitOfWork.AddNew(EntityWithoutRevision.CreateStandard());

			unitOfWork.ApplyChanges(new BulkUpdateBatch(dbUriConstructor)); // Starting saving changes
			unitOfWork.UpdateRevisions(                     // Returned from server after changes have been saved
				new [] {
					new DocumentInfo(SimpleEntity.StandardDocId, "2-cc2c5ab22cfa4a0faad27a0cb9ca7968"), 
					new DocumentInfo(EntityWithoutRevision.StandardDocId, EntityWithoutRevision.StandardRevision)
				}
			);

			var bulkUpdateBatch = new BulkUpdateBatch(dbUriConstructor);
			Assert.False(unitOfWork.ApplyChanges(bulkUpdateBatch));
			Assert.True(bulkUpdateBatch.IsEmpty);
		}

		[Fact]
		public void ShouldBeEmptyAfterUpdatingRevisionForDeletion()
		{
			var entity = SimpleEntity.CreateStandard();
			unitOfWork.Attach(entity, markAsUnchanged: true);
			unitOfWork.MarkAsRemoved(entity);

			unitOfWork.ApplyChanges(new BulkUpdateBatch(dbUriConstructor)); // Starting saving changes
			unitOfWork.UpdateRevisions(                     // Returned from server after changes have been saved
				new[] { new DocumentInfo(SimpleEntity.StandardDocId, "2-cc2c5ab22cfa4a0faad27a0cb9ca7968")});

			var bulkUpdateBatch = new BulkUpdateBatch(dbUriConstructor);
			Assert.False(unitOfWork.ApplyChanges(bulkUpdateBatch));
			Assert.True(bulkUpdateBatch.IsEmpty);
		}
		
		[Fact]
		public void ShouldSaveNewDocuments()
		{
			unitOfWork.AddNew(SimpleEntity.CreateStandardWithoutRevision());

			IDocument savedDoc = null;
			var bulkUpdateUnitOfWorkMock = new Mock<IBulkUpdateBatch>(MockBehavior.Strict);
			bulkUpdateUnitOfWorkMock
				.Setup(u => u.Create(It.IsAny<IDocument>()))
				.Callback<IDocument>(d => { savedDoc = d; });
				
			unitOfWork.ApplyChanges(bulkUpdateUnitOfWorkMock.Object);

			Assert.NotNull(savedDoc);
			Assert.Equal(SimpleEntity.CreateDocumentWithoutRevision(), savedDoc);
		}

		[Fact]
		public void ShouldUpdatePersistedDocumens()
		{
			var entity = SimpleEntity.CreateStandard();
			entity.Age = 24;
			unitOfWork.Attach(entity);

			IDocument savedDoc = null;
			var bulkUpdateUnitOfWorkMock = new Mock<IBulkUpdateBatch>(MockBehavior.Strict);
			bulkUpdateUnitOfWorkMock
				.Setup(u => u.Update(It.IsAny<IDocument>()))
				.Callback<IDocument>(d => { savedDoc = d; });
			
			unitOfWork.ApplyChanges(bulkUpdateUnitOfWorkMock.Object);

			Assert.NotNull(savedDoc);
			var expectedDoc = new {
				_id = SimpleEntity.StandardDocId, _rev = SimpleEntity.StandardRevision, type = SimpleEntity.DocType, age = 24
			}.ToDocument();
			Assert.Equal(expectedDoc, savedDoc);
		}

		[Fact]
		public void ShouldNotUpdateIfPersistedDocumentHaveNotChanged()
		{
			var entity = SimpleEntity.CreateStandard();
			unitOfWork.Attach(entity, markAsUnchanged: true);

			var bulkUpdateUnitOfWorkMock = new Mock<IBulkUpdateBatch>(MockBehavior.Strict);
			unitOfWork.ApplyChanges(bulkUpdateUnitOfWorkMock.Object); // Mock will throw on any call
		}

		[Fact]
		public void ShouldDeleteEntitiesDocument()
		{
			unitOfWork.MarkAsRemoved(SimpleEntity.CreateStandard());

			string deletedDocId = null;
			string deletedDocRevision = null;
			var bulkUpdateUnitOfWorkMock = new Mock<IBulkUpdateBatch>(MockBehavior.Strict);
			bulkUpdateUnitOfWorkMock
				.Setup(u => u.Delete(It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string>(
					(id, rev) => {
						deletedDocId = id;
						deletedDocRevision = rev;
					});

			unitOfWork.ApplyChanges(bulkUpdateUnitOfWorkMock.Object);

			Assert.Equal(SimpleEntity.StandardDocId, deletedDocId);
			Assert.Equal(SimpleEntity.StandardRevision, deletedDocRevision);
		}

		[Fact]
		public void ShouldDeleteDocumentOnlyOnce()
		{
			var entity = SimpleEntity.CreateStandard();
			unitOfWork.MarkAsRemoved(entity);
			unitOfWork.MarkAsRemoved(entity);

			int callTimes = 0;
			var bulkUpdateUnitOfWorkMock = new Mock<IBulkUpdateBatch>(MockBehavior.Strict);
			bulkUpdateUnitOfWorkMock
				.Setup(u => u.Delete(It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string>(
					(id, rev) => {
						callTimes++;
					});

			unitOfWork.ApplyChanges(bulkUpdateUnitOfWorkMock.Object);

			Assert.Equal(1, callTimes);
		}

		[Fact]
		public void ShouldNotCallCreateOrUpdateIfDocumentEntityHaveDeleted()
		{
			var persistedEntity = SimpleEntity.CreateStandard();
			unitOfWork.Attach(persistedEntity);
			var trancientEntity = SimpleEntity.CreateStandardWithoutRevision();
			unitOfWork.AddNew(trancientEntity);

			unitOfWork.MarkAsRemoved(trancientEntity);
			unitOfWork.MarkAsRemoved(persistedEntity);

			var bulkUpdateUnitOfWorkMock = new Mock<IBulkUpdateBatch>(MockBehavior.Strict);
			bulkUpdateUnitOfWorkMock
				.Setup(u => u.Delete(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((id, rev) => { });

			//Should throw if methods other than Delete() have been called
			unitOfWork.ApplyChanges(bulkUpdateUnitOfWorkMock.Object); 
		}

		[Fact]
		public void ShouldThrowIfTransientDocumentEntityAttached()
		{
			Assert.Throws<ArgumentException>(() => unitOfWork.Attach(SimpleEntity.CreateStandardWithoutRevision()));
		}
		
		[Fact]
		public void ShouldThrowIfPersistedDocumentEntityAddedAsNew()
		{
			Assert.Throws<ArgumentException>(() => unitOfWork.AddNew(SimpleEntity.CreateStandard()));
		}

		[Fact]
		public void ShouldReturnCachedEntityByIdAndExactType() 
		{
			var entity = SimpleEntity.CreateStandard();
			unitOfWork.Attach(entity);

			object cachedEntity;
			Assert.True(unitOfWork.TryGetByEntityIdAndType(entity.Id, typeof(SimpleEntity), out cachedEntity));
			Assert.Same(entity, cachedEntity);
		}

		[Fact]
		public void ShouldReturnCachedEntityByIdAndBaseType() 
		{
			var entity = SimpleDerivedEntity.CreateStandard();
			unitOfWork.Attach(entity);

			object cachedEntity;
			Assert.True(unitOfWork.TryGetByEntityIdAndType(entity.Id, typeof (SimpleEntity), out cachedEntity));
			Assert.Same(entity, cachedEntity);
		}

		[Fact]
		public void ShouldAddDocumentEntityToTheCacheIfDoesNotPresent() 
		{
			unitOfWork.UpdateWithDocument(SimpleEntity.CreateDocument());

			object cachedEntity;
			Assert.True(unitOfWork.TryGetByEntityIdAndType(SimpleEntity.StandardId, typeof (SimpleEntity), out cachedEntity));
			Assert.NotNull(cachedEntity);
			Assert.Equal(42, ((SimpleEntity)cachedEntity).Age);
		}

		[Fact]
		public void ShouldNotUpdateEntitesUnderUser()
		{
			var entity = SimpleEntity.CreateStandard();
			entity.Age = 43;
			unitOfWork.Attach(entity);

			unitOfWork.UpdateWithDocument(SimpleEntity.CreateDocument());

			object cachedEntity;
			Assert.True(unitOfWork.TryGetByDocumentId(SimpleEntity.StandardDocId, out cachedEntity));
			Assert.IsType<SimpleEntity>(cachedEntity);
			Assert.NotNull(cachedEntity);
			Assert.Equal(43, ((SimpleEntity)cachedEntity).Age);
		}


		[Fact]
		public void ShouldRetriveCachedEntitesByDocumentId() 
		{
			unitOfWork.UpdateWithDocument(SimpleEntity.CreateDocument());

			object cachedEntity;
			Assert.True(unitOfWork.TryGetByDocumentId(SimpleEntity.StandardDocId, out cachedEntity));
			Assert.IsType<SimpleEntity>(cachedEntity);
			Assert.NotNull(cachedEntity);
			Assert.Equal(42, ((SimpleEntity)cachedEntity).Age);
		}

		[Fact]
		public void ShouldCheckNullArguments() 
		{
			Assert.Throws<ArgumentNullException>(() => unitOfWork.Attach(null));
			Assert.Throws<ArgumentNullException>(() => unitOfWork.AddNew(null));
			Assert.Throws<ArgumentNullException>(() => unitOfWork.MarkAsRemoved(null));
			Assert.Throws<ArgumentNullException>(() => unitOfWork.ApplyChanges(null));
		}

		
		[Theory]
		[InlineData(@"{}")]
		[InlineData(@"{""_id"": ""entity.doc1""}")]
		[InlineData(@"{""_rev"": ""1-1a517022a0c2d4814d51abfedf9bfee7""}")]
		[InlineData(@"{""_id"": ""entity.doc1"", ""_rev"": ""1-1a517022a0c2d4814d51abfedf9bfee7""}")]
		[InlineData(@"{""_id"": ""entity.doc1"", ""type"": ""entity""}")]
		[InlineData(@"{""_rev"": ""1-1a517022a0c2d4814d51abfedf9bfee7"", ""type"": ""entity""}")]
		public void ShouldNotThrowOnIncorrectDocumentUpdate(string documentString)
		{
			IDocument doc = new Document(documentString);
			Assert.DoesNotThrow(() => unitOfWork.UpdateWithDocument(doc));
		}
	}
}
