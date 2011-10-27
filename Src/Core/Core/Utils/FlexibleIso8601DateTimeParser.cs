using System;
using System.Globalization;

namespace CouchDude.Utils
{
	/// <summary>Flexible ISO8601-like date/time string parser.</summary>
	public static class FlexibleIso8601DateTimeParser
	{
		/// <summary>Attemts to parse date/time string returning <c>null</c> if operation was unsuccessfull.</summary>
		public static DateTimeOffset? TryParse(string dateTimeString)
		{
			DateTimeOffset result;
			if (!string.IsNullOrEmpty(dateTimeString))
				if (DateTimeOffset.TryParse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result))
					return result;
			return null;
		}
	}
}
