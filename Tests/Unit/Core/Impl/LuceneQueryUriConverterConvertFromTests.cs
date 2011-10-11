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
using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class LuceneQueryUriConverterConvertFromTests
	{
		private static LuceneQuery ConvertFromString(string urlString)
		{
			var converter = TypeDescriptor.GetConverter(typeof (LuceneQuery));
			if (converter != null)
				return (LuceneQuery) converter.ConvertFrom(urlString);
			return null;
		}
		
		[Fact]
		public void ShouldParseDesignDocumentName()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42");
			Assert.Equal("dd", viewQuery.DesignDocumentName);
		}

		[Fact]
		public void ShouldParseIndexName()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42");
			Assert.Equal("someIndex", viewQuery.IndexName);
		}

		[Fact]
		public void ShouldParseQuery()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=%e8%96%84%e8%8d%b7%e5%86%b0%e6%bc%a0%e6%b7%8b");
			Assert.Equal("薄荷冰漠淋", viewQuery.Query);
		}
		
		[Fact]
		public void ShouldParseSort()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&sort=%2ffield1%2c%5cfield2%2cfield3%3cdouble%3e%2c%5cfield4%3clong%3e");
			Assert.Equal(4, viewQuery.Sort.Count);
			Assert.Equal("field1", viewQuery.Sort[0].FieldName);
			Assert.Equal("field2", viewQuery.Sort[1].FieldName);
			Assert.Equal("field3", viewQuery.Sort[2].FieldName);
			Assert.Equal("field4", viewQuery.Sort[3].FieldName);
			Assert.False(viewQuery.Sort[0].SortDescending);
			Assert.True(viewQuery.Sort[1].SortDescending);
			Assert.False(viewQuery.Sort[2].SortDescending);
			Assert.True(viewQuery.Sort[3].SortDescending);
			Assert.Null(viewQuery.Sort[0].Type);
			Assert.Null(viewQuery.Sort[1].Type);
			Assert.Equal(LuceneType.Double, viewQuery.Sort[2].Type);
			Assert.Equal(LuceneType.Long, viewQuery.Sort[3].Type);
		}

		[Fact]
		public void ShouldParseIncludeDocsOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?key=%22key%22&include_docs=true");
			Assert.True(viewQuery.IncludeDocs);
		}

		[Fact]
		public void ShouldParseNegativeIncludeDocsOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&include_docs=false");
			Assert.False(viewQuery.IncludeDocs);
		}

		[Fact]
		public void ShouldParseAbsentIncludeDocsOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42");
			Assert.False(viewQuery.IncludeDocs);
		}

		[Fact]
		public void ShouldParseSkipOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&skip=42");
			Assert.Equal(42, viewQuery.Skip);
		}

		[Fact]
		public void ShouldParseLimitOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&limit=42");
			Assert.Equal(42, viewQuery.Limit);
		}

		[Fact]
		public void ShouldParseDefaultOperatorOR()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&default_operator=OR");
			Assert.False(viewQuery.UseConjunctionSematics);
		}

		[Fact]
		public void ShouldParseDefaultOperatorAND()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&default_operator=AND");
			Assert.True(viewQuery.UseConjunctionSematics);
		}

		[Fact]
		public void ShouldParseIncludeFieldsOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&include_fields=field1%2cfield2%2cfield3");
			Assert.Equal("field1", viewQuery.Fields.First());
			Assert.Equal("field2", viewQuery.Fields.Skip(1).First());
			Assert.Equal("field3", viewQuery.Fields.Skip(2).First());
		}

		[Fact]
		public void ShouldParseSingleIncludeFieldsOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&include_fields=field");
			Assert.Equal("field", viewQuery.Fields.First());
		}

		[Fact]
		public void ShouldParseEmptyIncludeFieldsOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&include_fields=");
			Assert.Null(viewQuery.Fields);
		}

		[Fact]
		public void ShouldParseStaleOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&stale=ok");
			Assert.True(viewQuery.DoNotBlockIfStale);
		}
		
		[Fact]
		public void ShouldParseAnalyserOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&analyzer=snowball%3aEnglish");
			Assert.Equal("snowball:English", viewQuery.Analyzer);
		}

		[Fact]
		public void ShouldParsePositiveDebugOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&debug=true");
			Assert.True(viewQuery.SuppressCaching);
		}

		[Fact]
		public void ShouldParseNegativeDebugOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&debug=false");
			Assert.False(viewQuery.SuppressCaching);
		}

		[Fact]
		public void ShouldParseEmptyDebugOption()
		{
			var viewQuery = ConvertFromString("_fti/_design/dd/someIndex?q=42&debug=");
			Assert.False(viewQuery.SuppressCaching);
		}
		
		[Theory]
		[InlineData("http://examlpe.com")]
		[InlineData("17dd470d57d04e62bc416d1aa5e2f7ba")]
		[InlineData("_fti/_design/someView?kui=42")]
		public void ShouldNotThrowAtAnyInput(string input)
		{
			LuceneQuery viewQuery = null;
			Assert.DoesNotThrow(() => { viewQuery = ConvertFromString(input); });
			Assert.Null(viewQuery);
		}
	}
}