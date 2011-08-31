using System;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Unit.Core
{
	public class ViewQueryTests
	{
		[Fact]
		public void ShouldConvertFromStringAndBackUsingCtorAndToString()
		{
			const string testUri = 
				"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start+dockey&endkey=%22second+key%22&endkey_docid=end+dockey" +
				"&limit=42&skip=42&stale=update_after&descending=true&reduce=false&include_docs=true&inclusive_end=false&group=true&group_level=42";

			var viewQuery = new ViewQuery(testUri);
			var generatedUri = viewQuery.ToString();

			Assert.Equal(testUri, generatedUri);
		}

		[Fact]
		public void ShouldConvertFromUriAndBackUsingCtorAndToUri()
		{
			var testUri = new Uri(
				"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start+dockey&endkey=%22second+key%22&endkey_docid=end+dockey" +
				"&limit=42&skip=42&stale=update_after&descending=true&reduce=false&include_docs=true&inclusive_end=false&group=true&group_level=42", 
				UriKind.Relative);

			var viewQuery = new ViewQuery(testUri);
			var generatedUri = viewQuery.ToUri();

			Assert.Equal(testUri, generatedUri);
		}

		[Fact]
		public void ShouldClone() 
		{
			var query = new ViewQuery(
				"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start+dockey&endkey=%22second+key%22&endkey_docid=end+dockey" +
				"&limit=42&skip=42&stale=update_after&descending=true&reduce=false&include_docs=true&inclusive_end=false&group=true&group_level=42");

			var clone = query.Clone();

			Assert.Equal(query, clone);
		}
	}
}
