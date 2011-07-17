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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace CouchDude.Core.Api
{
	/// <summary>Result of CouchDB view query.</summary>
	public class LuceneResult
	{
		/// <constructor />
		public LuceneResult()
		{
			Rows = new List<LuceneResultRow>();
		}

		/// <summary>Query used to produce result.</summary>
		[JsonIgnore]
		public LuceneQuery Query { get; internal set; }

		/// <summary>Total rows in requested range.</summary>
		[JsonProperty("total_rows")]
		public int TotalRows { get; internal set; }
		
		/// <summary>Selected rows.</summary>
		[JsonProperty("rows")]
		public IList<LuceneResultRow> Rows { get; protected set; }
	}
}
