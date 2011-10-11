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
using System.Threading.Tasks;

using CouchDude.Api;
using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class CouchSessionLoadTests
	{
		[Fact]
		public void ShouldReturnSameInstanceWhenRequestingTwice()
		{
			var session = new CouchSession(Default.Settings, MockCouchApi());
			var entityA = session.Synchronously.Load<Entity>("doc1");
			var entityB = session.Synchronously.Load<Entity>("doc1");
			Assert.Same(entityA, entityB);
		}

		[Fact]
		public void ShouldReturnSameInstancePerformingLoadAfterSave()
		{
			var entity = new Entity { Id = "doc1", Name = "John Smith" };
			var session = new CouchSession(Default.Settings, MockCouchApi());
			session.Save(entity);
			var loadedEntity = session.Synchronously.Load<Entity>("doc1");

			Assert.Same(entity, loadedEntity);
		}
		
		[Fact]
		public void ShouldLoadDataThroughtCouchApi()
		{
			var dbApiMock = new Mock<IDatabaseApi>(MockBehavior.Loose);
			dbApiMock
				.Setup(ca => ca.RequestDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(
					(string id, string revision) => Entity.CreateDocWithRevision().ToTask());
			dbApiMock
				.Setup(ca => ca.SaveDocument(It.IsAny<IDocument>()))
				.Returns(Entity.StandardDococumentInfo.ToTask());
			dbApiMock
				.Setup(ca => ca.Synchronously).Returns(() => new SynchronousDatabaseApi(dbApiMock.Object));

			var session = new CouchSession(Default.Settings, MockCouchApi(dbApiMock.Object));
			var loadedEntity = session.Synchronously.Load<Entity>(Entity.StandardEntityId);
			Assert.NotNull(loadedEntity);
			Assert.Equal(Entity.StandardEntityId, loadedEntity.Id);
			Assert.Equal(Entity.StandardRevision, loadedEntity.Revision);
			Assert.Equal("John Smith", loadedEntity.Name);
		}

		[Fact]
		public void ShouldLoadDataCorrectlyIfNoRevisionPropertyFound()
		{
			var couchApi = Mock.Of<ICouchApi>(
				c => c.Db("testdb") ==  Mock.Of<IDatabaseApi>(
					api => api.RequestDocument(It.IsAny<string>(), It.IsAny<string>()) == 
						EntityWithoutRevision.CreateDocumentWithRevision().ToTask()
			));

			var loadedEntity = new CouchSession(Default.Settings, couchApi).Synchronously.Load<EntityWithoutRevision>("doc1");
			Assert.NotNull(loadedEntity);
			Assert.Equal("doc1", loadedEntity.Id);
			Assert.Equal("John Smith", loadedEntity.Name);
		}

		[Fact]
		public void ShouldThrowOnNullOrEmptyId()
		{
			Assert.Throws<ArgumentNullException>(
				() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Synchronously.Load<Entity>(null));
			Assert.Throws<ArgumentNullException>(
				() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Synchronously.Load<Entity>(""));
		}

		[Fact]
		public void ShouldRequestCouchApiUsingDocumentIdRatherEntityId()
		{
			string requestedId = null;

			var dbApiMock = new Mock<IDatabaseApi>(MockBehavior.Loose);
			dbApiMock
				.Setup(ca => ca.RequestDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(
					(string id, string revision) => {
						requestedId = id;
						return Entity.CreateDocWithRevision().ToTask();
					});
			var session = new CouchSession(Default.Settings, MockCouchApi(dbApiMock));
			session.Synchronously.Load<Entity>(Entity.StandardEntityId);

			Assert.Equal(Entity.StandardDocId, requestedId);
		}

		static ICouchApi MockCouchApi()
		{
			var databaseApiMock = new Mock<IDatabaseApi>(MockBehavior.Loose);
			databaseApiMock
				.Setup(ca => ca.RequestDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(
					(string id, string revision) => Task.Factory.StartNew(
						() => new {
							_id = "entity" + ".doc1",
							_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
							type = "entity",
							name = "John Smith"
						}.ToDocument()));
			databaseApiMock
				.Setup(ca => ca.SaveDocument(It.IsAny<IDocument>()))
				.Returns(new DocumentInfo(Entity.StandardDocId, "42-1a517022a0c2d4814d51abfedf9bfee7").ToTask());
			databaseApiMock
				.Setup(ca => ca.Synchronously).Returns(() => new SynchronousDatabaseApi(databaseApiMock.Object));
			
			return MockCouchApi(databaseApiMock);
		}

		private static ICouchApi MockCouchApi(Mock<IDatabaseApi> databaseApiMock) { return MockCouchApi(databaseApiMock.Object); }

		private static ICouchApi MockCouchApi(IDatabaseApi databaseApi)
		{
			return Mock.Of<ICouchApi>(ca => ca.Db("testdb") == databaseApi);
		}
	}
}