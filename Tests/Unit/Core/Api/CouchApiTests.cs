using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class CouchApiTests
	{
		[Fact]
		public void ShouldThrowOnNullEmptyOrWhitespaceDbName()
		{
			var couchApi = Factory.CreateCouchApi("http://example.com");
			Assert.Throws<ArgumentNullException>(() => couchApi.Db(null));
			Assert.Throws<ArgumentNullException>(() => couchApi.Db(string.Empty));
			Assert.Throws<ArgumentNullException>(() => couchApi.Db("   "));
		}

		[Theory]
		[InlineData("DB")]
		[InlineData("_db1")]
		[InlineData("$db1")]
		[InlineData("1db1")]
		[InlineData("(a)db1")]
		[InlineData(")b(db1")]
		[InlineData("[db1")]
		[InlineData("+db1")]
		[InlineData("-db1")]
		public void ShouldThrowOnInvalidDbName(string invalidDbName)
		{
			var couchApi = Factory.CreateCouchApi("http://example.com");
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => couchApi.Db(invalidDbName));
			Assert.Equal(invalidDbName, exception.ActualValue as string);
		}

		[Theory]
		[InlineData("adb")]
		[InlineData("a_db1")]
		[InlineData("a$db1")]
		[InlineData("a1db1")]
		[InlineData("a(a)db1")]
		[InlineData("a)b(db1")]
		[InlineData("a+db1")]
		[InlineData("a-db1")]
		public void ShouldNotThrowOnValidDbName(string validDbName)
		{
			var couchApi = Factory.CreateCouchApi("http://example.com");
			Assert.DoesNotThrow(() => couchApi.Db(validDbName));
		}
	}
}
