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

namespace CouchDude
{
	/// <summary>Types supported by Lucene engine.</summary>
	public enum LuceneType
	{
		/// <summary>Single persision fload-point number.</summary>
		Float,
		/// <summary>Double persision fload-point number.</summary>
		Double,
		/// <summary>Single persision integer number.</summary>
		Int,
		/// <summary>Double persision integer number.</summary>
		Long,
		/// <summary>Date/time number.</summary>
		Date
	}
}