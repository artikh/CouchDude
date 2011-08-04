#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov																					
																																					
	Licensed under the Apache License, Version 2.0 (the "License");					
	you may not use this file except in compliance with the License.					
	You may obtain a copy of the License at																	
																																					
	    http://www.apache.org/licenses/LICENSE-2.0														
																																					
	Unless required by applicable law or agreed to in writing, software			
	distributed under the License is distributed on an "AS IS" BASIS,				
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
	See the License for the specific language governing permissions and			
	limitations under the License.																						
*/
#endregion

using System;
using System.Linq;
using CouchDude.Core;
using CouchDude.Core.Api;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Moq;
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
				.Setup(ca => ca.QueryAndWaitForResult(It.IsAny<ViewQuery>()))
				.Returns(
					(ViewQuery query) =>
					{
						sendQuery = query;
						return new ViewResult(
							new []{
								new ViewResultRow(
									SimpleEntity.StandardEntityId.ToJsonFragment(), 
									new { rev = SimpleEntity.StandardRevision}.ToJsonFragment(),
									SimpleEntity.StandardEntityId,
									SimpleEntity.DocWithRevision
								)
							},
							1,
							offset: 0,
							query: query);
					});

			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			var viewQuery = new ViewQuery<SimpleEntity> { ViewName = "_all_docs", IncludeDocs = true };
			session.QuerySync(viewQuery);

			Assert.Same(sendQuery, viewQuery);
		}
		
		[Fact]
		public void ShouldBindDocumentsCorrectly()
		{
			var entity = SimpleEntity.CreateStd();

			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.QueryAndWaitForResult(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					query => new ViewResult(
						new [] {
							new ViewResultRow(
								SimpleEntity.StandardEntityId.ToJsonFragment(),
								new { rev = SimpleEntity.StandardRevision }.ToJsonFragment(),
								SimpleEntity.StandardEntityId,
								SimpleEntity.DocWithRevision
							)
						},
						totalRowCount: 1,
						offset: 0,
						query: query
					)
			);

			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			var queryResult = session.QuerySync(new ViewQuery<SimpleEntity> { ViewName = "_all_docs", IncludeDocs = true });

			var firstRow = queryResult.First();
			Assert.NotNull(firstRow);
			Assert.Equal(SimpleEntity.StandardEntityId, firstRow.Id);
			Assert.Equal(SimpleEntity.StandardRevision, firstRow.Revision);
			Assert.Equal(entity.Age, firstRow.Age);
			Assert.Equal(entity.Date, firstRow.Date);
			Assert.Equal(entity.Name, firstRow.Name);
		}

		[Fact]
		public void ShouldThrowQueryExceptionIfNoIncludeDocsOptionAndEntityTypeParameter()
		{
			var session = new CouchSession(Default.Settings, Mock.Of<ICouchApi>());
			Assert.Throws<QueryException>(() => session.QuerySync(new ViewQuery<SimpleEntity> { ViewName = "_all_docs" }));
		}

		[Fact]
		public void ShouldThrowOnNullQuery()
		{
			Assert.Throws<ArgumentNullException>(
				() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).QuerySync<SimpleEntity>(query: null));
		}

		[Fact]
		public void ShouldMapEntitiesIfTypeIsCompatible()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.QueryAndWaitForResult(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(q =>
					new ViewResult(
						new[] { 
							new ViewResultRow (
								new object[] {"key1", 0}.ToJsonFragment(),
								new {
									Title = "Object title",
									Subject = "some"
								}.ToJsonFragment(),
								SimpleEntity.StandardDocId,
								SimpleEntity.DocWithRevision
							)
						},
						totalRowCount: 1,
						offset: 0,
						query: q
					)
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.QuerySync(new ViewQuery<SimpleEntity> {IncludeDocs = true});

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
				.Setup(a => a.QueryAndWaitForResult(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(q =>
					new ViewResult(
						new[] { 
							new ViewResultRow (
								new object[] {"key1", 0}.ToJsonFragment(),
								null,
								SimpleEntity.StandardDocId,
								SimpleEntity.DocWithRevision
							)
						},
						totalRowCount: 1,
						offset: 0,
						query: q
					)
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);

			var queriedEntity = session.QuerySync(new ViewQuery<SimpleEntity> { IncludeDocs = true }).First();
			var loadedEntity = session.LoadSync<SimpleEntity>(SimpleEntity.StandardEntityId);
			Assert.Same(queriedEntity, loadedEntity);
		}

		[Fact]
		public void ShouldNotFailIfOnNullDocumentRowsFromCouchDb()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.QueryAndWaitForResult(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(q =>
					new ViewResult(
						new[] { 
							new ViewResultRow (
								new object[] {"key1", 0}.ToJsonFragment(),
								new { Title = "Object title", Subject = "some" }.ToJsonFragment(),
								null,
								null
							)
						},
						totalRowCount: 1,
						offset: 0,
						query: q
					)
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.QuerySync(new ViewQuery<SimpleEntity> {IncludeDocs = true});

			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.Count());
			Assert.Null(queryResult.First());
		}

		[Fact]
		public void ShouldNotFailIfOnNullValueRowsFromCouchDB()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.QueryAndWaitForResult(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(q =>
					new ViewResult(
						new[] { 
							new ViewResultRow (
								new object[] {"key1", 0}.ToJsonFragment(),
								null,
								SimpleEntity.StandardDocId,
								SimpleEntity.DocWithRevision
							)
						},
						totalRowCount: 1,
						offset: 0,
						query: q
					)
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.QuerySync(new ViewQuery<SimpleViewData>());

			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.Count());
			Assert.Null(queryResult.First());
		}

		[Fact]
		public void ShouldMapViewdataIfTypeIsCompatible()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.QueryAndWaitForResult(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(q =>
					new ViewResult(
						new[] { 
							new ViewResultRow (
								new object[] {"key1", 0}.ToJsonFragment(),
								new { Title = "Object title", Subject = "some" }.ToJsonFragment(),
								SimpleEntity.StandardDocId,
								SimpleEntity.DocWithRevision
							)
						},
						totalRowCount: 1,
						offset: 0,
						query: q
					)
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var queryResult = session.QuerySync(new ViewQuery<SimpleViewData>());

			Assert.NotNull(queryResult);
			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.TotalRowCount);

			var row = queryResult.First();
			Assert.NotNull(row);
			Assert.Equal("Object title", row.Title);
		}
	}
}