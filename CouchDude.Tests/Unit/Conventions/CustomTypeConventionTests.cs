using CouchDude.Core.Conventions;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Unit.Conventions
{
	public class CustomTypeConventionTests
	{
		[Fact]
		public void ShouldConvertTypeNameToCamelCase()
		{
			var convention = new CustomTypeConvention(
				new[] { typeof(SimpleEntity).Assembly }, 
				baseTypes: new[] { typeof(SimpleEntity) },
				createDocumentTypeFromEntityType: t => "testString");
			convention.Init();
			Assert.Equal("testString", convention.GetDocumentType(typeof(SimpleEntity)));
		}
	}
}