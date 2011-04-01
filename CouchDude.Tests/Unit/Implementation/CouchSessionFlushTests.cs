using System;

using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

using CouchDude.Core;
using CouchDude.Core.Implementation;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchSessionFlushTests
	{
		public class TestEntity
		{
			[JsonIgnore]
			public string Id { get; set; }

			[JsonIgnore]
			public string Revision { get; set; }

			public string Name { get; set; }
		}

		private readonly Settings settings =
			new Settings(new Uri("http://example.com"), "temp");

		private readonly TestEntity entity = new TestEntity
		{
			Id = "doc1",
			Name = "John Smith"
		};


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
			
			var session = new CouchSession(settings, couchApiMock.Object);
			session.Save(entity);
			entity.Name = "Artem Tikhomirov";
			session.Flush();

			Assert.Equal(1, totalUpdateCount);
			Assert.Equal(entity.Id, updatedDocId);
			Utils.AssertSameJson(
				new {
					_id = "doc1",
					_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
					type = "testEntity",
					name = "Artem Tikhomirov"
				},
				updatedDoc
			);
		}
	}
}
