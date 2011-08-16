﻿#region Licence Info 
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
using System.Linq;
using System.Text;

namespace CouchDude.Core
{
	/// <summary>Typed lucene-couchdb query.</summary>
	public class LuceneQuery<T> : LuceneQuery, IQuery<LuceneResultRow, T>
	{
		/// <summary>Processes raw query result producing meningfull results.</summary>
		public Func<IEnumerable<LuceneResultRow>, IEnumerable<T>> ProcessRows { get; set; }
	}
}
