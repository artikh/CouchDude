using CouchDude.Core.Api;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;

namespace CouchDude.Tests.Unit.Impl
{
	public class CouchSessionFlushTests
	{
		private readonly SimpleEntity entity = SimpleEntity.CreateStdWithoutRevision();

		[Fact]
		public void ShouldUpdateChangedDocumentsOnFlush()
		{
			string updatedDocId = null;
			JObject updatedDoc = null;
			var totalUpdateCount = 0;

			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.UpdateDocumentInDb(It.IsAny<string>(), It.IsAny<JObject>()))
				.Returns((string docId, JObject doc) => {
					updatedDocId = docId;
					updatedDoc = doc;
					totalUpdateCount++;
					return new {
						ok = true,
						id = entity.Id,
						rev = "2-1a517022a0c2d4814d51abfedf9bfee7"
					}.ToJObject();
				});
			couchApiMock
				.Setup(ca => ca.SaveDocumentToDb(It.IsAny<string>(), It.IsAny<JObject>()))
				.Returns(new {
					ok = true,
					id = entity.Id,
					rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
				}.ToJObject());
			
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			session.Save(entity);
			entity.Name = "Artem Tikhomirov";
			session.Flush();

			Assert.Equal(1, totalUpdateCount);
			Assert.Equal("simpleEntity.doc1", updatedDocId);
			TestUtils.AssertSameJson(
				new
				{
					_id = "simpleEntity.doc1",
					_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
					type = "simpleEntity",
					name = "Artem Tikhomirov",
					age = 42,
					date = "1957-04-10T00:00:00"
				},
				updatedDoc
			);
		}
	}
}

