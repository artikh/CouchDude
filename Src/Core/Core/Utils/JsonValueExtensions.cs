#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																				
																																					
	Licensed under the Apache License, Version 2.0 (the "License");					
	you may not use this file except in compliance with the License.					
	You may obtain a copy of the License at																	
																																					
	    http://www.apache.org/licenses/LICENSE-2.0														
																																					
	Unless required by applicable law or agreed to in writing, software			
	distributed under the License is distributed on an "AS IS" BASIS,				
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
	See the License for the specific language governing permissions and			
	limitations under the License.																						
*/
#endregion

using System.Json;

namespace CouchDude.Utils
{
	/// <summary>Utility methods for classes in <see cref="System.Json"/> namespace.</summary>
	public static class JsonValueExtensions
	{
		/// <summary>Clones JSON value.</summary>
		public static JsonValue DeepClone(this JsonValue jsonValue)
		{
			// HACK: Implement proper visitor here
			return JsonValue.Parse(jsonValue.ToString(JsonSaveOptions.None));
		}

		/// <summary>Clones JSON value.</summary>
		public static JsonObject DeepClone(this JsonObject jsonObject)
		{
			return (JsonObject) DeepClone((JsonValue) jsonObject);
		}

		/// <summary>Returns primitive value of the property or defalut value.</summary>
		public static T GetPrimitiveProperty<T>(this JsonObject self, string propertyName, T defaultValue = default(T))
		{
			JsonValue propertyValue;
			if (self.TryGetValue(propertyName, out propertyValue))
			{
				var jsonPrimitive = propertyValue as JsonPrimitive;
				if (jsonPrimitive != null)
				{
					var value = jsonPrimitive.Value;
					if (value is T)
						return (T) value;
				}
			}
			return defaultValue;
		}
	}
}
