using CouchDude.Core.Conventions;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Unit.Conventions
{
	public class CamelCaseTypeNameToConventionTests
	{
		[Fact]
		public void ShouldConvertTypeNameToCamelCase()
		{
			var convention = new TypeNameAsIsTypeConvention(
				new[] { typeof(SimpleEntity).Assembly }, baseTypes: new[] { typeof(SimpleEntity) });
			convention.Init();
			Assert.Equal("SimpleEntity", convention.GetDocumentType(typeof(SimpleEntity)));
		}
	}
}