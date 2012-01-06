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
using System.Collections.Generic;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal class SynchronousDatabaseApi : ISynchronousDatabaseApi
	{
		private readonly IDatabaseApi databaseApi;

		public SynchronousDatabaseApi(IDatabaseApi databaseApi)
		{
			this.databaseApi = databaseApi;
		}

		public Document RequestDocument(
			string documentId,
			string revision = null,
			AdditionalDocumentProperty additionalProperties = default(AdditionalDocumentProperty))
		{
			return databaseApi.RequestDocument(documentId, revision, additionalProperties).WaitForResult();
		}

		public DocumentInfo DeleteDocument(string docId, string revision)
		{
			return databaseApi.DeleteDocument(docId, revision).WaitForResult();
		}

		public DocumentInfo SaveDocument(Document document)
		{
			return databaseApi.SaveDocument(document).WaitForResult();
		}

		public DocumentInfo SaveDocument(Document document, bool overwriteConcurrentUpdates) 
		{
			return databaseApi.SaveDocument(document, overwriteConcurrentUpdates).WaitForResult();
		}

		public DocumentInfo CopyDocument(
			string originalDocumentId, 
			string originalDocumentRevision,
			string targetDocumentId, 
			string targetDocumentRevision = null)
		{
			return databaseApi
				.CopyDocument(originalDocumentId, originalDocumentRevision, targetDocumentId, targetDocumentRevision)
				.WaitForResult();
		}

		public DocumentAttachment RequestAttachment(string attachmentId, string documentId, string documentRevision = null)
		{
			return databaseApi.RequestAttachment(attachmentId, documentId, documentRevision).WaitForResult();
		}

		public DocumentInfo SaveAttachment(DocumentAttachment attachment, string documentId, string documentRevision = null)
		{
			return databaseApi.SaveAttachment(attachment, documentId, documentRevision).WaitForResult();
		}

		public DocumentInfo DeleteAttachment(string attachmentId, string documentId, string documentRevision)
		{
			return databaseApi.DeleteAttachment(attachmentId, documentId, documentRevision).WaitForResult();
		}

		public IDictionary<string, DocumentInfo> BulkUpdate(Action<IBulkUpdateBatch> updateCommandBuilder)
		{
			return databaseApi.BulkUpdate(updateCommandBuilder).WaitForResult();
		}

		public string RequestLastestDocumentRevision(string docId)
		{
			return databaseApi.RequestLastestDocumentRevision(docId).WaitForResult();
		}

		public IViewQueryResult Query(ViewQuery query)
		{
			return databaseApi.Query(query).WaitForResult();
		}

		public ILuceneQueryResult QueryLucene(LuceneQuery query)
		{
			return databaseApi.QueryLucene(query).WaitForResult();
		}

		public void Create(bool throwIfExists = true)
		{
			databaseApi.Create(throwIfExists).WaitForResult();
		}

		public void Delete()
		{
			databaseApi.Delete().WaitForResult();
		}

		public DatabaseInfo RequestInfo()
		{
			return databaseApi.RequestInfo().WaitForResult();
		}
	}
}