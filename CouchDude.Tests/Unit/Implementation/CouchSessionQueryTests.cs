using System.Linq;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using CouchDude.Tests.SampleData;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchSessionQueryTests
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
			session.Query(new ViewQuery<SimpleEntity> { ViewName = "_all_docs" });

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
			var queryResult = session.Query(new ViewQuery<SimpleEntity> { ViewName = "_all_docs" });

			var firstRow = queryResult.Rows.First();
			Assert.NotNull(firstRow);
			Assert.Equal(SimpleEntity.StandardEntityId, firstRow.Id);
			Assert.Equal(SimpleEntity.StandardRevision, firstRow.Revision);
			Assert.Equal(entity.Age, firstRow.Age);
			Assert.Equal(entity.Date, firstRow.Date);
			Assert.Equal(entity.Name, firstRow.Name);
		}
	}
}