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

using System.IO;
using System.Linq;
using System.Text;

using CouchDude.SchemeManager;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.SchemeManager
{
	public class DesignDocumentExtractorTest
	{
		readonly IDesignDocumentExtractor designDocumentExtractor = new DesignDocumentExtractor();

		private static TextReader CreateStream(string str)
		{
			return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(str)), Encoding.UTF8);
		}

		[Fact]
		public void ShouldThrowOnUnknownStructure()
		{
            Assert.Throws<ParseException>(
				() =>
				designDocumentExtractor.Extract(CreateStream(@"
					{ 
						""some"": ""arbitrary"",
						""json"": [
							{ ""with"": ""nested"" },
							""structure""
						]
					}")));
		}

		[Fact]
		public void ShouldThrowOnKeylessRow()
		{
            Assert.Throws<ParseException>(
				() =>
				designDocumentExtractor.Extract(CreateStream(@"
					{
						""rows"": [
							{
								""value"": { ""rev"": ""3-ee7084f94345720bf9fdcd8f087e5518"" },
								""doc"": { }
							}
						]
					}
				")));
		}

		[Fact]
		public void ShouldThrowOnValuelessRow()
		{
            Assert.Throws<ParseException>(
				() =>
				designDocumentExtractor.Extract(CreateStream(@"
					{
						""rows"": [
							{
								""key"": ""_design/bin_doc"",
								""doc"": { }
							}
						]
					}
				")));
		}

		[Fact]
		public void ShouldThrowOnRevisionlessRow()
		{
            Assert.Throws<ParseException>(
				() =>
				designDocumentExtractor.Extract(CreateStream(@"
					{
						""rows"": [
							{
								""key"": ""_design/bin_doc"",
								""value"": { },
								""doc"": { }
							}
						]
					}
				")));
		}

		[Fact]
		public void ShouldThrowOnNoneDesignPrefexedKeyRow()
		{
            Assert.Throws<ParseException>(
				() =>
				designDocumentExtractor.Extract(CreateStream(@"
					{
						""rows"": [
							{
								""key"": ""doc1"",
								""value"": { ""rev"": ""3-ee7084f94345720bf9fdcd8f087e5518"" },
								""doc"": { }
							}
						]
					}
				")));
		}

		[Fact]
		public void ShouldThrowOnDocumentlessRow()
		{
            Assert.Throws<ParseException>(
				() =>
				designDocumentExtractor.Extract(CreateStream(@"
					{
						""rows"": [
							{
								""id"": ""_design/bin_doc"",
								""key"": ""_design/bin_doc"",
								""value"": { ""rev"": ""3-ee7084f94345720bf9fdcd8f087e5518"" }
							}
						]
					}
				")));
		}

		[Fact]
		public void ShouldExtractDesignDocumentsFromCorrectStream()
		{
			var documents =
				designDocumentExtractor.Extract(CreateStream(
					@"
					{
						""total_rows"": 2,
						""offset"": 0,
						""rows"": [
							{
								""id"": ""_design/bin_doc1"",
								""key"": ""_design/bin_doc1"",
								""value"": {
									""rev"": ""3-ee7084f94345720bf9fdcd8f087e5518""
								},
								""doc"": {
									""_id"": ""_design/bin_doc1"",
									""_rev"": ""3-ee7084f94345720bf9fdcd8f087e5518"",
									""some_property1"": ""test content""
								}
							},
							{
								""id"": ""_design/bin_doc2"",
								""key"": ""_design/bin_doc2"",
								""value"": {
									""rev"": ""2-d6c91c592f5aa33822db032d3eb61ca1""
								},
								""doc"": {
									""_id"": ""_design/bin_doc2"",
									""_rev"": ""2-d6c91c592f5aa33822db032d3eb61ca1"",
									""some_property2"": ""test content""
								}
							}
						]
					}
				"));



			Assert.Equal(2, documents.Count);
			var keys = documents.Keys.ToList();
			var designDocumentA = documents[keys[0]];
			var designDocumentB = documents[keys[1]];

			var expectedDocA = JObject.Parse(@"{
				""_id"": ""_design/bin_doc1"", 
				""_rev"": ""3-ee7084f94345720bf9fdcd8f087e5518"",
				""some_property1"": ""test content"" 
			}");
			Assert.Equal(expectedDocA, designDocumentA.Definition, new JTokenEqualityComparer());
			Assert.Equal("3-ee7084f94345720bf9fdcd8f087e5518", designDocumentA.Revision);
			Assert.Equal("_design/bin_doc1", designDocumentA.Id);

			var expectedDocB = JObject.Parse(@"{
				""_id"": ""_design/bin_doc2"", 
				""_rev"": ""2-d6c91c592f5aa33822db032d3eb61ca1"",
				""some_property2"": ""test content"" 
			}");
			Assert.Equal(expectedDocB, designDocumentB.Definition, new JTokenEqualityComparer());
			Assert.Equal("2-d6c91c592f5aa33822db032d3eb61ca1", designDocumentB.Revision);
			Assert.Equal("_design/bin_doc2", designDocumentB.Id);
		}
	}
}
