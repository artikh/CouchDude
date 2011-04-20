using System.Reflection;
using CouchDude.Core;
using CouchDude.Core.Conventions;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Unit.Conventions
{
	public class TypeNameTypeConventionTests
	{
		[Fact]
		public void ShouldNotScanAnythingIfNullAssemblyListProvided()
		{
			var convention = new TypeNameTypeConvention(assembliesToScan: null);
			convention.Init();
			Assert.Null(convention.GetDocumentType(typeof(SimpleEntity)));
		}

		[Fact]
		public void ShouldNotScanAnythingIfEmptyAssemblyListProvided()
		{
			var convention = new TypeNameTypeConvention(assembliesToScan: new Assembly[0]);
			convention.Init();
			Assert.Null(convention.GetDocumentType(typeof(SimpleEntity)));
		}

		[Fact]
		public void ShouldNotScanIfNoBaseClass()
		{
			var convention = new TypeNameTypeConvention(
				new[] { Assembly.GetExecutingAssembly() }, baseTypes: new[] { typeof(SimpleEntity) });
			convention.Init();
			Assert.Equal("SimpleEntity", convention.GetDocumentType(typeof(SimpleEntity)));
		}
		
		[Fact]
		public void ShouldThrowOnSameTypeName()
		{
			var convention = new TypeNameTypeConvention(
				new[] { Assembly.GetExecutingAssembly() }, baseTypes: new[] { typeof(Base) });

			Assert.Throws<ConventionException>(() => convention.Init());
		}
	}

	internal class Base { }
	internal class A : Base { }
	namespace B
	{
		internal class A : Base { }
	}
}