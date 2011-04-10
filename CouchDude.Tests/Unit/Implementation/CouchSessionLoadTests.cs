using System;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using CouchDude.Tests.SampleData;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchSessionLoadTests
	{
		public class TestEntity
		{
			public string Id { get; set; }
			public string Revision { get; set; }
			public string Name { get; set; }
		}

		public class TestEntity2
		{
			public string Id { get; protected set; }
			public string Name { get; set; }
		}

		private readonly Settings settings =
			new Settings(new Uri("http://example.com"), "temp");

		[Fact]
		public void ShouldReturnNullIfApiReturnedNull()
		{
			var api = new Mock<ICouchApi>();
			api.Setup(ca => ca.GetDocumentFromDbById(It.IsAny<string>())).Returns((JObject) null);
			var session = new CouchSession(settings, api.Object);
			Assert.Null(session.Load<SimpleEntity>("doc1"));
		}

		[Fact]
		public void ShouldReturnSameInstanceWhenRequestingTwice()
		{
			TestEntity entityA = null;
			TestEntity entityB = null;

			DoTest(
				action: session => {
					entityA = session.Load<TestEntity>("doc1");
					entityB = session.Load<TestEntity>("doc1");
				});

			Assert.Same(entityA, entityB);
		}

		[Fact]
		public void ShouldReturnSameInstancePerformingLoadAfterSave()
		{
			var entity = new TestEntity { Id = "doc1", Name = "John Smith" };
			TestEntity loadedEntity = null;
			DoTest(
				action: session => {
					session.Save(entity);
					loadedEntity = session.Load<TestEntity>("doc1");
				});

			Assert.Same(entity, loadedEntity);
		}

		[Fact]
		public void ShouldThrowOnInvalidTypeDuringCacheLookup()
		{
			var entity = new TestEntity { Id = "doc1", Name = "John Smith" };
			Assert.Throws<EntityTypeMismatchException>(() => DoTest(
				action: session => {
					session.Save(entity);
					session.Load<TestEntity2>("doc1");
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
			var loadedEntity = DoTest<TestEntity2>(documentType: "testEntity2");
			Assert.NotNull(loadedEntity);
			Assert.Equal("doc1", loadedEntity.Id);
			Assert.Equal("John Smith", loadedEntity.Name);
		}

		[Fact]
		public void ShouldThrowOnNullOrEmptyId()
		{
			Assert.Throws<ArgumentNullException>(() => DoTest(documentId: null));
			Assert.Throws<ArgumentNullException>(() => DoTest(documentId: ""));
		}

		private TestEntity DoTest(
			Func<string, JObject> apiResponse = null,
			Mock<ICouchApi> couchApiMock = null,
			Action<ISession> action = null,
			string documentId = "doc1")
		{
			return DoTest<TestEntity>(apiResponse, couchApiMock, action, documentId);
		}

		private T DoTest<T>(
			Func<string, JObject> apiResponse = null,
			Mock<ICouchApi> couchApiMock = null,
			Action<ISession> action = null,
			string documentId = "doc1",
			string documentType = "testEntity") where T: class
		{
			if (apiResponse == null)
				apiResponse = requestedUrl => new {
					_id = "doc1",
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

			var session = new CouchSession(settings, couchApiMock.Object);

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