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
using CouchDude.Core;
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

			DoTest(
				action: session => {
					entityA = session.Load<SimpleEntity>("doc1");
					entityB = session.Load<SimpleEntity>("doc1");
				});

			Assert.Same(entityA, entityB);
		}

		[Fact]
		public void ShouldReturnSameInstancePerformingLoadAfterSave()
		{
			var entity = new SimpleEntity { Id = "doc1", Name = "John Smith" };
			SimpleEntity loadedEntity = null;
			DoTest(
				action: session => {
					session.Save(entity);
					loadedEntity = session.Load<SimpleEntity>("doc1");
				});

			Assert.Same(entity, loadedEntity);
		}

		[Fact]
		public void ShouldThrowOnInvalidTypeDuringCacheLookup()
		{
			var entity = new SimpleEntity { Id = "doc1", Name = "John Smith" };
			Assert.Throws<EntityTypeMismatchException>(() => DoTest(
				action: session => {
					session.Save(entity);
					session.Load<SimpleEntityWithoutRevision>("doc1");
			}));  
		}

		[Fact]
		public void ShouldLoadDataThroughtCouchApi()
		{
			var loadedEntity = DoTest();
			Assert.NotNull(loadedEntity);
			Assert.Equal("doc1", loadedEntity.Id);
			Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", loadedEntity.Revision);
			Assert.Equal("John Smith", loadedEntity.Name);
		}

		[Fact]
		public void ShouldLoadDataCorrectlyIfNoRevisionPropertyFound()
		{
			var couchApi = Mock.Of<ICouchApi>(
				ca => ca.GetDocumentFromDbById(It.IsAny<string>()) ==
					SimpleEntityWithoutRevision.DocWithRevision
				);

			var loadedEntity = new CouchSession(Default.Settings, couchApi).Load<SimpleEntityWithoutRevision>("simpleEntityWithoutRevision");
			Assert.NotNull(loadedEntity);
			Assert.Equal("doc1", loadedEntity.Id);
			Assert.Equal("John Smith", loadedEntity.Name);
		}

		[Fact]
		public void ShouldThrowOnNullOrEmptyId()
		{
			Assert.Throws<ArgumentNullException>(() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Load<SimpleEntity>(null));
			Assert.Throws<ArgumentNullException>(() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Load<SimpleEntity>(""));
		}

		[Fact]
		public void ShouldAskCouchApiUsingPrefixedDocId()
		{
			string requestedId = null;

			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.GetDocumentFromDbById(It.IsAny<string>()))
				.Returns<string>(docId => { 
					requestedId = docId;
					return SimpleEntity.DocWithRevision;
				});
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			session.Load<SimpleEntity>("doc1");

			Assert.Equal(requestedId, "simpleEntity.doc1");
		}

		private SimpleEntity DoTest(
			Func<string, IDocument> apiResponse = null,
			Mock<ICouchApi> couchApiMock = null,
			Action<ISession> action = null,
			string documentId = "doc1")
		{
			return DoTest<SimpleEntity>(apiResponse, couchApiMock, action, documentId);
		}

		private T DoTest<T>(
			Func<string, IDocument> apiResponse = null,
			Mock<ICouchApi> couchApiMock = null,
			Action<ISession> action = null,
			string documentId = "doc1",
			string documentType = "simpleEntity") where T : class
		{
			if (apiResponse == null)
				apiResponse = requestedUrl => 
					new {
						_id = documentType + ".doc1",
						_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
						type = documentType,
						name = "John Smith"
					}.ToDocument();

			if (couchApiMock == null)
			{
				couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
				couchApiMock
					.Setup(ca => ca.GetDocumentFromDbById(It.IsAny<string>()))
					.Returns(apiResponse);
				couchApiMock
					.Setup(ca => ca.SaveDocumentToDb(It.IsAny<IDocument>()))
					.Returns(new { id = documentId, rev = "42-1a517022a0c2d4814d51abfedf9bfee7" }.ToJsonFragment());
			}

			var session = new CouchSession(Default.Settings, couchApiMock.Object);

			if (action == null)
				return session.Load<T>(documentId);
			else
			{
				action(session);
				return default(T);
			}
		}
	}
}