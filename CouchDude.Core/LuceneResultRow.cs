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

namespace CouchDude.Core
{
	/// <summary>CouchDB-lucene query result row.</summary>
	public class LuceneResultRow
	{
		/// <summary>The unique identifier for this match.</summary>
		[JsonProperty("id")]
		public string DocumentId { get; protected internal set; }

		/// <summary>All the fields that were stored with this match</summary>
		[JsonProperty("fields")]
		public JsonFragment Fields { get; protected internal set; }

		/// <summary>The normalized score (0.0-1.0, inclusive) for this match.</summary>
		[JsonProperty("score")]
		public decimal Score { get; protected internal set; }

		/// <summary>Document associated with the row.</summary>
		[JsonProperty("doc")]
		public Document Document { get; protected internal set; }
	}
}