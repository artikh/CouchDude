using System;
using System.Collections.Generic;
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
		private readonly Settings settings = new Settings(new Uri("http://example.com"), "temp");

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
									Key = SimpleEntity.StandardId,
									Value = new JArray("rev", SimpleEntity.StandardRevision),
									DocumentId = SimpleEntity.StandardId,
									Document = SimpleEntity.DocumentWithRevision
								}
							}
						};
					});

			var session = new CouchSession(settings, couchApiMock.Object);
			session.GetAll<SimpleEntity>().ToList();

			Assert.NotNull(sendQuery);
			Assert.Null(sendQuery.DesignDocumentName);
			Assert.Equal("_all_docs", sendQuery.ViewName);
			Assert.True(sendQuery.IncludeDocs);
		}
	
		[Fact]
		public void ShouldBindDocumentsCorrectly()
		{
			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.Query(It.IsAny<ViewQuery>()))
				.Returns(
					(ViewQuery query) => new ViewResult {
						Query = query,
						TotalRows = 1,
						Rows = {
							new ViewResultRow {
								Key = SimpleEntity.StandardId,
								Value = new JArray("rev", SimpleEntity.StandardRevision),
								DocumentId = SimpleEntity.StandardId,
								Document = SimpleEntity.DocumentWithRevision
							}
						}
					});

			var session = new CouchSession(settings, couchApiMock.Object);
			var loadedEntities = session.GetAll<SimpleEntity>().ToList();

			Assert.Equal(1, loadedEntities.Count);
			Assert.NotNull(loadedEntities[0]);
			Assert.Equal(SimpleEntity.StandardId, loadedEntities[0].Id);
			Assert.Equal(SimpleEntity.StandardRevision, loadedEntities[0].Revision);
			Assert.Equal(SimpleEntity.WithRevision.Age, loadedEntities[0].Age);
			Assert.Equal(SimpleEntity.WithRevision.Date, loadedEntities[0].Date);
			Assert.Equal(SimpleEntity.WithRevision.Name, loadedEntities[0].Name);
		}
	}
}