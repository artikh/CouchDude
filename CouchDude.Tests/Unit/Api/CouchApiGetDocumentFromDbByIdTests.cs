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
using CouchDude.Core;
using CouchDude.Core.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class CouchApiGetDocumentFromDbByIdTests
	{
		[Fact]
		public void ShouldGetDocumentFromDbByIdCorrectly()
		{
			var httpMock = new HttpClientMock(
				new {
					_id = "doc1",
					_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
					name = "John Smith"
				}.ToJsonString());

			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");
			var result = couchApi.GetDocumentFromDbById("doc1");

			Assert.Equal("http://example.com:5984/testdb/doc1", httpMock.Request.RequestUri.ToString());
			Assert.Equal(HttpMethod.Get, httpMock.Request.Method);
			Assert.Null(httpMock.Request.Content);
			Assert.Equal(
				new
				{
					_id = "doc1",
					_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
					name = "John Smith"
				}.ToDocument(),
				result
			);
		}

		[Fact]
		public void ShouldEscapeDocumentId()
		{
			var httpMock = new HttpClientMock(new
				{
					_id = "docs/doc1",
					_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
					name = "John Smith"
				}.ToJsonString());
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");
			couchApi.GetDocumentFromDbById("docs/doc1");

			Assert.Equal("http://example.com:5984/testdb/docs%2Fdoc1", httpMock.Request.RequestUri.ToString());
		}

		[Fact]
		public void ShouldNotEscapeDesignDocumentIdPrefix()
		{
			var httpMock = new HttpClientMock(
				new
					{
						_id = "_design/docs/doc1",
						_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
						name = "John Smith"
					}.ToJsonString());
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");
			
			couchApi.GetDocumentFromDbById("_design/docs/doc1");

			Assert.Equal("http://example.com:5984/testdb/_design/docs%2Fdoc1", httpMock.Request.RequestUri.ToString());
		}

		[Fact]
		public void ShouldThrowOnIncorrectJsonGettingDocumentById()
		{
			Assert.Throws<ParseException>(() => {
				var httpMock = new HttpClientMock("Some none-json [) content");
				ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");
				couchApi.GetDocumentFromDbById("doc1");
			});
		}
		
		[Fact]
		public void ShouldThrowOnNullParametersGettingDocumentById()
		{
			var httpMock = new HttpClientMock(string.Empty);
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");
			Assert.Throws<ArgumentNullException>(() => couchApi.GetDocumentFromDbById(null));
			Assert.Throws<ArgumentNullException>(() => couchApi.GetDocumentFromDbById(string.Empty));
		}

		[Fact]
		public void ShouldThrowOnEmptyResponseGettingDocumentById()
		{
			Assert.Throws<ParseException>(() => {
				var httpMock = new HttpClientMock("    ");
				ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");
				couchApi.GetDocumentFromDbById("doc1");
			});
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenGettingDocument()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpMock = new HttpClientMock(webExeption);
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			var couchCommunicationException = 
				Assert.Throws<CouchCommunicationException>(() => couchApi.GetDocumentFromDbById("doc1"));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}

		[Fact]
		public void ShouldReturnNullOn404WebExceptionWhenGettingDocument()
		{
			var httpMock = new HttpClientMock(new HttpResponseMessage{ StatusCode = HttpStatusCode.NotFound });
			ICouchApi couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			Assert.Null(couchApi.GetDocumentFromDbById("doc1"));
		}
	}
}
