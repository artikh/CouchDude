using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CouchDude.Core;
using CouchDude.Core.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class ViewResultParserTests
	{
		private static readonly string TestData =
			new
			{
				total_rows = 42,
				offset = 1,
				rows = new[] {
					new object[] {
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
				}
			}.ToJsonString();

		[Fact]
		public void ShouldParseViewResultInfoProperties()
		{
			IPagedList<ViewResultRow> viewResult;
			using (TextReader stringReader = new StringReader(TestData))
				viewResult = ViewResultParser.Parse(stringReader);

			Assert.Equal(42, viewResult.TotalRowCount);
			Assert.Equal(3, viewResult.RowCount);
			Assert.Equal(1, viewResult.Offset);
		}

		[Fact]
		public void ShouldParseViewResultInfoRows()
		{
			IPagedList<ViewResultRow> viewResult;
			using (TextReader stringReader = new StringReader(TestData))
				viewResult = ViewResultParser.Parse(stringReader);

			var secondRow = viewResult.Skip(1).First();

			Assert.Equal("c615149e5ac83b40b9ad20914d00011d", secondRow.DocumentId);
			Assert.Equal("c615149e5ac83b40b9ad20914d00011d-42".ToJsonFragment(), secondRow.Key);
			Assert.Equal(new { rev = "1-5af52f56d6ca7a6d600f2d9f4c2c7489" }.ToJsonFragment(), secondRow.Value);
			Assert.Equal(
				new
					{
						_id = "c615149e5ac83b40b9ad20914d00011d",
						_rev = "1-5af52f56d6ca7a6d600f2d9f4c2c7489",
						eventType = "ViewerDisconnected",
						type = "liveVideoEvent",
						viewersCount = 1
					}.ToJsonFragment(),
				secondRow.Value);
		}
	}

}
