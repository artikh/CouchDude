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
using System.Threading.Tasks;
using CouchDude.Core;
using CouchDude.Core.Api;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Impl
{
	public class CouchSessionLoadTests
	{
		[Fact]
		public void ShouldReturnSameInstanceWhenRequestingTwice()
		{
			SimpleEntity entityA = null;
			SimpleEntity entityB = null;

			var session = new CouchSession(Default.Settings, MockCouchApi());
			entityA = session.Synchronously.Load<SimpleEntity>("doc1");
			entityB = session.Synchronously.Load<SimpleEntity>("doc1");
			Assert.Same(entityA, entityB);
		}

		[Fact]
		public void ShouldReturnSameInstancePerformingLoadAfterSave()
		{
			var entity = new SimpleEntity { Id = "doc1", Name = "John Smith" };
			SimpleEntity loadedEntity = null;
			var session = new CouchSession(Default.Settings, MockCouchApi());
			session.Save(entity);
			loadedEntity = session.Synchronously.Load<SimpleEntity>("doc1");

			Assert.Same(entity, loadedEntity);
		}

		[Fact]
		public void ShouldThrowOnInvalidTypeDuringCacheLookup()
		{
			var entity = new SimpleEntity { Id = "doc1", Name = "John Smith" };
			var session = new CouchSession(Default.Settings, MockCouchApi());
			Assert.Throws<EntityTypeMismatchException>(() => {
					session.Save(entity);
					session.Synchronously.Load<SimpleEntityWithoutRevision>("doc1");
				}
			);  
		}

		[Fact]
		public void ShouldLoadDataThroughtCouchApi()
		{
			var session = new CouchSession(Default.Settings, MockCouchApi());
			var loadedEntity = session.Synchronously.Load<SimpleEntity>(SimpleEntity.StandardEntityId);
			Assert.NotNull(loadedEntity);
			Assert.Equal("doc1", loadedEntity.Id);
			Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", loadedEntity.Revision);
			Assert.Equal("John Smith", loadedEntity.Name);
		}

		[Fact]
		public void ShouldLoadDataCorrectlyIfNoRevisionPropertyFound()
		{
			var couchApi = Mock.Of<ICouchApi>(
				api => api.RequestDocumentById(It.IsAny<string>()) == SimpleEntityWithoutRevision.DocWithRevision.ToTask()
			);

			var loadedEntity = new CouchSession(Default.Settings, couchApi).Synchronously.Load<SimpleEntityWithoutRevision>("simpleEntityWithoutRevision");
			Assert.NotNull(loadedEntity);
			Assert.Equal("doc1", loadedEntity.Id);
			Assert.Equal("John Smith", loadedEntity.Name);
		}

		[Fact]
		public void ShouldThrowOnNullOrEmptyId()
		{
			Assert.Throws<ArgumentNullException>(() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Synchronously.Load<SimpleEntity>(null));
			Assert.Throws<ArgumentNullException>(() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Synchronously.Load<SimpleEntity>(""));
		}

		[Fact]
		public void ShouldAskCouchApiUsingPrefixedDocId()
		{
			string requestedId = null;

			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.RequestDocumentById(It.IsAny<string>()))
				.Returns<string>(docId => { 
					requestedId = docId;
					return SimpleEntity.DocWithRevision.ToTask<IDocument>();
				});
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			session.Synchronously.Load<SimpleEntity>("doc1");

			Assert.Equal(requestedId, "simpleEntity.doc1");
		}

		static ICouchApi MockCouchApi()
		{
			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.RequestDocumentById(It.IsAny<string>()))
				.Returns<string>(id => Task.Factory.StartNew(
					() => new {
						_id = "simpleEntity" + ".doc1",
						_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
						type = "simpleEntity",
						name = "John Smith"
					}.ToDocument()));
			couchApiMock
				.Setup(ca => ca.SaveDocument(It.IsAny<IDocument>()))
				.Returns(new { id = SimpleEntity.StandardDocId, rev = "42-1a517022a0c2d4814d51abfedf9bfee7" }.ToJsonFragment().ToTask());
			couchApiMock
				.Setup(ca => ca.Synchronously).Returns(() => new SynchronousCouchApi(couchApiMock.Object));
			return couchApiMock.Object;
		}
	}
}