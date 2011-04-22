using System;
using CouchDude.Core;
using CouchDude.Core.Conventions;
using CouchDude.Core.Initialization;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Unit.Initialization
{
	public class FluentConfigurationTests
	{
		[Fact]
		public void ShouldSetServerUriAndDatabaseName()
		{
			var settings = Configure.With().ServerUri("http://example.com").DatabaseName("db1").CreateSettings();

			Assert.Equal(new Uri("http://example.com"), settings.ServerUri);
			Assert.Equal("db1", settings.DatabaseName);
		}

		[Fact]
		public void ShouldThrowOnIncompleteInitialization()
		{
			Assert.Throws<ConfigurationException>(() => Configure.With().ServerUri("http://example.com").CreateSettings());
			Assert.Throws<ConfigurationException>(() => Configure.With().DatabaseName("db1").CreateSettings());
		}

		[Fact]
		public void ShouldCreateCamelCaseTypeNameConvention()
		{
			var settings = Configure.With()
				.ServerUri("http://example.com")
				.DatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<SimpleEntity>()
					.InheritingFrom<SimpleEntity>()
					.ToDocumentTypeCamelCase()
				.CreateSettings();
			
			Assert.IsType<CamelCaseTypeNameToConvention>(settings.TypeConvension);
		}

		[Fact]
		public void ShouldCreateTypeNameTypeConvention()
		{
			var settings = Configure.With()
				.ServerUri("http://example.com")
				.DatabaseName("db1")
				.MappingEntities()
					.FromAssemblyOf<SimpleEntity>()
					.InheritingFrom<SimpleEntity>()
					.ToDocumentTypePascalCase()
				.CreateSettings();

			Assert.IsType<TypeNameAsIsTypeConvention>(settings.TypeConvension);
		}
	}
}