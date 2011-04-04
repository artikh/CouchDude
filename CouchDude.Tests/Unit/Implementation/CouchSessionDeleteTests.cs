using System;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchSessionDeleteTests
	{
		private readonly Settings settings =
			new Settings(new Uri("http://example.com"), "temp");

		[Fact]
		public void ShouldInvokeCouchApiForDeletion()
		{
			var entity = SimpleEntity.WithRevision;
			string deletedId = null;
			string deletedRevision = null;

			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(ca => ca.DeleteDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(
					(string id, string revision) => {
						deletedId = id;
						deletedRevision = revision;
						return new {ok = true, id, rev = "2-1a517022a0c2d4814d51abfedf9bfee7"}.ToJObject();
					});
			var session = new CouchSession(settings, couchApi.Object);
			var docInfo = session.Delete(entity);
			
			Assert.Equal(entity.Id, deletedId);
			Assert.Equal(entity.Revision, deletedRevision);
			Assert.Equal(entity.Id, docInfo.Id);
			Assert.Equal("2-1a517022a0c2d4814d51abfedf9bfee7", docInfo.Revision);
		}

		[Fact]
		public void ShouldThrowOnNullEntity()
		{
			Assert.Throws<ArgumentNullException>(
				() => new CouchSession(settings, Mock.Of<ICouchApi>()).Delete<SimpleEntity>(entity: null));
		}

		[Fact]
		public void ShouldThrowArgumentExceptionIfNoRevisionOnNewEntity()
		{
			Assert.Throws<ArgumentException>(
				() => new CouchSession(settings, Mock.Of<ICouchApi>()).Delete(entity: SimpleEntity.WithoutRevision)
			);
		}

		[Fact]
		public void ShouldNotThrowArgumentExceptionIfNoRevisionOnLoadedEntity()
		{
			const string standardId = SimpleEntityWithoutRevision.StandardId;
			const string standardRevision = SimpleEntityWithoutRevision.StandardRevision;

			string deletedId = null;
			string deletedRev = null;

			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(ca => ca.GetDocumentFromDbById(standardId))
				.Returns(SimpleEntityWithoutRevision.DocumentWithRevision);
			couchApi
				.Setup(ca => ca.DeleteDocument(standardId, standardRevision))
				.Returns((string id, string rev) => {
					deletedId = id;
					deletedRev = rev;
					return SimpleEntityWithoutRevision.OkResponse;
				});

			var session = new CouchSession(settings, couchApi.Object);

			Assert.DoesNotThrow(() => {
				var entity = session.Load<SimpleEntityWithoutRevision>(standardId);
				session.Delete(entity: entity);
			});

			Assert.Equal(standardId, deletedId);
			Assert.Equal(standardRevision, deletedRev);
		}
	}
}