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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Utils
{
	/// <summary><see cref="JsonWriter"/> implementation writing to <see cref="JsonValue"/> object.</summary>
	public class SystemJsonValueWriter: JTokenWriter
	{
		/// <summary>Returs <see cref="JsonValue"/> object representing JSON written to writer so far.</summary>
		public JsonValue JsonValue
		{
			get 
			{
				// HACK: Change this to proper JsonReader implementation
				switch (Token.Type)
				{
					case JTokenType.None:
					case JTokenType.Null:
					case JTokenType.Undefined:
						return null;
					case JTokenType.Integer:
					case JTokenType.Float:
					case JTokenType.String:
					case JTokenType.Boolean:
					case JTokenType.Date:
					case JTokenType.Bytes:
					case JTokenType.Guid:
					case JTokenType.Uri:
					case JTokenType.TimeSpan:
						JsonPrimitive primitive;
						JsonPrimitive.TryCreate(((JValue) Token).Value, out primitive);
						return primitive;
					default:
						return JsonValue.Parse(Token.ToString());
				}
			}
		}
	}
}