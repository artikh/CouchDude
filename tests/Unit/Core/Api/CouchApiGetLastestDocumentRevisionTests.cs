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
using System.Net.Http.Headers;
using CouchDude;
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Core.Api
{
	public class CouchApiGetLastestDocumentRevisionTests
	{
		private static HttpResponseMessage ConstructOkResponse(string etag = null)
		{
			var response = new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty)};
			if(etag != null)
				response.Headers.ETag = new EntityTagHeaderValue(string.Format("\"{0}\"", etag));
			return response;
		}

		[Fact]
		public void ShouldGetLastestDocumentRevisionCorrectly()
		{
			var response = ConstructOkResponse("1-1a517022a0c2d4814d51abfedf9bfee7");

			var httpMock = new HttpClientMock(response);
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");
			var revision = couchApi.Synchronously.RequestLastestDocumentRevision("doc1");

			Assert.Equal("http://example.com:5984/testdb/doc1", httpMock.Request.RequestUri.ToString());
			Assert.Equal(HttpMethod.Head, httpMock.Request.Method);
			Assert.Equal(null, httpMock.Request.Content);
			Assert.Equal("1-1a517022a0c2d4814d51abfedf9bfee7", revision);
		}

		[Fact]
		public void ShouldThrowOnNullParametersGettingLastestDocumentRevision()
		{
			var response = ConstructOkResponse();
			var httpMock = new HttpClientMock(response);
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			Assert.Throws<ArgumentNullException>(() => couchApi.Synchronously.RequestLastestDocumentRevision(""));
			Assert.Throws<ArgumentNullException>(() => couchApi.Synchronously.RequestLastestDocumentRevision(null));
		}

		[Fact]
		public void ShouldThrowOnAbcentEtagGettingLastestDocumentRevision()
		{
			var response = ConstructOkResponse();
			var httpMock = new HttpClientMock(response);
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			Assert.Throws<ParseException>(() => couchApi.Synchronously.RequestLastestDocumentRevision("doc1"));
		}

		[Fact]
		public void ShouldReturnNullIfNoDocumentFound()
		{
			var httpMock = new HttpClientMock(new HttpResponseMessage(HttpStatusCode.NotFound, "not found"));
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			var version = couchApi.Synchronously.RequestLastestDocumentRevision("doc1");
			Assert.Null(version);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenUpdatingDocumentInDb()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpMock = new HttpClientMock(webExeption);

			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			var couchCommunicationException =
				Assert.Throws<CouchCommunicationException>(
					() => couchApi.Synchronously.RequestLastestDocumentRevision("doc1"));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}
	}
}
