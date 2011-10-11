#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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
			var result = new LuceneQueryResult(new LuceneQuery(), new LuceneResultRow[3], 3, 0, default(TimeSpan), default(TimeSpan), 0, 0);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNullIfTotalCountAndCountAreEqualForTypedLuceneQueryResult()
		{
			var result = new LuceneQueryResult<object>(
				new LuceneQuery(), new LuceneResultRow[3],  3, 0, default(TimeSpan), default(TimeSpan), 0, 0, StubConvertor);
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
			var result = new LuceneQueryResult(new LuceneQuery(), new LuceneResultRow[0], 0, 3, 0, default(TimeSpan), default(TimeSpan), 0, 0);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNullIfCountIsZeroForTypedLuceneQueryResult() 
		{
			var result = new LuceneQueryResult<object>(
				new LuceneQuery(), new LuceneResultRow[0], 0, 3, 0, default(TimeSpan), default(TimeSpan), 0, 0, StubConvertor);
			Assert.Null(result.NextPageQuery);
		}

		[Fact]
		public void ShouldReturnNextPageQueryForViewQueryResultAndRangeKeyQuery() 
		{
			var result = new ViewQueryResult<object>(
				ViewQuery.Parse("_design/dd/_view/pointOfView?startkey=%22first%22&endkey=%22third%22"),
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
				ViewQuery.Parse("_design/dd/_view/pointOfView?startkey=%22first%22&endkey=%22third%22"),
				new [] {
					new ViewResultRow("firstKey".ToJsonFragment(), null, "firstDocId", null),
					new ViewResultRow("firstKey".ToJsonFragment(), null, "secondDocId", null)
				}, 
				totalCount: 3, 
				offset: 0, 
				rowConvertor: StubConvertor);

			var nextPageQuery = result.NextPageQuery;

			Assert.Equal("firstKey".ToJsonFragment(), nextPageQuery.StartKey);
			Assert.Equal("secondDocId", nextPageQuery.StartDocumentId);
			Assert.Equal(1, nextPageQuery.Skip);
		}

		[Fact]
		public void ShouldReturnNextPageQueryMentioningStartDocIdForViewQueryResultAndKeyQuery() 
		{
			var result = new ViewQueryResult<object>(
				ViewQuery.Parse("_design/dd/_view/pointOfView?key=%22keyvalue%22"),
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
				ViewQuery.Parse("_design/dd/_view/pointOfView?startkey=%22first%22&endkey=%22third%22&limit=2"),
				new[] {
					new ViewResultRow("first".ToJsonFragment(), null, null, null),
					new ViewResultRow("second".ToJsonFragment(), null, null, null)
				}, 
				totalCount: 3, 
				offset: 0, 
				rowConvertor: StubConvertor);

			Assert.Equal(2, result.NextPageQuery.Limit);
		}
		
		[Fact]
		public void ShouldReturnNextPageQueryForLuceneQueryResult() 
		{
			var result = new LuceneQueryResult<object>(
				new LuceneQuery { DesignDocumentName = "dd", IndexName = "someIndex", Query = "test:query", Limit = 2 },
				new[] {
					new LuceneResultRow("54e39b4a6d9442c6bd2032e6b12fec5d", "field-value".ToJsonFragment(), 0, null, null),
					new LuceneResultRow("db7df2f7a0da4a24923ac2a02b9cb7f7", "field-value".ToJsonFragment(), 0, null, null)
				}, 
				totalCount: 3, 
				offset: 0,
				fetchDuration: default(TimeSpan),
				searchDuration: default(TimeSpan),
				limit: 2,
				skip: 0,
				rowConvertor: StubConvertor);

			Assert.Equal(2, result.NextPageQuery.Skip);
			Assert.Equal(2, result.NextPageQuery.Limit);
		}
		
		[Fact]
		public void ShouldUseLimitAndSkipReturnedWithResult() 
		{
			var result = new LuceneQueryResult<object>(
				new LuceneQuery { DesignDocumentName = "dd", IndexName = "someIndex", Query = "test:query", Limit = 100500, Skip = 2 },
				new[] {
					new LuceneResultRow("54e39b4a6d9442c6bd2032e6b12fec5d", "field-value".ToJsonFragment(), 0, null, null),
					new LuceneResultRow("db7df2f7a0da4a24923ac2a02b9cb7f7", "field-value".ToJsonFragment(), 0, null, null)
				}, 
				totalCount: 3, 
				offset: 0,
				fetchDuration: default(TimeSpan),
				searchDuration: default(TimeSpan),
				limit: 2,
				skip: 0,
				rowConvertor: StubConvertor);

			Assert.Equal(0 + 2, result.NextPageQuery.Skip);
			Assert.Equal(2, result.NextPageQuery.Limit);
		}

		private static IEnumerable<object> StubConvertor(IEnumerable<LuceneResultRow> r) { return r.Select(_ => (object) null); }
		private static IEnumerable<object> StubConvertor(IEnumerable<ViewResultRow> r) { return r.Select(_ => (object)null); }
	}
}