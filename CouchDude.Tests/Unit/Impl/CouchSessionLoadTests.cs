using System;
using CouchDude.Core;
using CouchDude.Core.Api;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Newtonsoft.Json.Linq;
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
					SimpleEntityWithoutRevision.DocumentWithRevision
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
				  return SimpleEntity.DocumentWithRevision;
				});
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			session.Load<SimpleEntity>("doc1");

			Assert.Equal(requestedId, "simpleEntity.doc1");
		}

		private SimpleEntity DoTest(
			Func<string, JObject> apiResponse = null,
			Mock<ICouchApi> couchApiMock = null,
			Action<ISession> action = null,
			string documentId = "doc1")
		{
			return DoTest<SimpleEntity>(apiResponse, couchApiMock, action, documentId);
		}

		private T DoTest<T>(
			Func<string, JObject> apiResponse = null,
			Mock<ICouchApi> couchApiMock = null,
			Action<ISession> action = null,
			string documentId = "doc1",
			string documentType = "simpleEntity") where T : class
		{
			if (apiResponse == null)
				apiResponse = requestedUrl => new
				                              	{
				                              		_id = documentType + ".doc1",
				                              		_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
				                              		type = documentType,
				                              		name = "John Smith"
				                              	}.ToJObject();

			if (couchApiMock == null)
			{
				couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
				couchApiMock
					.Setup(ca => ca.GetDocumentFromDbById(It.IsAny<string>()))
					.Returns(apiResponse);
				couchApiMock
					.Setup(ca => ca.SaveDocumentToDb(It.IsAny<string>(), It.IsAny<JObject>()))
					.Returns(new {id = documentId, rev = "42-1a517022a0c2d4814d51abfedf9bfee7"}.ToJObject());
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