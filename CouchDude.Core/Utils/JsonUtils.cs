#region Licence Info 
/*
  Copyright 2011 · Artem Tikhomirov																					
 																																					
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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.Utils
{
	/// <summary>Utility method class.</summary>
	public static class JsonUtils
	{
		/// <summary>Grabs required property value from provided object throwing 
		/// <see cref="ParseException"/> if not found or empty.</summary>
		public static string GetRequiredProperty(this JObject doc, string name, string additionalMessage = null)
		{
			var propertyValue = doc[name] as JValue;
			if (propertyValue == null)
				throw new ParseException(
					"Required field '{0}' have not found on document{1}:\n {2}",
					name,
					additionalMessage == null? string.Empty: ". " + additionalMessage,
					doc.ToString(Formatting.None)
				);
			var value = propertyValue.Value<string>();
			if(string.IsNullOrWhiteSpace(value))
				throw new ParseException("Required field '{0}' is empty", name);

			return value;
		}
	}
}
