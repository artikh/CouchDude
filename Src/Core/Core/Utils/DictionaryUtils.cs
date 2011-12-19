using System.Collections.Generic;

namespace CouchDude.Utils
{
	/// <summary><see cref="IDictionary{TKey, TValue}"/>-related utility method.</summary>
	public static class DictionaryUtils
	{
		/// <summary>Inline version of <see cref="IDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/> method.</summary>
		public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue defaultValue = default(TValue))
		{
// ReSharper disable RedundantAssignment
			var value = defaultValue;
// ReSharper restore RedundantAssignment
			self.TryGetValue(key, out value);
			return value;
		}
	}
}
