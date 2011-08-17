using System;
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Core.Api
{
	public class CouchApiConstructorTests
	{
		[Fact]
		public void ShouldThrowOnNullParameters()
		{
			Assert.Throws<ArgumentNullException>(() => new CouchApi(null, new Uri("http://example.com"), "db1"));
			Assert.Throws<ArgumentNullException>(() => new CouchApi(new HttpClientMock(), null, "db1"));
			Assert.Throws<ArgumentNullException>(() => new CouchApi(new HttpClientMock(), new Uri("http://example.com"), null));
			Assert.Throws<ArgumentNullException>(() => new CouchApi(new HttpClientMock(), new Uri("http://example.com"), string.Empty));
		}
	}
}