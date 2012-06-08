using System.Collections.Generic;
using System.Json;
using CouchDude.Serialization;
using Xunit;

namespace CouchDude.Tests.Regress
{
	public class UnableToCastInt32ValueToString
	{
		public sealed class Object
		{
			public List<string> ArrayOfStrings { get; set; }
			public int Integer { get; set; }
		}

		readonly JsonObject inputObject = new { arrayOfStrings = new[]{"qwe qwe q ewq eq"}, integer = 42 }.ToJsonObject();

		[Fact]
		public void ShouldNotThrow()
		{
			Object serialized = null;
			Assert.DoesNotThrow(() =>
				serialized = (Object)new NewtonsoftSerializer().ConvertFromJson(typeof (Object), inputObject, throwOnError: true)
			);

			Assert.NotNull(serialized);
			Assert.Equal(new[]{"qwe qwe q ewq eq"}, serialized.ArrayOfStrings);
			Assert.Equal(42, serialized.Integer);
		}
	}
}