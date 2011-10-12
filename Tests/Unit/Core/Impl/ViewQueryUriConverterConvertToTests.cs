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
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class ViewQueryUriConverterConvertToTests
	{
		private static string ConvertToString(ViewQuery viewQuery)
		{
			var converter = TypeDescriptor.GetConverter(typeof (ViewQuery));
			if (converter != null) 
				return (string)converter.ConvertTo(viewQuery, typeof (string));
			return null;
		}

		[Fact]
		public void ShouldCorrectlyGenerateKeyRangeUri()
		{
			Assert.Equal(
				"_design/dd/_view/pointOfView?startkey=%5B%22first%20key%22,0%5D&endkey=%5B%22second%20key%22,9%5D",
				ConvertToString(new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "pointOfView",
					StartKey = new object[]{ "first key", 0 },
					EndKey = new object[]{ "second key", 9 }
				})
			);
		}

		[Fact]
		public void ShouldCorrectlyGenerateKeyRangeUriIfRangeKeysAreJsonFragments()
		{
			Assert.Equal(
				"_design/dd/_view/pointOfView?startkey=%5B%22first%20key%22,0%5D&endkey=%5B%22second%20key%22,9%5D",
				ConvertToString(new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "pointOfView",
					StartKey = new JsonFragment("[\"first key\",0]"),
					EndKey = new JsonFragment("[\"second key\",9]")
				})
			);
		}

		[Fact]
		public void ShouldCorrectlyGenerateKeySingleKeyUriIfKeyIsJsonFragment()
		{
			Assert.Equal(
				"_design/dd/_view/pointOfView?key=%5B%22key%22,0%5D",
				ConvertToString(new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "pointOfView",
					Key = new JsonFragment("[\"key\",0]")
				})
			);
		}

		[Fact]
		public void ShouldCorrectlyGenerateKeySingleKeyUri()
		{
			Assert.Equal(
				"_design/dd/_view/pointOfView?key=%5B%22key%22,0%5D",
				ConvertToString(new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "pointOfView",
					Key = new object[]{ "key", 0 }
				})
			);
		}

		[Fact]
		public void ShouldAddIncludeDocsParameter()
		{
			Assert.Equal(
				"_design/dd/_view/pointOfView?key=%22key%22&include_docs=true",
				ConvertToString(new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "pointOfView",
					Key = "key",
					IncludeDocs = true
				})
			);
		}

		[Fact]
		public void ShouldEmitStaleOk()
		{
			Assert.Equal(
				"_design/dd/_view/pointOfView?key=%22key%22&stale=ok",
				ConvertToString(new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "pointOfView",
					Key = "key",
					StaleViewIsOk = true
				})
			);
		}

		[Fact]
		public void ShouldEmitStaleOkUpdateAfter()
		{
			Assert.Equal(
				"_design/dd/_view/pointOfView?key=%22key%22&stale=update_after",
				ConvertToString(new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "pointOfView",
					Key = "key",
					StaleViewIsOk = true,
					UpdateIfStale = true
				})
			);
		}

		[Fact]
		public void ShouldGenerateSpecialAllDocumentsViewUrl()
		{
			Assert.Equal("_all_docs?key=%22key%22", ConvertToString(new ViewQuery { ViewName = "_all_docs", Key = "key" }));
		}

		[Fact]
		public void ShouldThrowQueryExceptionIfNotSecialViewAndNoDesignDocumentMentioned()
		{
			Assert.Null(ConvertToString(new ViewQuery { ViewName = "_not_so_special_view" }));
		}

		[Fact]
		public void ShouldThrowQueryExceptionIfNoViewNameMentioned()
		{
			Assert.Null(ConvertToString(new ViewQuery { DesignDocumentName = "dd" }));
		}
	}
}
