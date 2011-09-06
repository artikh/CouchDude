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

namespace CouchDude
{
	/// <summary>Typed CouchDB view query result.</summary>
	public interface ILuceneQueryResult<out T> : ILuceneQueryResult, IQueryResult<T, LuceneResultRow> { }

	/// <summary>CouchDB view query result.</summary>
	public interface ILuceneQueryResult : IQueryResult<LuceneResultRow>
	{
		/// <summary>Time spent retrieving documents.</summary>
		TimeSpan FetchDuration { get; }

		/// <summary>Time spent performing the search.</summary>
		TimeSpan SearchDuration { get; }

		/// <summary>Effective limit revised by couchdb-lucene.</summary>
		int Limit { get; }

		/// <summary>Number of initial matches skipped.</summary>
		int Skip { get; }

		/// <summary>Query used to produce current results set.</summary>
		LuceneQuery Query { get; }

		/// <summary>Returns next page query or <c>null</c> if instance represents last page of results.</summary>
		LuceneQuery NextPageQuery { get; }
	}
}