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
using System.Linq;

namespace CouchDude.Core
{
	/// <summary>Describes typed CouchDB view query.</summary>
	public class ViewQuery<T>: ViewQuery
	{
		/// <summary>Processes raw query result producing meningfull results.</summary>
		public readonly Func<IEntityConfigRepository, IEnumerable<ViewResultRow>, IEnumerable<T>> ProcessRows = ProcessResultDefault;

		private static IEnumerable<T> ProcessResultDefault(IEntityConfigRepository entityConfigRepository, IEnumerable<ViewResultRow> rawViewResults)
		{
			var entityConfig = entityConfigRepository.TryGetConfig(typeof(T));
			if(entityConfig != null)
			{
				return 
					from row in rawViewResults
					select row.Document into document
					select document == null ? default(T) : (T)document.TryDeserialize(entityConfig);
			}
			else
				return
					from row in rawViewResults
					select row.Value into value
					select value == null ? default(T) : (T)value.TryDeserialize(typeof(T));
		}
	}
}