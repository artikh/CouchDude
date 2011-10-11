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
using System.IO;
using System.Linq;

using CouchDude.Api;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class FullTextSearchResultParserTests
	{
		private static readonly string TestData =
			new {
				total_rows = 42,
				skip = 1,
				limit = 3,
				search_duration = 518,
				fetch_duration = 4,
				offset = 1,
				rows = new object[] {
					new {
						doc = new {
							_id = "c615149e5ac83b40b9ad20914d000117",
							_rev = "1-7af2e64f5b106d5b6c5563fc380bde87",
							eventType = "ViewerConnected",
							type = "liveVideoEvent",
							viewersCount = 2
						},
						id = "c615149e5ac83b40b9ad20914d000117",
						fields = new {one = 1, two = "two"},
						score = 0.42
					},
					new {
						doc = new {
							_id = "c615149e5ac83b40b9ad20914d00011d",
							_rev = "1-5af52f56d6ca7a6d600f2d9f4c2c7489",
							eventType = "ViewerDisconnected",
							type = "liveVideoEvent",
							viewersCount = 1
						},
						id = "c615149e5ac83b40b9ad20914d00011d",
						fields = new {three = 3, four = "four"},
						score = 0.1
					},
					new {
						doc = new {
							_id = "c615149e5ac83b40b9ad20914d000128",
							_rev = "1-f1a9cf30e6279485a204911219fd7322",
							eventType = "ViewerDisconnected",
							type = "liveVideoEvent",
							viewersCount = 0
						},
						id = "c615149e5ac83b40b9ad20914d000128",
						key = "c615149e5ac83b40b9ad20914d000128-42",
						fields = new {five = 5, six = "six"},
						score = 0.1
					}
				}
			}.ToJsonString();

		[Fact]
		public void ShouldParseViewResultInfoProperties()
		{
			ILuceneQueryResult viewResult;
			using (TextReader stringReader = new StringReader(TestData))
				viewResult = LuceneQueryResultParser.Parse(stringReader, new LuceneQuery());

			Assert.Equal(42, viewResult.TotalCount);
			Assert.Equal(3, viewResult.Count);
			Assert.Equal(1, viewResult.Offset);
			Assert.Equal(1, viewResult.Skip);
			Assert.Equal(3, viewResult.Limit);
			Assert.Equal(TimeSpan.FromMilliseconds(4), viewResult.FetchDuration);
			Assert.Equal(TimeSpan.FromMilliseconds(518), viewResult.SearchDuration);
		}

		[Theory]
		[InlineData("{{}")]
		[InlineData("{]")]
		[InlineData("{\"prop\": }")]
		[InlineData("[\"prop\": \"hello\"]")]
		public void ShouldThrowParseExceptionOnInvalidJson(string json)
		{
			using (TextReader stringReader = new StringReader(json))
				Assert.Throws<ParseException>(() => LuceneQueryResultParser.Parse(stringReader, new LuceneQuery()));
		}

		[Theory]
		[InlineData("{  rows: {}}")]
		[InlineData("{ \"total_rows\": \"non a number\", \"offset\": 0, \"rows\":[] }")]
		[InlineData("{ \"total_rows\": 42, \"offset\": \"non a number\", \"rows\":[] }")]
		[InlineData("{ \"total_rows\": 42, \"offset\": 0, \"rows\": \"non an array\" }")]
		public void ShouldThrowParseExceptionOnInvalidResponse(string json)
		{
			using (TextReader stringReader = new StringReader(json))
				Assert.Throws<ParseException>(() => LuceneQueryResultParser.Parse(stringReader, new LuceneQuery()));
		}

		[Fact]
		public void ShouldParseViewResultInfoRows()
		{
			ILuceneQueryResult viewResult;
			using (TextReader stringReader = new StringReader(TestData))
				viewResult = LuceneQueryResultParser.Parse(stringReader, new LuceneQuery());

			LuceneResultRow secondRow = viewResult.Rows.Skip(1).First();

			Assert.Equal("c615149e5ac83b40b9ad20914d00011d", secondRow.DocumentId);
			Assert.Equal(0.1m, secondRow.Score);
			Assert.Equal(new { three = 3, four = "four" }.ToJsonFragment(), secondRow.Fields);
			Assert.Equal("c615149e5ac83b40b9ad20914d00011d", secondRow.Id);
			Assert.Equal(
				new
					{
						_id = "c615149e5ac83b40b9ad20914d00011d",
						_rev = "1-5af52f56d6ca7a6d600f2d9f4c2c7489",
						eventType = "ViewerDisconnected",
						type = "liveVideoEvent",
						viewersCount = 1
					}.ToDocument(),
				secondRow.Document);
		}
	}
}