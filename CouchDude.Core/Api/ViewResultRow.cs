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

namespace CouchDude.Core.Api
{
	/// <summary>CouchDB query result row.</summary>
	public class ViewResultRow
	{
		/// <summary>View key.</summary>
		[JsonProperty("key")]
		public JToken Key { get; protected internal set; }

		/// <summary>View value.</summary>
		[JsonProperty("value")]
		public JToken Value { get; protected internal set; }

		/// <summary>Document ID associated with view row.</summary>
		[JsonProperty("id")]
		public string DocumentId { get; protected internal set; }

		/// <summary>Document associated with the row.</summary>
		[JsonProperty("doc")]
		public JObject Document { get; protected internal set; }
	}
}