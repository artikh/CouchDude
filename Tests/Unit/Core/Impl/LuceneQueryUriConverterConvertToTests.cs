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

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class LuceneQueryUriConverterConvertToTests
	{
		private static string ConvertToString(LuceneQuery viewQuery, bool setDefaultDesignDocumentAndView = true)
		{
			if (setDefaultDesignDocumentAndView)
			{
				viewQuery.DesignDocumentName = "dd";
				viewQuery.IndexName = "someIndex";
			}

			var converter = TypeDescriptor.GetConverter(typeof(LuceneQuery));
			if (converter != null)
				return (string) converter.ConvertTo(viewQuery, typeof (string));
			return null;
		}

		[Fact]
		public void ShouldThrowQueryExceptionIfNoDesignDocumentMentioned()
		{
			Assert.Null(ConvertToString(new LuceneQuery { IndexName = "someIndex" }, setDefaultDesignDocumentAndView: false));
		}

		[Fact]
		public void ShouldThrowQueryExceptionIfNoIndexNameMentioned()
		{
			Assert.Null(ConvertToString(new LuceneQuery { DesignDocumentName = "dd" }, setDefaultDesignDocumentAndView: false));
		}
		
		[Fact]
		public void ShouldEncodeQuery()
		{
			Assert.Equal(
				"_fti/_design/dd/someIndex?q=%E8%96%84%E8%8D%B7%E5%86%B0%E6%BC%A0%E6%B7%8B",
				ConvertToString(new LuceneQuery{ Query = "薄荷冰漠淋" })
				);
		}

		[Fact]
		public void ShouldAddSkipParameter()
		{
			Assert.Equal("_fti/_design/dd/someIndex?q=42&skip=42", ConvertToString(new LuceneQuery{ Query = "42", Skip = 42}));
		}

		[Fact]
		public void ShouldAddLimitParameter()
		{
			Assert.Equal("_fti/_design/dd/someIndex?q=42&limit=42", ConvertToString(new LuceneQuery{ Query = "42", Limit = 42}));
		}

		[Fact]
		public void ShouldUseDoNotBlockIfStaleParameterIfTrue()
		{
			Assert.Equal("_fti/_design/dd/someIndex?q=42&stale=ok", ConvertToString(new LuceneQuery{ Query = "42", DoNotBlockIfStale = true}));
		}

		[Fact]
		public void ShouldNotUseDoNotBlockIfStaleParameterIfFalse()
		{
			Assert.Equal("_fti/_design/dd/someIndex?q=42", ConvertToString(new LuceneQuery{ Query = "42", DoNotBlockIfStale = false}));
		}

		[Fact]
		public void ShouldUseFieldsParameter()
		{
			Assert.Equal(
				"_fti/_design/dd/someIndex?q=42&include_fields=field1,field2,field3", 
				ConvertToString(new LuceneQuery { Query = "42", Fields = new[] { "field1", "field2", "field3" } }));
		}

		[Fact]
		public void ShouldUseSingleItemFieldsParameter()
		{
			Assert.Equal(
				"_fti/_design/dd/someIndex?q=42&include_fields=field", 
				ConvertToString(new LuceneQuery { Query = "42", Fields = new[] { "field" } }));
		}

		[Fact]
		public void ShouldAddIncludeDocsIfTrue()
		{
			Assert.Equal(
				"_fti/_design/dd/someIndex?q=42&include_docs=true", 
				ConvertToString(new LuceneQuery { Query = "42", IncludeDocs = true }));
		}

		[Fact]
		public void ShouldNotAddIncludeDocsIfFalse()
		{
			Assert.Equal(
				"_fti/_design/dd/someIndex?q=42", 
				ConvertToString(new LuceneQuery { Query = "42", IncludeDocs = false }));
		}

		[Fact]
		public void ShouldGenerateSortParameter()
		{
			Assert.Equal(
				"_fti/_design/dd/someIndex?q=42&sort=field1,%5Cfield2,field3%3Cdouble%3E,%5Cfield4%3Clong%3E",
				ConvertToString(
					new LuceneQuery {
						Query = "42",
						Sort = new[] {
							new LuceneSort("field1"),
							new LuceneSort("field2", sortDescending: true),
							new LuceneSort("field3", type: LuceneType.Double),
							new LuceneSort("field4", sortDescending: true, type: LuceneType.Long)
						}
					}));
		}
		
		[Fact]
		public void ShouldUseDebugModeIfSuppressCaching()
		{
			Assert.Equal(
				"_fti/_design/dd/someIndex?q=42&debug=true",
				ConvertToString(new LuceneQuery { Query = "42", SuppressCaching = true}));
		}
		
		[Fact]
		public void ShouldNotUseDebugModeIfSuppressCachingFalse()
		{
			Assert.Equal(
				"_fti/_design/dd/someIndex?q=42",
				ConvertToString(new LuceneQuery { Query = "42", SuppressCaching = false}));
		}

		[Fact]
		public void ShouldSetDefaultOperatorToAndIfUseConjunctionSematicsIsTrue()
		{
			Assert.Equal(
				"_fti/_design/dd/someIndex?q=42&default_operator=AND",
				ConvertToString(new LuceneQuery { Query = "42", UseConjunctionSematics = true}));
		}

		[Fact]
		public void ShouldNotSetDefaultOperatorToAndIfUseConjunctionSematicsIsFalse()
		{
			Assert.Equal(
				"_fti/_design/dd/someIndex?q=42", ConvertToString(new LuceneQuery { Query = "42", UseConjunctionSematics = false}));
		}
	}
}