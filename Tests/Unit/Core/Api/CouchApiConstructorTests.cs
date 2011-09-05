using System;
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class CouchApiConstructorTests
	{
		[Fact]
		public void ShouldThrowOnNullParameters()
		{
			Assert.Throws<ArgumentNullException>(() => new CouchApi(null, new Uri("http://example.com")));
			Assert.Throws<ArgumentNullException>(() => new CouchApi(new HttpClientMock(), null));
		}
	}
}