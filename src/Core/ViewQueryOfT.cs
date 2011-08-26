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

using System;
using System.Collections.Generic;

namespace CouchDude
{
	/// <summary>Describes typed CouchDB view query.</summary>
	public class ViewQuery<T> : ViewQuery, IQuery<ViewResultRow, T>
	{
		/// <summary>Restores view query from provided URI ignoring it if malformed.</summary>
		public ViewQuery(Uri uri) : base(uri) {}

		/// <summary>Restores view query from provided URI string ignoring it if malformed.</summary>
		public ViewQuery(string uriString) : base(uriString) {}

		/// <constructor />
		public ViewQuery() {}

		/// <summary>Processes raw query result producing meningfull results.</summary>
		public Func<IEnumerable<ViewResultRow>, IEnumerable<T>> ProcessRows { get; set; }
	}
}