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

using System.ComponentModel;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class ViewQueryUriConverterConvertFromTests
	{
		private static ViewQuery ConvertFromString(string urlString)
		{
			var converter = TypeDescriptor.GetConverter(typeof (ViewQuery));
			if (converter != null)
				return (ViewQuery)converter.ConvertFrom(urlString);
			return null;
		}

		[Fact]
		public void ShouldConvertFromStringAndBack()
		{
			const string testUri = 
				"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start+dockey&endkey=%22second+key%22&endkey_docid=end+dockey" +
				"&limit=42&skip=42&descending=true&include_docs=true&inclusive_end=false&group=true&group_level=42&reduce=false&stale=update_after";

			var converter = TypeDescriptor.GetConverter(typeof(ViewQuery));
			var viewQuery = (ViewQuery) converter.ConvertFrom(testUri);
			var generatedUri = (string) converter.ConvertTo(viewQuery, typeof (string));

			Assert.Equal(testUri, generatedUri);
		}

		[Fact]
		public void ShouldParseDesignDocumentName() 
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22");
			Assert.Equal("dd", viewQuery.DesignDocumentName);
		}

		[Fact]
		public void ShouldParseViewName() 
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22");
			Assert.Equal("pointOfView", viewQuery.ViewName);
		}

		[Fact]
		public void ShouldParseSingleKeyViewLookup() 
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22");
			Assert.Equal("\"key\"", viewQuery.Key.ToString());
		}

		[Fact]
		public void ShouldParseKeyRangeViewLookup() 
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?startkey=%5b%22first%20key%22%2c0%5d&endkey=%5b%22second%20key%22%2c9%5d");

			Assert.Equal("[\"first key\",0]", viewQuery.StartKey.ToString());
			Assert.Equal("[\"second key\",9]", viewQuery.EndKey.ToString());
		}

		[Fact]
		public void ShouldParseStartDocIdAndEndDocId()
		{
			var viewQuery =
				ConvertFromString(
					"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start%20dockey&endkey=%22second%20key%22&endkey_docid=end%20dockey");
			Assert.Equal("start dockey", viewQuery.StartDocumentId);
			Assert.Equal("end dockey", viewQuery.EndDocumentId);
		}

		[Fact]
		public void ShouldParseIncludeDocsOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&include_docs=true");
			Assert.True(viewQuery.IncludeDocs);
		}

		[Fact]
		public void ShouldParseNegativeIncludeDocsOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&include_docs=false");
			Assert.False(viewQuery.IncludeDocs);
		}

		[Fact]
		public void ShouldParseInclusiveEndOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&inclusive_end=true");
			Assert.False(viewQuery.DoNotIncludeEndKey);
		}

		[Fact]
		public void ShouldParseNegativeInclusiveEndOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&inclusive_end=false");
			Assert.True(viewQuery.DoNotIncludeEndKey);
		}

		[Fact]
		public void ShouldParseGroupLevelOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&group_level=42");
			Assert.Equal(42, viewQuery.GroupLevel);
		}

		[Fact]
		public void ShouldParseSkipOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&skip=42");
			Assert.Equal(42, viewQuery.Skip);
		}

		[Fact]
		public void ShouldParseLimitOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&limit=42");
			Assert.Equal(42, viewQuery.Limit);
		}

		[Fact]
		public void ShouldParseDescendingOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&descending=true");
			Assert.True(viewQuery.FetchDescending);
		}

		[Fact]
		public void ShouldParseNegativeDescendingOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&descending=false");
			Assert.False(viewQuery.FetchDescending);
		}

		[Fact]
		public void ShouldParseStaleOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&stale=ok");
			Assert.True(viewQuery.StaleViewIsOk);
		}

		[Fact]
		public void ShouldParseStaleUpdateAfterOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&stale=update_after");
			Assert.True(viewQuery.StaleViewIsOk);
			Assert.True(viewQuery.UpdateIfStale);
		}

		[Fact]
		public void ShouldParseGroupOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&group=true");
			Assert.True(viewQuery.Group);
		}

		[Fact]
		public void ShouldParseNegativeGroupOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&group=false");
			Assert.False(viewQuery.Group);
		}

		[Fact]
		public void ShouldParseReduceOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&reduce=true");
			Assert.False(viewQuery.SuppressReduce);
		}

		[Fact]
		public void ShouldParseNegativeReduceOption()
		{
			var viewQuery = ConvertFromString("_design/dd/_view/pointOfView?key=%22key%22&reduce=false");
			Assert.True(viewQuery.SuppressReduce);
		}

		[Theory]
		[InlineData("http://examlpe.com")]
		[InlineData("17dd470d57d04e62bc416d1aa5e2f7ba")]
		[InlineData("_design/someView?kui=42")]
		public void ShouldNotThrowAtAnyInput(string input)
		{
			ViewQuery viewQuery = null;
			Assert.DoesNotThrow(() => { viewQuery = ConvertFromString(input); });
			Assert.Null(viewQuery);
		}
	}
}