using CouchDude.Core;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class ViewQueryTests
	{
		[Fact]
		public void ShouldCorrectlyGenerateKeyRangeUri()
		{
			Assert.Equal(
				"_design/dd/_view/pointOfView?startkey=%5b%22first+key%22%2c0%5d&endkey=%5b%22second+key%22%2c9%5d",
				new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "pointOfView",
					StartKey = new object[]{ "first key", 0 },
					EndKey = new object[]{ "second key", 9 }
				}.ToUri()
			);
		}

		[Fact]
		public void ShouldCorrectlyGenerateKeySingleKeyUri()
		{
			Assert.Equal(
				"_design/dd/_view/pointOfView?key=%5b%22key%22%2c0%5d",
				new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "pointOfView",
					Key = new object[]{ "key", 0 }
				}.ToUri()
			);
		}

		[Fact]
		public void ShouldAddIncludeDocsParameter()
		{
			Assert.Equal(
				"_design/dd/_view/pointOfView?key=%22key%22&include_docs=true",
				new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "pointOfView",
					Key = "key",
					IncludeDocs = true
				}.ToUri()
			);
		}

		[Fact]
		public void ShouldGenerateSpecialAllDocumentsViewUrl()
		{
			Assert.Equal("_all_docs", new ViewQuery {ViewName = "_all_docs"}.ToUri());
		}

		[Fact]
		public void ShouldThrowQueryExceptionIfNotSecialViewAndNoDesignDocumentMentioned()
		{
			Assert.Throws<QueryException>(() => new ViewQuery { ViewName = "_not_so_special_view" }.ToUri());
		}
	}
}
