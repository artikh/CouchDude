using System.IO;
using CouchDude.Core;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit
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
