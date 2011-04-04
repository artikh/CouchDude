using System.IO;
using CouchDude.Core.Implementation;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchApiParseErrorResponseBodyTests
	{
		[Fact]
		public void ShouldReturnNullWhenParsingIncorretResultBody()
		{
			using (var textReader = new StringReader("Some none-JSON string"))
				Assert.Null(CouchApi.ParseErrorResponseBody(textReader));
		}

		[Fact]
		public void ShouldReturnOnlyErrorIfNoReason()
		{
			using (var textReader = new StringReader(@"{ ""error"": ""some error name"" }"))
				Assert.Equal("some error name", CouchApi.ParseErrorResponseBody(textReader));
		}

		[Fact]
		public void ShouldReturnOnlyReasonIfNoError()
		{
			using (var textReader = new StringReader(@"{ ""reason"": ""some reason message"" }"))
				Assert.Equal("some reason message", CouchApi.ParseErrorResponseBody(textReader));
		}

		[Fact]
		public void ShouldReturnOnlyErrorAndReasonIfBothPresent()
		{
			using (var textReader = new StringReader(
				@"{ ""error"": ""some error name"", ""reason"": ""some reason message"" }"))
				Assert.Equal(
					"some error name: some reason message",
					CouchApi.ParseErrorResponseBody(textReader));
		}

	}
}
