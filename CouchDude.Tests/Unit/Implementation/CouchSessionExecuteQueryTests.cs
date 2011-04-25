using System;
using System.Linq;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchSessionExecuteQueryTests
	{
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
				.Returns<ViewQuery>(q =>
					new ViewResult {
						TotalRows = 1,
						Query = q,
						Rows = {
								new ViewResultRow {
									Key = new object[] { "key1", 0 }.ToJToken(),
									Value = new {
										Title = "Object title",
        						Subject = "some"
									}.ToJToken(),
									Document = SimpleEntity.DocumentWithRevision,
									DocumentId = SimpleEntity.StandardDocId
								},
							}
						});
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.Query(new ViewQuery<SimpleEntity>());

			Assert.NotNull(queryResult);
			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.TotalRowCount);

			var row = queryResult.Rows.First();
			Assert.NotNull(row);
			Assert.Equal(SimpleEntity.StandardEntityId, row.Id);
			Assert.Equal(SimpleEntity.StandardRevision, row.Revision);
		}

		[Fact]
		public void ShouldCacheInstaneceFromQuery()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(q =>
					new ViewResult {
						TotalRows = 1,
						Query = q,
						Rows = {
								new ViewResultRow {
									Key = new object[] { "key1", 0 }.ToJToken(),
									Value = null,
									Document = SimpleEntity.DocumentWithRevision,
									DocumentId = SimpleEntity.StandardDocId
								},
							}
						});
			var session = new CouchSession(Default.Settings, couchApi.Object);
			
			var queriedEntity = session.Query(new ViewQuery<SimpleEntity>()).Rows.First();
			var loadedEntity = session.Load<SimpleEntity>(SimpleEntity.StandardEntityId);
			Assert.Same(queriedEntity, loadedEntity);
		}

		[Fact]
		public void ShouldNotFailIfOnNullDocumentRowsFromCouchDB()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(q =>
					new ViewResult {
						TotalRows = 1,
						Query = q,
						Rows = {
							new ViewResultRow {
								Key = new object[] { "key1", 0 }.ToJToken(),
								Value = null,
								Document = null,
								DocumentId = null
							},
						}
					});
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.Query(new ViewQuery<SimpleEntity>());

			Assert.Equal(0, queryResult.RowCount);
			Assert.Equal(0, queryResult.Rows.Count());
		}

		[Fact]
		public void ShouldNotFailIfOnNullValueRowsFromCouchDB()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(q =>
					new ViewResult {
						TotalRows = 1,
						Query = q,
						Rows = {
								new ViewResultRow {
									Key = new object[] { "key1", 0 }.ToJToken(),
									Value = null,
									Document = null,
									DocumentId = null
								},
							}
					});
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.Query(new ViewQuery<SimpleViewData>());

			Assert.Equal(0, queryResult.RowCount);
			Assert.Equal(0, queryResult.Rows.Count());
		}

		[Fact]
		public void ShouldMapViewdataIfTypeIsCompatible()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(q =>
					new ViewResult {
						TotalRows = 1,
						Query = q,
						Rows = {
							new ViewResultRow {
									Key = new object[] {"key1", 0}.ToJToken(),
									Value = new {
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

			var row = queryResult.Rows.First();
			Assert.NotNull(row);
			Assert.Equal("Object title", row.Title);
		}
	}
}