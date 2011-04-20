using System;
using System.Reflection;
using CouchDude.Core.Conventions;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Conventions
{
	public class TypeConventionBaseTests
	{
		private readonly Assembly[][] assemblies = new[] {new[] { Assembly.GetExecutingAssembly() }};

		[Fact]
		public void ShouldReturnNullIfNoProcessingHaveBeenDone()
		{
			var convention = new Mock<TypeConventionBase>(MockBehavior.Loose, assemblies).Object;
			convention.Init();

			Assert.Null(convention.GetDocumentType(typeof(SimpleEntity)));
			Assert.Null(convention.GetEntityType("simpleEntity"));
		}
		
		[Fact]
		public void ShouldReturnProcessedDocumentType()
		{
			var conventionMock = new Mock<TypeConventionBase>(assemblies);
			conventionMock
				.Setup(c => c.ProcessType(It.IsAny<Type>()))
				.Returns<Type>(type => type == typeof (SimpleEntity) ? "simpleEntity" : null);

			var convention = conventionMock.Object;
			convention.Init();

			Assert.Equal("simpleEntity", convention.GetDocumentType(typeof(SimpleEntity)));
			Assert.Equal(convention.GetEntityType("simpleEntity"), typeof(SimpleEntity));
		}
	}
}