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
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal class SynchronousCouchApi : ISynchronousCouchApi
	{
		private readonly ICouchApi couchApi;

		/// <constructor />
		public SynchronousCouchApi(ICouchApi couchApi)
		{
			this.couchApi = couchApi;
		}

		public IDocument RequestDocumentById(string databaseName, string docId)
		{
			return couchApi.RequestDocumentById(databaseName, docId).WaitForResult();
		}

		public DocumentInfo DeleteDocument(string databaseName, string docId, string revision)
		{
			return couchApi.DeleteDocument(databaseName, docId, revision).WaitForResult();
		}

		public DocumentInfo SaveDocument(string databaseName, IDocument document)
		{
			return couchApi.SaveDocument(databaseName, document).WaitForResult();
		}

		public IDictionary<string, DocumentInfo> BulkUpdate(string databaseName, Action<IBulkUpdateBatch> updateCommandBuilder)
		{
			return couchApi.BulkUpdate(databaseName, updateCommandBuilder).WaitForResult();
		}

		public string RequestLastestDocumentRevision(string databaseName, string docId)
		{
			return couchApi.RequestLastestDocumentRevision(databaseName, docId).WaitForResult();
		}

		/// <inheritdoc/>
		public IViewQueryResult Query(string databaseName, ViewQuery query)
		{
			return couchApi.Query(databaseName, query).WaitForResult();
		}

		/// <inheritdoc/>
		public ILuceneQueryResult QueryLucene(string databaseName, LuceneQuery query)
		{
			return couchApi.QueryLucene(databaseName, query).WaitForResult();
		}
	}
}