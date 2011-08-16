#region Licence Info 
/*
	Copyright 2011 � Artem Tikhomirov																					
																																					
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

namespace CouchDude.Core
{
	/// <summary>CouchDB query common members.</summary>
	public interface IQuery<TRow, TResult>
	{
		/// <summary>Design document name (id without '_design/' prefix) to use view from.</summary>
		string DesignDocumentName { get; set; }

		/// <summary>Indicates that we need documents from couchdb in result.</summary>
		bool IncludeDocs { get; set; }

		/// <summary>Maximum rows should be returned from database.</summary>
		int? Limit { get; set; }

		/// <summary>Rows should be skipped before first being returned.</summary>
		int? Skip { get; set; }

		/// <summary>Query result row to result transform operation. If <c>null</c> default one gets used.</summary>
		Func<IEnumerable<TRow>, IEnumerable<TResult>> ProcessRows { get; set; }

		/// <summary>Expreses query as relative URL.</summary>
		Uri ToUri();
	}
}