using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace CouchDude.Core.Utils
{
	/// <summary>Utility methods for string manipulation.</summary>
	public static class StringUtils
	{
		/// <summary>Converts first letter of the string to </summary>
		public static string ToCamelCase(this string self)
		{
			if(string.IsNullOrEmpty(self)) throw new ArgumentNullException("self");
			Contract.EndContractBlock();

			var firstLetter = self[0];
			if (!Char.IsUpper(firstLetter))
				return self;

			var output = new StringBuilder(self);
			output[0] = Char.ToLower(firstLetter);
			return output.ToString();
		}
	}
}
