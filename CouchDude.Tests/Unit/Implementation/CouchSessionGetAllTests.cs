using System.Linq;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using CouchDude.Tests.SampleData;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchSessionGetAllTests
	{
		[Fact]
		public void ShouldQuerySpecialAllDocumentsView()
		{
			ViewQuery sendQuery = null;

			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.Query(It.IsAny<ViewQuery>()))
				.Returns(
					(ViewQuery query) => {
						sendQuery = query;
						return new ViewResult {
							Query = query,
							TotalRows = 1,
							Rows = {
								new ViewResultRow {
									Key = SimpleEntity.StandardEntityId,
									Value = new JArray("rev", SimpleEntity.StandardRevision),
									DocumentId = SimpleEntity.StandardEntityId,
									Document = SimpleEntity.DocumentWithRevision
								}
							}
						};
					});

			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			session.GetAll<SimpleEntity>().ToList();

			Assert.NotNull(sendQuery);
			Assert.Null(sendQuery.DesignDocumentName);
			Assert.Equal("_all_docs", sendQuery.ViewName);
			Assert.True(sendQuery.IncludeDocs);
		}
	
		[Fact]
		public void ShouldBindDocumentsCorrectly()
		{
			var entity = SimpleEntity.CreateStd();


			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.Query(It.IsAny<ViewQuery>()))
				.Returns(
					(ViewQuery query) => new ViewResult {
						Query = query,
						TotalRows = 1,
						Rows = {
							new ViewResultRow {
								Key = SimpleEntity.StandardEntityId,
								Value = new JArray("rev", SimpleEntity.StandardRevision),
								DocumentId = SimpleEntity.StandardEntityId,
								Document = SimpleEntity.DocumentWithRevision
							}
						}
					});

			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			var loadedEntities = session.GetAll<SimpleEntity>().ToList();

			Assert.Equal(1, loadedEntities.Count);
			Assert.NotNull(loadedEntities[0]);
			Assert.Equal(SimpleEntity.StandardEntityId, loadedEntities[0].Id);
			Assert.Equal(SimpleEntity.StandardRevision, loadedEntities[0].Revision);
			Assert.Equal(entity.Age, loadedEntities[0].Age);
			Assert.Equal(entity.Date, loadedEntities[0].Date);
			Assert.Equal(entity.Name, loadedEntities[0].Name);
		}
	}
}