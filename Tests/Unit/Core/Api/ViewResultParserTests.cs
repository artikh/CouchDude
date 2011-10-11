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
using System.IO;
using System.Linq;
using System.Text;

using CouchDude.Api;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class ViewResultParserTests
	{
		private static readonly string TestData =
			new
			{
				total_rows = 42,
				offset = 1,
				rows = new[] {
					new {
						doc = new {
							_id = "c615149e5ac83b40b9ad20914d000117",
							_rev = "1-7af2e64f5b106d5b6c5563fc380bde87",
							eventType = "ViewerConnected",
							type = "liveVideoEvent",
							viewersCount = 2
						},
						id = "c615149e5ac83b40b9ad20914d000117",
						key = "c615149e5ac83b40b9ad20914d000117-42",
						value = new { rev = "1-7af2e64f5b106d5b6c5563fc380bde87" }
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
						key = "c615149e5ac83b40b9ad20914d00011d-42",
						value = new { rev = "1-5af52f56d6ca7a6d600f2d9f4c2c7489" }
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
						value = new { rev = "1-f1a9cf30e6279485a204911219fd7322" }
					}
				}
			}.ToJsonString();

		[Fact]
		public void ShouldParseViewResultInfoProperties()
		{
			IViewQueryResult viewResult;
			using (TextReader stringReader = new StringReader(TestData))
				viewResult = ViewQueryResultParser.Parse(stringReader, new ViewQuery());

			Assert.Equal(42, viewResult.TotalCount);
			Assert.Equal(3, viewResult.Count);
			Assert.Equal(1, viewResult.Offset);
		}

		[Theory]
		[InlineData("{{}")]
		[InlineData("{]")]
		[InlineData("{\"prop\": }")]
		[InlineData("[\"prop\": \"hello\"]")]
		public void ShouldThrowParseExceptionOnInvalidJson(string json)
		{
			using (TextReader stringReader = new StringReader(json))
				Assert.Throws<ParseException>(() =>ViewQueryResultParser.Parse(stringReader, new ViewQuery()));
		}

		[Theory]
		[InlineData("{  rows: {}}")]
		[InlineData("{ \"total_rows\": \"non a number\", \"offset\": 0, \"rows\":[] }")]
		[InlineData("{ \"total_rows\": 42, \"offset\": \"non a number\", \"rows\":[] }")]
		[InlineData("{ \"total_rows\": 42, \"offset\": 0, \"rows\": \"non an array\" }")]
		public void ShouldThrowParseExceptionOnInvalidResponse(string json)
		{
			using (TextReader stringReader = new StringReader(json))
				Assert.Throws<ParseException>(() =>ViewQueryResultParser.Parse(stringReader, new ViewQuery()));
		}

		[Fact]
		public void ShouldParseViewResultInfoRows()
		{
			IViewQueryResult viewResult;
			using (TextReader stringReader = new StringReader(TestData))
				viewResult = ViewQueryResultParser.Parse(stringReader, new ViewQuery());

			ViewResultRow secondRow = viewResult.Rows.Skip(1).First();

			Assert.Equal("c615149e5ac83b40b9ad20914d00011d", secondRow.DocumentId);
			Assert.Equal("c615149e5ac83b40b9ad20914d00011d-42".ToJsonFragment(), secondRow.Key);
			Assert.Equal(new { rev = "1-5af52f56d6ca7a6d600f2d9f4c2c7489" }.ToJsonFragment(), secondRow.Value);
			Assert.Equal(
				new {
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
