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
using CouchDude.Core.Api;
using CouchDude.Core.Http;
using CouchDude.Core;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class CouchApiUpdateDocumentInDbTests
	{
		[Fact]
		public void ShouldUpdateDocumentInDbCorrectly()
		{
			HttpClientMock httpClientMock;
			ICouchApi couchApi = CreateCouchApi(
				out httpClientMock,
				response: new {
						ok = true,
						id = "doc1",
						rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
					}.ToJsonString());

			var result = couchApi.UpdateDocumentInDb(new { _id = "doc1", name = "John Smith" }.ToDocument());

			Assert.Equal("http://example.com:5984/testdb/doc1", httpClientMock.Request.RequestUri.ToString());
			Assert.Equal(HttpMethod.Put, httpClientMock.Request.Method);
			Assert.Equal(
				new { _id = "doc1", name = "John Smith" }.ToJsonString(), httpClientMock.Request.Content.GetTextReader().ReadToEnd());
			Assert.Equal(
				new {
					ok = true,
					id = "doc1",
					rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
				}.ToJsonFragment(),
				result
			);
		}

		[Fact]
		public void ShouldThrowOnNullParametersUpdatingDocumentInDb()
		{
			ICouchApi couchApi = CreateCouchApi();
			Assert.Throws<ArgumentNullException>(() =>  couchApi.SaveDocumentToDb(new { _id = "doc1" }.ToDocument()));
			Assert.Throws<ArgumentNullException>(() => couchApi.SaveDocumentToDb(new { _id = "doc1" }.ToDocument()));
			Assert.Throws<ArgumentNullException>(() => couchApi.SaveDocumentToDb(null));
		}

		[Fact]
		public void ShouldThrowOnIncorrectJsonUpdatingDocumentInDb()
		{
			ICouchApi couchApi = CreateCouchApi(response: "Some none-json [) content");
			Assert.Throws<ParseException>(() => couchApi.SaveDocumentToDb(new { _id = "doc1" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowOnEmptyResponseUpdatingDocumentInDb()
		{
			ICouchApi couchApi = CreateCouchApi(response: "    ");
			Assert.Throws<ParseException>(() => couchApi.SaveDocumentToDb(new { _id = "doc1" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenUpdatingDocumentInDb()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpMock = new HttpClientMock(webExeption);

			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			var couchCommunicationException =
				Assert.Throws<CouchCommunicationException>(
				() => couchApi.SaveDocumentToDb("doc1", new { _id = "doc1" }.ToDocument()));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}
		
		private static ICouchApi CreateCouchApi(string response = "")
		{
			HttpClientMock httpClientMock;
			return CreateCouchApi(out httpClientMock, response);
		}

		private static ICouchApi CreateCouchApi(out HttpClientMock httpClientMock, string response = "")
		{
			httpClientMock = new HttpClientMock(response);
			return new CouchApi(httpClientMock, new Uri("http://example.com:5984/"), "testdb");
		}
	}
}
