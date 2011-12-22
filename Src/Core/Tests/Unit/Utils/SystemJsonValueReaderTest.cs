using System.Json;
using CouchDude.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Utils
{
	public class SystemJsonValueReaderTest
	{
		[Theory]
		[InlineData(@"{}")]
		[InlineData(@"[]")]
		[InlineData(@"{""key A"":""value A""}")]
		[InlineData(@"[1,true,""42""]")]
		[InlineData(@"{""key A"":""value A"",""key B"":[1,true,""42""]}")]
		[InlineData(@"42")]
		[InlineData(@"""42""")]
		[InlineData(@"true")]
		[InlineData(@"false")]
		public void ShouldReadJsonAsIs(string jsonString)
		{
			var value = JsonValue.Parse(jsonString);

			using (var reader = new SystemJsonValueReader(value))
			{
				var jToken = JToken.Load(reader);
				Assert.Equal(jsonString, jToken.ToString(Formatting.None));
			}	
		}
	}
}