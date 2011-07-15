using System;
using System.Linq;
using CouchDude.Core;
using CouchDude.Core.Api;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Impl
{
	public class CouchSessionQueryTests
	{

		[Fact]
		public void ShouldQueryCochApiWithSameQueryObject()
		{
			ViewQuery sendQuery = null;

			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.Query(It.IsAny<ViewQuery>()))
				.Returns(
					(ViewQuery query) =>
					{
						sendQuery = query;
						return new ViewResult
						{
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
			var viewQuery = new ViewQuery<SimpleEntity> { ViewName = "_all_docs", IncludeDocs = true };
			session.Query(viewQuery);

			Assert.Same(sendQuery, viewQuery);
		}

		[Fact]
		public void ShouldBindDocumentsCorrectly()
		{
			var entity = SimpleEntity.CreateStd();

			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.Query(It.IsAny<ViewQuery>()))
				.Returns(
					(ViewQuery query) => new ViewResult
					{
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
			var queryResult = session.Query(new ViewQuery<SimpleEntity> { ViewName = "_all_docs", IncludeDocs = true });

			var firstRow = queryResult.First();
			Assert.NotNull(firstRow);
			Assert.Equal(SimpleEntity.StandardEntityId, firstRow.Id);
			Assert.Equal(SimpleEntity.StandardRevision, firstRow.Revision);
			Assert.Equal(entity.Age, firstRow.Age);
			Assert.Equal(entity.Date, firstRow.Date);
			Assert.Equal(entity.Name, firstRow.Name);
		}

		[Fact]
		public void ShouldThrowArgumentExceptioIfNoIncludeDocsOptionAndEntityTypeParameter()
		{
			var session = new CouchSession(Default.Settings, Mock.Of<ICouchApi>());
			Assert.Throws<ArgumentException>(() => session.Query(new ViewQuery<SimpleEntity> { ViewName = "_all_docs" }));
		}

		[Fact]
		public void ShouldThrowOnNullQuery()
		{
			Assert.Throws<ArgumentNullException>(
				() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Query<SimpleEntity>(query: null));
		}

		[Fact]
		public void ShouldMapEntitiesIfTypeIsCompatible()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					q =>
					new ViewResult
						{
							TotalRows = 1,
							Query = q,
							Rows =
								{
									new ViewResultRow
										{
											Key = new object[] {"key1", 0}.ToJToken(),
											Value = new
											        	{
											        		Title = "Object title",
											        		Subject = "some"
											        	}.ToJToken(),
											Document = SimpleEntity.DocumentWithRevision,
											DocumentId = SimpleEntity.StandardDocId
										},
								}
						});
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.Query(new ViewQuery<SimpleEntity> {IncludeDocs = true});

			Assert.NotNull(queryResult);
			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.TotalRowCount);

			var row = queryResult.First();
			Assert.NotNull(row);
			Assert.Equal(SimpleEntity.StandardEntityId, row.Id);
			Assert.Equal(SimpleEntity.StandardRevision, row.Revision);
		}

		[Fact]
		public void ShouldCacheInstanceFromQuery()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					q =>
					new ViewResult
						{
							TotalRows = 1,
							Query = q,
							Rows =
								{
									new ViewResultRow
										{
											Key = new object[] {"key1", 0}.ToJToken(),
											Value = null,
											Document = SimpleEntity.DocumentWithRevision,
											DocumentId = SimpleEntity.StandardDocId
										},
								}
						});
			var session = new CouchSession(Default.Settings, couchApi.Object);

			var queriedEntity = session.Query(new ViewQuery<SimpleEntity> { IncludeDocs = true }).First();
			var loadedEntity = session.Load<SimpleEntity>(SimpleEntity.StandardEntityId);
			Assert.Same(queriedEntity, loadedEntity);
		}

		[Fact]
		public void ShouldNotFailIfOnNullDocumentRowsFromCouchDb()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					q =>
					new ViewResult
						{
							TotalRows = 1,
							Query = q,
							Rows =
								{
									new ViewResultRow
										{
											Key = new object[] {"key1", 0}.ToJToken(),
											Value = null,
											Document = null,
											DocumentId = null
										},
								}
						});
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.Query(new ViewQuery<SimpleEntity> {IncludeDocs = true});

			Assert.Equal(0, queryResult.RowCount);
			Assert.Equal(0, queryResult.Count());
		}

		[Fact]
		public void ShouldNotFailIfOnNullValueRowsFromCouchDB()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					q =>
					new ViewResult
						{
							TotalRows = 1,
							Query = q,
							Rows =
								{
									new ViewResultRow
										{
											Key = new object[] {"key1", 0}.ToJToken(),
											Value = null,
											Document = null,
											DocumentId = null
										},
								}
						});
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.Query(new ViewQuery<SimpleViewData>());

			Assert.Equal(0, queryResult.RowCount);
			Assert.Equal(0, queryResult.Count());
		}

		[Fact]
		public void ShouldMapViewdataIfTypeIsCompatible()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					q =>
					new ViewResult
						{
							TotalRows = 1,
							Query = q,
							Rows =
								{
									new ViewResultRow
										{
											Key = new object[] {"key1", 0}.ToJToken(),
											Value = new
											        	{
											        		Title = "Object title",
											        		Subject = "some"
											        	}.ToJToken(),
											Document = SimpleEntity.DocumentWithRevision,
											DocumentId = SimpleEntity.StandardDocId
										},
								}
						});
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.Query(new ViewQuery<SimpleViewData>());

			Assert.NotNull(queryResult);
			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.TotalRowCount);

			var row = queryResult.First();
			Assert.NotNull(row);
			Assert.Equal("Object title", row.Title);
		}
	}
}