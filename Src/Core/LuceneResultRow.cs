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

namespace CouchDude
{
	/// <summary>CouchDB-lucene query result row.</summary>
	public class LuceneResultRow : IQueryResultRow
	{
		/// <constructor />
		public LuceneResultRow() { }

		/// <constructor />
		public LuceneResultRow(string id, IJsonFragment fields, decimal score, string documentId, IDocument document)
		{
			Id = id;
			Score = score;
			Fields = fields;
			DocumentId = documentId;
			Document = document;
		}

		/// <summary>The unique identifier for this match.</summary>
		public string Id { get; private set; }

		/// <summary>The normalized score (0.0-1.0, inclusive) for this match.</summary>
		public decimal Score { get; private set; }

		/// <summary>All the fields that were stored with this match</summary>
		public IJsonFragment Fields { get; private set; }

		IJsonFragment IQueryResultRow.Value { get { return Fields; } }

		/// <summary>Document ID associated with view row.</summary>
		public string DocumentId { get; private set; }

		/// <summary>Document associated with the row.</summary>
		public IDocument Document { get; private set; }
	}
}