using System;
using System.Collections.Generic;
using System.Reflection;
using CouchDude.Core;
using CouchDude.Core.Conventions;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Conventions
{
	public class TypeConventionBaseTests
	{
		class ConventionMock : TypeConventionBase
		{
			private readonly Func<Type, string> getDocType;

			public ConventionMock(IEnumerable<Assembly> assembliesToScan = null, ICollection<Type> baseTypes = null, Func<Type, string> getDocType = null)
				: base(assembliesToScan, baseTypes)
			{
				this.getDocType = getDocType ?? (t => null);
			}

			
			protected internal override string CreateDocumentTypeFromEntityType(Type entityType)
			{
				return getDocType(entityType);
			}
		}

		[Fact]
		public void ShouldReturnNullIfNoProcessingHaveBeenDone()
		{
			var convention = new ConventionMock(assembliesToScan: new[] { typeof(SimpleEntity).Assembly });
			convention.Init();

			Assert.Null(convention.GetDocumentType(typeof(SimpleEntity)));
			Assert.Null(convention.GetEntityType("simpleEntity"));
			Assert.Null(convention.GetEntityType("SimpleEntity"));
		}
		
		[Fact]
		public void ShouldReturnProcessedDocumentType()
		{
			var convention = new ConventionMock(
				assembliesToScan: new[] { typeof(SimpleEntity).Assembly },
				getDocType: type => type == typeof (SimpleEntity) ? "test" : null);
			convention.Init();

			Assert.Equal("test", convention.GetDocumentType(typeof(SimpleEntity)));
			Assert.Equal(convention.GetEntityType("test"), typeof(SimpleEntity));
		}

		[Fact]
		public void ShouldNotScanAnythingIfNullAssemblyListProvided()
		{
			var convention = new ConventionMock(assembliesToScan: null, baseTypes: null);
			convention.Init();
			Assert.Null(convention.GetDocumentType(typeof(SimpleEntity)));
		}

		[Fact]
		public void ShouldNotScanAnythingIfEmptyAssemblyListProvided()
		{
			var convention = new ConventionMock(new Assembly[0], baseTypes: null);
			convention.Init();
			Assert.Null(convention.GetDocumentType(typeof(SimpleEntity)));
		}
		
		[Fact]
		public void ShouldThrowOnSameTypeName()
		{
			var conventionMock = new ConventionMock(new[] { typeof(Base).Assembly }, getDocType: t => "same string all over again");

			Assert.Throws<ConventionException>(() => conventionMock.Init());
		}
	}

	internal class Base { }
	internal class A : Base { }
	namespace B
	{
		internal class A : Base { }
	}
}