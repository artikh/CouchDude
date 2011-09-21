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
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class CouchApiAttachmentSupport
	{
		[Fact]
		public void ShouldSaveInlineAttachmentsForDocumentsAndThenLoadThem() 
		{
			var couchApi = Factory.CreateCouchApi("http://127.0.0.1:5984/");
			var dbApi = couchApi.Db("testdb");

			var docId = Guid.NewGuid().ToString();
			var newDoc = new {_id = docId}.ToDocument();
			newDoc.Attachments.AddInline("attachment1", new byte[] { 42, 42, 42 });
			newDoc.Attachments.AddInline("attachment2", "LUE");

			dbApi.Synchronously.SaveDocument(newDoc);

			var loadedDoc = dbApi.Synchronously.RequestDocumentById(docId);

			Assert.Equal(2, loadedDoc.Attachments.Count);
			var firstAttachment = loadedDoc.Attachments["attachment1"];
			Assert.False(firstAttachment.Inline);
			Assert.Equal("application/octet-stream", firstAttachment.ContentType);
			Assert.Equal(3, firstAttachment.Length);
			
			var secondAttachment = loadedDoc.Attachments["attachment2"];
			Assert.False(secondAttachment.Inline);
			Assert.Equal("text/plain", secondAttachment.ContentType);
			Assert.Equal(3, secondAttachment.Length);
		}
	}
}