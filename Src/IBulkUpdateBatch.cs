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
	/// <summary>Bulk update changes summary interface.</summary>
	public interface IBulkUpdateBatch
	{
		/// <summary>Requires provided document to be updated.</summary>
		void Update(Document document);

		/// <summary>Requires provided document to be created.</summary>
		void Create(Document document);

		/// <summary>Requires provided document to be deleted.</summary>
		void Delete(Document document);

		/// <summary>Requires document of provided ID and revision to be deleted.</summary>
		void Delete(string documentId, string revision);
	}
}