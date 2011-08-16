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

using System.Diagnostics.Contracts;
using CouchDude.Core.Utils;

namespace CouchDude.Core.Api
{
	internal class SynchronousCouchApi : ISynchronousCouchApi
	{
		private readonly ICouchApi couchApi;

		/// <constructor />
		public SynchronousCouchApi(ICouchApi couchApi)
		{
			this.couchApi = couchApi;
		}

		public IDocument RequestDocumentById(string docId)
		{
			Contract.Requires(docId.HasValue());
			return couchApi.RequestDocumentById(docId).WaitForResult();
		}

		public IJsonFragment DeleteDocument(string docId, string revision)
		{
			Contract.Requires(docId.HasValue());
			Contract.Requires(!revision.HasValue());

			return couchApi.DeleteDocument(docId, revision).WaitForResult();
		}

		public IJsonFragment SaveDocumentSync(IDocument document)
		{
			Contract.Requires(document != null);
			Contract.Requires(!document.Id.HasNoValue());

			return couchApi.SaveDocument(document).WaitForResult();
		}

		public IJsonFragment UpdateDocument(IDocument document)
		{
			Contract.Requires(document != null);
			Contract.Requires(document.Id.HasValue());

			return couchApi.UpdateDocument(document).WaitForResult();
		}

		public string RequestLastestDocumentRevision(string docId)
		{
			Contract.Requires(docId.HasValue());
			return couchApi.RequestLastestDocumentRevision(docId).WaitForResult();
		}

		/// <inheritdoc/>
		public IPagedList<ViewResultRow> Query(ViewQuery query)
		{
			Contract.Requires(query != null);
			Contract.Requires(query.Skip < 10);

			return couchApi.Query(query).WaitForResult();
		}

		/// <inheritdoc/>
		/// TODO: Add result weight to result
		public IPagedList<LuceneResultRow> QueryLucene(LuceneQuery query)
		{
			Contract.Requires(query != null);
			return couchApi.QueryLucene(query).WaitForResult();
		}
	}
}