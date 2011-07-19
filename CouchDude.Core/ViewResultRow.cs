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

using System.Dynamic;
using CouchDude.Core.Api;

namespace CouchDude.Core
{
	/// <summary>CouchDB query result row.</summary>
	public class ViewResultRow
	{
		/// <constructor />
		public ViewResultRow() { }

		/// <constructor />
		public ViewResultRow(IDynamicMetaObjectProvider key, JsonFragment value, string documentId, IDocument document)
		{
			Key = key;
			Value = value;
			DocumentId = documentId;
			Document = document;
		}

		/// <summary>View key.</summary>
		public IDynamicMetaObjectProvider Key { get; private set; }

		/// <summary>View value.</summary>
		public JsonFragment Value { get; private set; }

		/// <summary>Document ID associated with view row.</summary>
		public string DocumentId { get; private set; }

		/// <summary>Document associated with the row.</summary>
		public IDocument Document { get; private set; }
	}
}