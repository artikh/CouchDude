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
		[Fact]
		public void ShouldInvokeCouchApiForDeletion()
		{
			var entity = SimpleEntity.CreateStd();
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
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var docInfo = session.Delete(entity);

			Assert.Equal(SimpleEntity.StandardDocId, deletedId);
			Assert.Equal(entity.Revision, deletedRevision);

			Assert.Equal(entity.Id, docInfo.Id);
			Assert.Equal("2-1a517022a0c2d4814d51abfedf9bfee7", docInfo.Revision);
		}

		[Fact]
		public void ShouldThrowOnNullEntity()
		{
			Assert.Throws<ArgumentNullException>(
				() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Delete<SimpleEntity>(entity: null));
		}

		[Fact]
		public void ShouldThrowArgumentExceptionIfNoRevisionOnNewEntity()
		{
			Assert.Throws<ArgumentException>(
				() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Delete(entity: SimpleEntity.CreateStdWithoutRevision())
			);
		}

		[Fact]
		public void ShouldNotThrowArgumentExceptionIfNoRevisionOnLoadedEntity()
		{
			string deletedId = null;
			string deletedRev = null;

			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(ca => ca.GetDocumentFromDbById(It.IsAny<string>()))
				.Returns(SimpleEntityWithoutRevision.DocumentWithRevision);
			couchApi
				.Setup(ca => ca.DeleteDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns((string id, string rev) => {
					deletedId = id;
					deletedRev = rev;
					return SimpleEntityWithoutRevision.OkResponse;
				});

			var session = new CouchSession(Default.Settings, couchApi.Object);

			Assert.DoesNotThrow(() => {
				var entity = session.Load<SimpleEntityWithoutRevision>(SimpleEntityWithoutRevision.StandardEntityId);
				session.Delete(entity: entity);
			});

			Assert.Equal(SimpleEntityWithoutRevision.StandardDocId, deletedId);
			Assert.Equal(SimpleEntityWithoutRevision.StandardRevision, deletedRev);
		}
	}
}