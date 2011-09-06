using System;
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class CouchApiFactoryTests
	{
		[Fact]
		public void ShouldThrowOnNullParameters()
		{
			Assert.Throws<ArgumentNullException>(() => Factory.CreateCouchApi(""));
			Assert.Throws<ArgumentNullException>(() => Factory.CreateCouchApi((Uri)null));
			Assert.Throws<UriFormatException>(() => Factory.CreateCouchApi(new Uri("/some/relative/uri")));
			Assert.Throws<UriFormatException>(() => Factory.CreateCouchApi(new Uri("/some malformed uri")));
		}
	}
}