using System;
using CouchDude.Core;
using CouchDude.Core.Api;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Impl
{
	public class CouchSessionConstuctorTests
	{
		[Fact]
		public void ShouldThrowOnNullParameters()
		{
			Assert.Throws<ArgumentNullException>(() => new CouchSession(Default.Settings, null));
			Assert.Throws<ArgumentNullException>(() => new CouchSession(null, Mock.Of<ICouchApi>()));
		}

		[Fact]
		public void ShouldThrowOnUnfinishedSettings()
		{
			Assert.Throws<ArgumentException>(() => 
				new CouchSession(new Settings(), Mock.Of<ICouchApi>())
			);
		}
	}
}