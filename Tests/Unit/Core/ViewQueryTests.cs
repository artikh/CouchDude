using System;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Unit.Core
{
	public class ViewQueryTests
	{
		[Fact]
		public void ShouldConvertFromStringAndBackUsingParseAndToString()
		{
			const string testUri =
				"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start+dockey&endkey=%22second+key%22&endkey_docid=end+dockey" +
				"&limit=42&skip=42&descending=true&include_docs=true&inclusive_end=false&group=true&group_level=42&reduce=false&stale=update_after";

			var viewQuery = ViewQuery.Parse(testUri);
			var generatedUri = viewQuery.ToString();

			Assert.Equal(testUri, generatedUri);
		}

		[Fact]
		public void ShouldConvertFromUriAndBackUsingParseAndToUri()
		{
			var testUri = new Uri(
				"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start+dockey&endkey=%22second+key%22&endkey_docid=end+dockey" +
					"&limit=42&skip=42&descending=true&include_docs=true&inclusive_end=false&group=true&group_level=42&reduce=false&stale=update_after", 
				UriKind.Relative);

			var viewQuery = ViewQuery.Parse(testUri);
			var generatedUri = viewQuery.ToUri();

			Assert.Equal(testUri, generatedUri);
		}

		[Fact]
		public void ShouldClone() 
		{
			var query = ViewQuery.Parse(
				"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start+dockey&endkey=%22second+key%22&endkey_docid=end+dockey" +
				"&limit=42&skip=42&stale=update_after&descending=true&reduce=false&include_docs=true&inclusive_end=false&group=true&group_level=42");

			var clone = query.Clone();

			Assert.Equal(query, clone);
		}
	}
}
