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
using System.Collections.Concurrent;

namespace CouchDude.Utils
{
	/// <summary>Wrapper over <see cref="ConcurrentDictionary{TValue,TKey}"/></summary>
	public class LazyConcurrentDictionary<TKey, TValue>
	{
		readonly ConcurrentDictionary<TKey, TValue> innerDic = new ConcurrentDictionary<TKey, TValue>();
		readonly Func<TKey, TValue> factory;

		/// <constructor />
		public LazyConcurrentDictionary(Func<TKey, TValue> factory) { this.factory = factory; }

		/// <summary>Gets value for the key invoking factory if needed.</summary>
		public TValue Get(TKey key) { return innerDic.GetOrAdd(key, factory); }
	}
}