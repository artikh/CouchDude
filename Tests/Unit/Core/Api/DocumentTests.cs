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
using CouchDude.Api;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class DocumentTests
	{
		[Fact]
		public void ShouldAccessIdViaSpecialProperty()
		{
			var document = new Document("{\"_id\": \"14BD5244-0C74-4882-B123-34129DE5713F\"}");
			Assert.Equal("14BD5244-0C74-4882-B123-34129DE5713F", document.Id);
		}

		[Fact]
		public void ShouldAccessRevisionViaSpecialProperty()
		{
			var document = new Document("{\"_rev\": \"1-42\"}");
			Assert.Equal("1-42", document.Revision);
		}
		
		[Fact]
		public void ShouldAccessTypeViaSpecialProperty()
		{
			var document = new Document("{\"type\": \"simpleEntity\"}");
			Assert.Equal("simpleEntity", document.Type);
		}

		[Theory]
		[InlineData("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_id\":\"8A7FD19B\",\"_rev\":\"2-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_rev\":\"2-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_id\":\"8A7FD19B\",\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_id\":\"8A7FD19B\",\"_rev\":\"2-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"name\":\"John\"}", "{\"_id\":\"8A7FD19B\",\"_rev\":\"2-42\",\"name\":\"John\"}")]
		[InlineData("{\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_rev\":\"2-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_id\":\"8A7FD19B\",\"name\":\"John\"}", "{\"_id\":\"8A7FD19B\",\"_rev\":\"2-42\",\"name\":\"John\"}")]
		[InlineData("{\"_rev\":\"1-42\",\"name\":\"John\"}", "{\"_rev\":\"2-42\",\"name\":\"John\"}")]
		[InlineData("{\"name\":\"John\"}", "{\"_rev\":\"2-42\",\"name\":\"John\"}")]
		[InlineData("{}", "{\"_rev\":\"2-42\"}")]
		public void ShouldInsertRevisionPropertyInSpecificOrder(string initialJson, string expectedJson)
		{
			var document = new Document(initialJson);
			document.Revision = "2-42";
			Assert.Equal(expectedJson, document.ToString());
		}

		[Theory]
		[InlineData("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_id\":\"3849D9BC\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_id\":\"3849D9BC\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_id\":\"8A7FD19B\",\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_id\":\"3849D9BC\",\"type\":\"simpleEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"name\":\"John\"}", "{\"_id\":\"3849D9BC\",\"_rev\":\"1-42\",\"name\":\"John\"}")]
		[InlineData("{\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_id\":\"3849D9BC\",\"type\":\"simpleEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_id\":\"8A7FD19B\",\"name\":\"John\"}", "{\"_id\":\"3849D9BC\",\"name\":\"John\"}")]
		[InlineData("{\"_rev\":\"1-42\",\"name\":\"John\"}", "{\"_id\":\"3849D9BC\",\"_rev\":\"1-42\",\"name\":\"John\"}")]
		[InlineData("{\"name\":\"John\"}", "{\"_id\":\"3849D9BC\",\"name\":\"John\"}")]
		[InlineData("{}", "{\"_id\":\"3849D9BC\"}")]
		public void ShouldInsertIdPropertyInSpecificOrder(string initialJson, string expectedJson)
		{
			var document = new Document(initialJson);
			document.Id = "3849D9BC";
			Assert.Equal(expectedJson, document.ToString());
		}

		[Theory]
		[InlineData("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"complexEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_rev\":\"1-42\",\"type\":\"complexEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_id\":\"8A7FD19B\",\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"_id\":\"8A7FD19B\",\"type\":\"complexEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"name\":\"John\"}", "{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"complexEntity\",\"name\":\"John\"}")]
		[InlineData("{\"type\":\"simpleEntity\",\"name\":\"John\"}", "{\"type\":\"complexEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_id\":\"8A7FD19B\",\"name\":\"John\"}", "{\"_id\":\"8A7FD19B\",\"type\":\"complexEntity\",\"name\":\"John\"}")]
		[InlineData("{\"_id\":\"8A7FD19B\",\"name\":\"John\"}", "{\"_id\":\"8A7FD19B\",\"type\":\"complexEntity\",\"name\":\"John\"}")]
		[InlineData("{\"name\":\"John\"}", "{\"type\":\"complexEntity\",\"name\":\"John\"}")]
		[InlineData("{}", "{\"type\":\"complexEntity\"}")]
		public void ShouldInsertTypePropertyInSpecificOrder(string initialJson, string expectedJson)
		{
			var document = new Document(initialJson);
			document.Type = "complexEntity";
			Assert.Equal(expectedJson, document.ToString());
		}
		
		[Fact]
		public void ShouldLoadJsonDocumentFromTextReader()
		{
			using(var textReader = new StringReader("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}"))
			{
				var document = new Document(textReader);

				Assert.Equal("8A7FD19B", document.Id);
				Assert.Equal("1-42", document.Revision);
				Assert.Equal("simpleEntity", document.Type);
				Assert.Equal("John", (string)((dynamic)document).name);
			}
		}

		[Fact]
		public void ShouldParseJsonDocumentString()
		{
			var document = new Document("{\"_id\":\"8A7FD19B\",\"_rev\":\"1-42\",\"type\":\"simpleEntity\",\"name\":\"John\"}");

			Assert.Equal("8A7FD19B", document.Id);
			Assert.Equal("1-42", document.Revision);
			Assert.Equal("simpleEntity", document.Type);
			Assert.Equal("John", (string)((dynamic)document).name);
		}
	}
}
