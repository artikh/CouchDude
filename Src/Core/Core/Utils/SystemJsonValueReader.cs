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

using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Utils
{
	/// <summary><see cref="JsonReader"/> implementation reading <see cref="JsonValue"/> object.</summary>
	public class SystemJsonValueReader : JTokenReader 
	{
		/// <constructor />
		public SystemJsonValueReader(JsonValue jsonValue) : base(ConvertToJToken(jsonValue)) { }

		private static JToken ConvertToJToken(JsonValue jsonValue)
		{
			// HACK: Change this to proper JsonReader implementation
			return JToken.Parse(jsonValue.ToString());
		}
	}
}
