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
using CouchDude.Api;
using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Impl
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
					(ViewQuery query) => {
						sendQuery = query;
						return new ViewResult(
							new[] {
								new ViewResultRow(
									Entity.StandardDocId.ToJsonFragment(),
									new {rev = Entity.StandardRevision}.ToJsonFragment(),
									Entity.StandardDocId,
									Entity.CreateDocWithRevision()
									)
							},
							1,
							offset: 0,
							query: query
							).ToTask<IPagedList<ViewResultRow>>();
					});

			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			var viewQuery = new ViewQuery<Entity> {ViewName = "_all_docs", IncludeDocs = true};
			session.Synchronously.Query(viewQuery);

			Assert.Same(sendQuery, viewQuery);
		}

		[Fact]
		public void ShouldBindDocumentsCorrectly()
		{
			Entity entity = Entity.CreateStandard();

			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					query => new ViewResult(
						new[] {
							new ViewResultRow(
								Entity.StandardDocId.ToJsonFragment(),
								new {rev = Entity.StandardRevision}.ToJsonFragment(),
								Entity.StandardDocId,
								Entity.CreateDocWithRevision()
								)
						},
						totalRowCount: 1,
						offset: 0,
						query: query
						).ToTask<IPagedList<ViewResultRow>>()
				);

			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			IPagedList<Entity> queryResult =
				session.Synchronously.Query(new ViewQuery<Entity> {ViewName = "_all_docs", IncludeDocs = true});

			Entity firstRow = queryResult.First();
			Assert.NotNull(firstRow);
			Assert.Equal(Entity.StandardEntityId, firstRow.Id);
			Assert.Equal(Entity.StandardRevision, firstRow.Revision);
			Assert.Equal(entity.Age, firstRow.Age);
			Assert.Equal(entity.Date, firstRow.Date);
			Assert.Equal(entity.Name, firstRow.Name);
		}

		[Fact]
		public void ShouldThrowQueryExceptionIfNoIncludeDocsOptionAndEntityTypeParameter()
		{
			var session = new CouchSession(Default.Settings, Mock.Of<ICouchApi>());
			Assert.Throws<QueryException>(
				() => session.Synchronously.Query(new ViewQuery<Entity> {ViewName = "_all_docs"}));
		}

		[Fact]
		public void ShouldThrowOnNullQuery()
		{
			Assert.Throws<ArgumentNullException>(
				() =>
					new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Synchronously.Query<Entity>(
						query: null));
		}

		[Fact]
		public void ShouldMapEntitiesIfTypeIsCompatible()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					q =>
						new ViewResult(
							new[] {
								new ViewResultRow(
									new object[] {"key1", 0}.ToJsonFragment(),
									new {
										Title = "Object title",
										Subject = "some"
									}.ToJsonFragment(),
									Entity.StandardDocId,
									Entity.CreateDocWithRevision()
									)
							},
							totalRowCount: 1,
							offset: 0,
							query: q
							).ToTask<IPagedList<ViewResultRow>>()
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);
			IPagedList<Entity> queryResult =
				session.Synchronously.Query(new ViewQuery<Entity> {IncludeDocs = true});

			Assert.NotNull(queryResult);
			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.TotalRowCount);

			Entity row = queryResult.First();
			Assert.NotNull(row);
			Assert.Equal(Entity.StandardEntityId, row.Id);
			Assert.Equal(Entity.StandardRevision, row.Revision);
		}

		[Fact]
		public void ShouldCacheInstanceFromQuery()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					q =>
						new ViewResult(
							new[] {
								new ViewResultRow(
									new object[] {"key1", 0}.ToJsonFragment(),
									null,
									Entity.StandardDocId,
									Entity.CreateDocWithRevision()
									)
							},
							totalRowCount: 1,
							offset: 0,
							query: q
							).ToTask<IPagedList<ViewResultRow>>()
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);

			Entity queriedEntity =
				session.Synchronously.Query(new ViewQuery<Entity> {IncludeDocs = true}).First();
			var loadedEntity = session.Synchronously.Load<Entity>(Entity.StandardEntityId);
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
						new ViewResult(
							new[] {
								new ViewResultRow(
									new object[] {"key1", 0}.ToJsonFragment(),
									new {Title = "Object title", Subject = "some"}.ToJsonFragment(),
									null,
									null
									)
							},
							totalRowCount: 1,
							offset: 0,
							query: q
							).ToTask<IPagedList<ViewResultRow>>()
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);
			IPagedList<Entity> queryResult =
				session.Synchronously.Query(new ViewQuery<Entity> {IncludeDocs = true});

			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.Count());
			Assert.Null(queryResult.First());
		}

		[Theory]
		[InlineData(@"{}")]
		[InlineData(@"{""_id"": ""entity.doc1""}")]
		[InlineData(@"{""_rev"": ""1-1a517022a0c2d4814d51abfedf9bfee7""}")]
		[InlineData(@"{""_id"": ""entity.doc1"", ""_rev"": ""1-1a517022a0c2d4814d51abfedf9bfee7""}")]
		[InlineData(@"{""_id"": ""entity.doc1"", ""type"": ""entity""}")]
		[InlineData(@"{""_rev"": ""1-1a517022a0c2d4814d51abfedf9bfee7"", ""type"": ""entity""}")]
		public void ShouldNotFailOnIncorrectDocumentFromCouchDb(string docString)
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					q =>
						new ViewResult(
							new[] {
								new ViewResultRow(
									new object[] {"key1", 0}.ToJsonFragment(),
									new {Title = "Object title", Subject = "some"}.ToJsonFragment(),
									Entity.StandardDocId,
									new Document(docString)
									)
							},
							totalRowCount: 1,
							offset: 0,
							query: q
							).ToTask<IPagedList<ViewResultRow>>()
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);
			IPagedList<Entity> queryResult =
				session.Synchronously.Query(new ViewQuery<Entity> {IncludeDocs = true});

			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.Count());
			Assert.Null(queryResult.First());
		}

		[Fact]
		public void ShouldNotFailIfOnNullValueRowsFromCouchDB()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					q =>
						new ViewResult(
							new[] {
								new ViewResultRow(
									new object[] {"key1", 0}.ToJsonFragment(),
									null,
									Entity.StandardDocId,
									Entity.CreateDocWithRevision()
									)
							},
							totalRowCount: 1,
							offset: 0,
							query: q
							).ToTask<IPagedList<ViewResultRow>>()
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);
			IPagedList<ViewData> queryResult = session.Synchronously.Query(new ViewQuery<ViewData>());

			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.Count());
			Assert.Null(queryResult.First());
		}

		[Fact]
		public void ShouldMapViewdataIfTypeIsCompatible()
		{
			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(a => a.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					q =>
						new ViewResult(
							new[] {
								new ViewResultRow(
									new object[] {"key1", 0}.ToJsonFragment(),
									new {Title = "Object title", Subject = "some"}.ToJsonFragment(),
									Entity.StandardDocId,
									Entity.CreateDocWithRevision()
									)
							},
							totalRowCount: 1,
							offset: 0,
							query: q
							).ToTask<IPagedList<ViewResultRow>>()
				);
			var session = new CouchSession(Default.Settings, couchApi.Object);
			IPagedList<ViewData> queryResult = session.Synchronously.Query(new ViewQuery<ViewData>());

			Assert.NotNull(queryResult);
			Assert.Equal(1, queryResult.RowCount);
			Assert.Equal(1, queryResult.TotalRowCount);

			ViewData row = queryResult.First();
			Assert.NotNull(row);
			Assert.Equal("Object title", row.Title);
		}
	}
}