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
using System.Net;
using System.Net.Http;

using CouchDude.Api;
using CouchDude.Impl;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class CouchApiDeleteDocumentFromDbTests
	{
		[Fact]
		public void ShouldSendDeleteRequestOnDeletion()
		{
			var httpMock = new HttpClientMock(new { ok = true, id = "doc1", rev = "1-1a517022a0c2d4814d51abfedf9bfee7" }.ToJsonString());
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			var resultObject = couchApi.Synchronously.DeleteDocument(docId: "doc1", revision: "1-1a517022a0c2d4814d51abfedf9bfee7");

			Assert.Equal(
				"http://example.com:5984/testdb/doc1?rev=1-1a517022a0c2d4814d51abfedf9bfee7", 
				httpMock.Request.RequestUri.ToString());
			Assert.Equal("DELETE", httpMock.Request.Method.ToString());
			Assert.Equal(new DocumentInfo(id: "doc1", revision: "1-1a517022a0c2d4814d51abfedf9bfee7"), resultObject);
		}

		[Fact]
		public void ShouldThrowOnNullArguments()
		{
			var httpMock = new HttpClientMock(new { ok = true }.ToJsonString());
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			Assert.Throws<ArgumentNullException>(() => couchApi.Synchronously.DeleteDocument(docId: "doc1", revision: null));
			Assert.Throws<ArgumentNullException>(() => couchApi.Synchronously.DeleteDocument(docId: null, revision: "1-1a517022a0c2d4814d51abfedf9bfee7"));
		}

		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnConflict()
		{
			var httpMock = new HttpClientMock(new HttpResponseMessage {
				StatusCode = HttpStatusCode.Conflict
			});
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			Assert.Throws<StaleObjectStateException>(
				() => couchApi.Synchronously.DeleteDocument(docId: "doc1", revision: "1-1a517022a0c2d4814d51abfedf9bfee7"));
		}

		[Fact]
		public void ShouldThrowInvalidDocumentExceptionOnForbidden()
		{
			var httpMock = new HttpClientMock(new HttpResponseMessage {
				StatusCode = HttpStatusCode.Forbidden
			});
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			Assert.Throws<InvalidDocumentException>(
				() => couchApi.Synchronously.DeleteDocument(docId: "doc1", revision: "1-1a517022a0c2d4814d51abfedf9bfee7"));
		}
	}
}