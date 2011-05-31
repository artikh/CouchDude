using CouchDude.Core;
using CouchDude.Core.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Utils
{
	public class JsonUtilsTests
	{
		[Fact]
		public void ShouldParseDocumentInfo()
		{
			var obj = JObject.Parse(@"{ ""some_prop"": ""some value"" }");
			Assert.Equal("some value", obj.GetRequiredProperty("some_prop"));
		}

		[Fact]
		public void ShouldThrowOnMissigProperty()
		{
			var obj = JObject.Parse(@"{  }");
			Assert.Throws<CouchResponseParseException>(() =>
				obj.GetRequiredProperty("some_prop")
			);
		}

		[Fact]
		public void ShouldThrowOnEmptyProperty()
		{
			var obj = JObject.Parse(@"{  ""some_prop"": """"  }");
			Assert.Throws<CouchResponseParseException>(() =>
				obj.GetRequiredProperty("some_prop")
			);
		}

		[Fact]
		public void ShouldThrowOnWhitespaseProperty()
		{
			var obj = JObject.Parse(@"{  ""some_prop"": ""   ""  }");
			Assert.Throws<CouchResponseParseException>(() =>
				obj.GetRequiredProperty("some_prop")
			);
		}
	}
}
