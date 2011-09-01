using System.Collections.Generic;
using System.Linq;
using CouchDude.Impl;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class ViewQueryResultNextPageQueryTests
	{
		[Fact]
		public void ShouldReturnNullIfTotalCountAndCountAreEqualForUntypedViewQueryResult()
		{
			var result = new ViewQueryResult(new ViewQuery(), new ViewResultRow[3], totalCount: 3, offset: 0);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNullIfTotalCountAndCountAreEqualForTypedViewQueryResult()
		{
			var result = new ViewQueryResult<object>(
				new ViewQuery(), new ViewResultRow[3], totalCount: 3, offset: 0, rowConvertor: StubConvertor);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNullIfTotalCountAndCountAreEqualForUntypedLuceneQueryResult()
		{
			var result = new LuceneQueryResult(new LuceneQuery(), new LuceneResultRow[3], totalCount: 3, offset: 0);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNullIfTotalCountAndCountAreEqualForTypedLuceneQueryResult()
		{
			var result = new LuceneQueryResult<object>(
				new LuceneQuery(), new LuceneResultRow[3], totalCount: 3, offset: 0, rowConvertor: StubConvertor);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNullIfCountIsZeroForUntypedViewQueryResult() 
		{
			var result = new ViewQueryResult(new ViewQuery(), new ViewResultRow[0], count: 0, totalCount: 3, offset: 0);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNullIfCountIsZeroForTypedViewQueryResult() 
		{
			var result = new ViewQueryResult<object>(
				new ViewQuery(), new ViewResultRow[0], count: 0, totalCount: 3, offset: 0, rowConvertor: StubConvertor);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNullIfCountIsZeroForUntypedLuceneQueryResult() 
		{
			var result = new LuceneQueryResult(new LuceneQuery(), new LuceneResultRow[0], count: 0, totalCount: 3, offset: 0);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNullIfCountIsZeroForTypedLuceneQueryResult() 
		{
			var result = new LuceneQueryResult<object>(
				new LuceneQuery(), new LuceneResultRow[0], count: 0, totalCount: 3, offset: 0, rowConvertor: StubConvertor);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNextPageQueryForViewQueryResultAndRangeKeyQuery() 
		{
			var result = new ViewQueryResult<object>(
				new ViewQuery("_design/dd/_view/pointOfView?startkey=%22first%22&endkey=%22third%22"),
				new [] {
					new ViewResultRow("first".ToJsonFragment(), null, null, null),
					new ViewResultRow("second".ToJsonFragment(), null, null, null)
				}, 
				totalCount: 3, 
				offset: 0, 
				rowConvertor: StubConvertor);

			var nextPageQuery = result.NextPageQuery;

			Assert.Equal("second".ToJsonFragment(), nextPageQuery.StartKey);
			Assert.Null(nextPageQuery.StartDocumentId);
			Assert.Equal(1, nextPageQuery.Skip);
		}

		[Fact]
		public void ShouldReturnNextPageQueryMentioningStartDocIdForViewQueryResultAndRangeKeyQuery() 
		{
			var result = new ViewQueryResult<object>(
				new ViewQuery("_design/dd/_view/pointOfView?startkey=%22first%22&endkey=%22third%22"),
				new [] {
					new ViewResultRow("first".ToJsonFragment(), null, "firstDocId", null),
					new ViewResultRow("first".ToJsonFragment(), null, "secondDocId", null)
				}, 
				totalCount: 3, 
				offset: 0, 
				rowConvertor: StubConvertor);

			var nextPageQuery = result.NextPageQuery;

			Assert.Equal("second".ToJsonFragment(), nextPageQuery.StartKey);
			Assert.Equal("secondDocId", nextPageQuery.StartDocumentId);
			Assert.Equal(1, nextPageQuery.Skip);
		}

		[Fact]
		public void ShouldReturnNextPageQueryMentioningStartDocIdForViewQueryResultAndKeyQuery() 
		{
			var result = new ViewQueryResult<object>(
				new ViewQuery("_design/dd/_view/pointOfView?key=%22keyvalue%22"),
				new [] {
					new ViewResultRow("keyvalue".ToJsonFragment(), null, "firstDocId", null),
					new ViewResultRow("keyvalue".ToJsonFragment(), null, "secondDocId", null)
				}, 
				totalCount: 3, 
				offset: 0, 
				rowConvertor: StubConvertor);

			var nextPageQuery = result.NextPageQuery;

			Assert.Null(nextPageQuery.StartKey);
			Assert.Null(nextPageQuery.EndKey);
			Assert.Equal("keyvalue".ToJsonFragment(), nextPageQuery.Key);
			Assert.Equal("secondDocId", nextPageQuery.StartDocumentId);
			Assert.Equal(1, nextPageQuery.Skip);
		}
		
		[Fact]
		public void ShouldReturnNextPageQueryCopingLimitForViewQueryResultAndRangeKeyQuery() 
		{
			var result = new ViewQueryResult<object>(
				new ViewQuery("_design/dd/_view/pointOfView?startkey=%22first%22&endkey=%22third%22&limit=2"),
				new[] {
					new ViewResultRow("first".ToJsonFragment(), null, null, null),
					new ViewResultRow("second".ToJsonFragment(), null, null, null)
				}, 
				totalCount: 3, 
				offset: 0, 
				rowConvertor: StubConvertor);

			Assert.Equal(2, result.NextPageQuery.Skip);
		}
		
		[Fact]
		public void ShouldReturnNextPageQueryForLuceneQueryResult() 
		{
			var result = new LuceneQueryResult<object>(
				new LuceneQuery("dd", "someIndex", "test:query") { Limit = 2 },
				new[] {
					new LuceneResultRow("field-value".ToJsonFragment(), 0, null, null),
					new LuceneResultRow("field-value".ToJsonFragment(), 0, null, null)
				}, 
				totalCount: 3, 
				offset: 0, 
				rowConvertor: StubConvertor);

			Assert.Equal(2, result.NextPageQuery.Skip);
			Assert.Equal(2, result.NextPageQuery.Limit);
		}

		private static IEnumerable<object> StubConvertor(IEnumerable<LuceneResultRow> r) { return r.Select(_ => (object) null); }
		private static IEnumerable<object> StubConvertor(IEnumerable<ViewResultRow> r) { return r.Select(_ => (object)null); }
	}
}