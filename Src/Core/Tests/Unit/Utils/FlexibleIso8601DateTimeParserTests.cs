using System;
using CouchDude.Utils;
using Xunit;

namespace CouchDude.Tests.Unit.Utils
{
	public class FlexibleIso8601DateTimeParserTests
	{
		[Fact]
		public void ShouldParseCorrectDateTimeWithOffset()
		{
			TestParse(
				new DateTimeOffset(2011, 10, 27, 7, 7, 16, 0, TimeSpan.FromHours(4)).AddMilliseconds(521.8705),
				"2011-10-27T07:07:16.5218705+04");
		}

		[Fact]
		public void ShouldParseCorrectDateTimeWithLongOffset()
		{
			TestParse(
				new DateTimeOffset(2011, 10, 27, 7, 7, 16, 0, TimeSpan.FromHours(4)).AddMilliseconds(521.8705),
				"2011-10-27T07:07:16.5218705+04:00");
		}

		[Fact]
		public void ShouldParseCorrectDateTimeWithZ()
		{
			TestParse(
				new DateTimeOffset(2011, 10, 27, 7, 7, 16, 0, TimeSpan.Zero).AddMilliseconds(521.8705),
				"2011-10-27T07:07:16.5218705Z");
		}

		[Fact]
		public void ShouldParseCorrectDateTimeWithoutOffsetAsUtc()
		{
			TestParse(
				new DateTimeOffset(2011, 10, 27, 7, 7, 16, 0, TimeSpan.Zero).AddMilliseconds(521.8705),
				"2011-10-27T07:07:16.5218705");
		}

		[Fact]
		public void ShouldParseSpacedDateTimeWithOffset()
		{
			TestParse(
				new DateTimeOffset(2011, 10, 27, 7, 7, 16, 0, TimeSpan.FromHours(4)).AddMilliseconds(521.8705),
				"2011-10-27 07:07:16.5218705 +04");
		}

		[Fact]
		public void ShouldParseSpacedDateTimeWithZ()
		{
			TestParse(
				new DateTimeOffset(2011, 10, 27, 7, 7, 16, 0, TimeSpan.Zero).AddMilliseconds(521.8705),
				"2011-10-27 07:07:16.5218705 Z");
		}

		[Fact]
		public void ShouldParseSpacedDateTimeWithoutOffsetAsUtc()
		{
			TestParse(
				new DateTimeOffset(2011, 10, 27, 7, 7, 16, 0, TimeSpan.Zero).AddMilliseconds(521.8705),
				"2011-10-27 07:07:16.5218705");
		}
		
		[Fact]
		public void ShouldParseZeroDateTimeWithoutOffset()
		{
			TestParse(default(DateTimeOffset), "0001-01-01T00:00:00.0000000");
		}

		[Fact]
		public void ShouldParseZeroDateTimeWithZeroOffset()
		{
			TestParse(default(DateTimeOffset), "0001-01-01T00:00:00.0000000+00");
		}

		[Fact]
		public void ShouldParseZeroDateTimeWithZOffset()
		{
			TestParse(default(DateTimeOffset), "0001-01-01T00:00:00.0000000Z");
		}

		[Fact]
		public void ShouldParseZeroDateTimeWithLongOffset()
		{
			TestParse(default(DateTimeOffset), "0001-01-01T00:00:00.0000000+00:00");
		}

		[Fact]
		public void ShouldParseZeroDateTimeWithNegativeOffset()
		{
			TestParse(new DateTimeOffset(default(DateTime), TimeSpan.FromHours(-3)), "0001-01-01T00:00:00.0000000-03:00");
		}

		private void TestParse(DateTimeOffset expectedResult, string dateTimeString)
		{
			var result = FlexibleIso8601DateTimeParser.TryParse(dateTimeString);
			Assert.NotNull(result);
			Assert.Equal(expectedResult.Offset,                  result.Value.Offset);
			Assert.Equal(expectedResult.UtcDateTime.Year,        result.Value.UtcDateTime.Year);
			Assert.Equal(expectedResult.UtcDateTime.Month,       result.Value.UtcDateTime.Month);
			Assert.Equal(expectedResult.UtcDateTime.Day,         result.Value.UtcDateTime.Day);
			Assert.Equal(expectedResult.UtcDateTime.Hour,        result.Value.UtcDateTime.Hour);
			Assert.Equal(expectedResult.UtcDateTime.Minute,      result.Value.UtcDateTime.Minute);
			Assert.Equal(expectedResult.UtcDateTime.Second,      result.Value.UtcDateTime.Second);
			Assert.True(Math.Abs(expectedResult.UtcDateTime.Millisecond - result.Value.UtcDateTime.Millisecond) <= 1);
		}
	}
}
