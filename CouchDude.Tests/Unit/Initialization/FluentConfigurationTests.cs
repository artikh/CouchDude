using System;
using CouchDude.Core;
using CouchDude.Core.Configuration;
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
			var settings = ConfigureCouchDude.With().ServerUri("http://example.com").DatabaseName("db1").CreateSettings();

			Assert.Equal(new Uri("http://example.com"), settings.ServerUri);
			Assert.Equal("db1", settings.DatabaseName);
		}

		[Fact]
		public void ShouldThrowOnIncompleteInitialization()
		{
			Assert.Throws<ConfigurationException>(
				() => ConfigureCouchDude.With().ServerUri("http://example.com").CreateSettings());
			Assert.Throws<ConfigurationException>(() => ConfigureCouchDude.With().DatabaseName("db1").CreateSettings());
		}
	}
}