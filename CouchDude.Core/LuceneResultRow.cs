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
	/// <summary>CouchDB-lucene query result row.</summary>
	public class LuceneResultRow: ViewResultRow
	{
		/// <constructor />
		public LuceneResultRow() { }

		/// <constructor />
		public LuceneResultRow(JsonFragment fields, decimal score, string documentId, IDocument document)
			: base(null, fields, documentId, document)
		{
			Score = score;
			Fields = fields;
		}

		/// <summary>The normalized score (0.0-1.0, inclusive) for this match.</summary>
		public decimal Score { get; private set; }

		/// <summary>All the fields that were stored with this match</summary>
		public IDynamicMetaObjectProvider Fields { get; private set; }
	}
}