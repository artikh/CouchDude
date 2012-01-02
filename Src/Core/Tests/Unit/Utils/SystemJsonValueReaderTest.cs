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
		[InlineData(@"{""null key"":null}")]
		[InlineData(@"[]")]
		[InlineData(@"[null]")]
		[InlineData(@"{""key A"":""value A""}")]
		[InlineData(@"{""key A"":""value A"",""keyB"":null,""keyC"":true}")]
		[InlineData(@"[1,true,""42"",null,0.048]")]
		[InlineData(@"{""key A"":""value A"",""key B"":[1,true,""42""]}")]
		[InlineData(@"[""value A"",[1,true,""42"",null,0.048]]")]
		[InlineData(@"42")]
		[InlineData(@"""42""")]
		[InlineData(@"true")]
		[InlineData(@"false")]
		[InlineData(@"0.048")]
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