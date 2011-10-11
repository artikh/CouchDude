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

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Tests
{
	public class JTokenStringCompairer: IEqualityComparer<string>
	{
		public bool Equals(string x, string y)
		{
			var xToken = Parse(x);
			var yToken = Parse(y);
			return JToken.DeepEquals(xToken, yToken);
		}

		private static JToken Parse(string str)
		{
			using (var reader = new StringReader(str))
			using (var jsonReader = new JsonTextReader(reader))
				return JToken.ReadFrom(jsonReader);
		}

		public int GetHashCode(string obj)
		{
			return JObject.Parse(obj).GetHashCode();
		}
	}
}