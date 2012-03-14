using CouchDude.Utils;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Utils
{
	public class SystemJsonValueWriterTest
	{
		[Theory]
		[InlineData(@"{}")]
		[InlineData(@"{""null key"":null}")]
		[InlineData(@"[]")]
		[InlineData(@"[null]")]
		[InlineData(@"{""key A"":""value A""}")]
		[InlineData(@"{""key A"":""value A"",""keyB"":null}")]
		[InlineData(@"[1,true,""42"",null,0.048]")]
		[InlineData(@"{""key A"":""value A"",""key B"":[1,true,""42""]}")]
		[InlineData(@"[""value A"",[1,true,""42"",null,0.048]]")]
		[InlineData(@"42")]
		[InlineData(@"""42""")]
		[InlineData(@"true")]
		[InlineData(@"false")]
		[InlineData(@"0.048")]
		public void ShouldWriteJsonAsIs(string jsonString)
		{
			var jToken = JToken.Parse(jsonString);
			using(var writer = new SystemJsonValueWriter())
			{
				jToken.WriteTo(writer);
				Assert.Equal(jsonString, writer.JsonValue.ToString());
			}
		}
		
		[Fact]
		public void ShouldWriteNull()
		{
			using (var writer = new SystemJsonValueWriter())
			{
				JToken.Parse("null").WriteTo(writer);
				Assert.Null(writer.JsonValue);
			}
		}
	}
}