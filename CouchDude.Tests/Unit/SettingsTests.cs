using System;
using CouchDude.Core;
using Xunit;

namespace CouchDude.Tests.Unit
{
	public class SettingsTests
	{
		[Fact]
		public void ShouldThrowOnUppercasedDbName()
		{
			Assert.Throws<ArgumentException>(
				() => new Settings(new Uri("http://example.com"), "UpprecasedName"));
		}

		[Fact]
		public void ShouldNotThrowOnIncorrectCharDbName()
		{
			Assert.Throws<ArgumentException>(
				() => new Settings(new Uri("http://example.com"), "name_with*in_the_middle"));
		}

		[Fact]
		public void ShouldNotThrowOnValidDbName()
		{
			Assert.DoesNotThrow(() => new Settings(new Uri("http://example.com"), "a0-9a-z_$()+-/"));
		}
	}
}
