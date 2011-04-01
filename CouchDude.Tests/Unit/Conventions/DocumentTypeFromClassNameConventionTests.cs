using CouchDude.Core.Conventions;
using Xunit;

namespace CouchDude.Tests.Unit.Conventions
{
	public class TestConventionClass { }

	public class DocumentTypeFromClassNameConventionTests
	{
		[Fact]
		public void ShouldProvideLowerCasedTypeNameAsDocType()
		{
			var docName = new DocumentTypeFromClassNameConvention().GetType(typeof(TestConventionClass));
			Assert.Equal("testConventionClass", docName);
		}
	}
}