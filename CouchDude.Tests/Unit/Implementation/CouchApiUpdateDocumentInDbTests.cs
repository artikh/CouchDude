﻿using System;
using System.Net; using CouchDude.Core.Http;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchApiUpdateDocumentInDbTests
	{
		[Fact]
		public void ShouldUpdateDocumentInDbCorrectly()
		{
			var r = TestInMockEnvironment(
				couchApi => couchApi.UpdateDocumentInDb(
					"doc1", new { _id = "doc1", name = "John Smith" }.ToJObject()
				),
				response: new {
					ok = true,
					id = "doc1", 
					rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
				}.ToJson()
			);

			Assert.Equal("http://example.com:5984/testdb/doc1", r.RequestedUri);
			Assert.Equal("PUT", r.RequestedMethod);
			Utils.AssertSameJson(
				new { _id = "doc1", name = "John Smith" }.ToJToken(), r.RequestBody);
			Utils.AssertSameJson(
				new {
					ok = true,
					id = "doc1",
					rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
				},
				r.Result
			);
		}

		[Fact]
		public void ShouldThrowOnNullParametersUpdatingDocumentInDb()
		{
			Assert.Throws<ArgumentNullException>(() => TestInMockEnvironment(
					couchApi => couchApi.SaveDocumentToDb(null, new { _id = "doc1" }.ToJObject())
			));
			Assert.Throws<ArgumentNullException>(() => TestInMockEnvironment(
					couchApi => couchApi.SaveDocumentToDb("", new { _id = "doc1" }.ToJObject())
			));
			Assert.Throws<ArgumentNullException>(() => TestInMockEnvironment(
					couchApi => couchApi.SaveDocumentToDb("doc1", null)
			));
		}

		[Fact]
		public void ShouldThrowOnIncorrectJsonUpdatingDocumentInDb()
		{
			Assert.Throws<CouchResponseParseException>(() =>
				TestInMockEnvironment(
					couchApi => couchApi.SaveDocumentToDb("doc1", new { _id = "doc1" }.ToJObject()),
					response: "Some none-json [) content"
				)
			);
		}

		[Fact]
		public void ShouldThrowOnEmptyResponseUpdatingDocumentInDb()
		{
			Assert.Throws<CouchResponseParseException>(() =>
				TestInMockEnvironment(
					couchApi => couchApi.SaveDocumentToDb("doc1", new { _id = "doc1" }.ToJObject()),
					response: "    "
				)
			);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenUpdatingDocumentInDb()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpMock = new HttpClientMock(webExeption);

			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			var couchCommunicationException =
				Assert.Throws<CouchCommunicationException>(
				() => couchApi.SaveDocumentToDb("doc1", new { _id = "doc1" }.ToJObject()));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}



		private static TestResult TestInMockEnvironment(
			Func<ICouchApi, JObject> doTest, string response = "")
		{
			var httpMock = new HttpClientMock(response);
			var result = doTest(
				new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb")
			);

			var recivedRequest = httpMock.Request;
			return new TestResult
			{
				RequestedUri = recivedRequest.RequestUri == null ? null : recivedRequest.RequestUri.ToString(),
				RequestedMethod = recivedRequest.Method.ToString(),
				RequestBody = recivedRequest.Content.GetTextReader() == null ? null : recivedRequest.Content.GetTextReader().ReadToEnd(),
				Result = result
			};
		}

		private class TestResult
		{
			public string RequestedUri;
			public string RequestedMethod;
			public string RequestBody;
			public JObject Result;
		}
	}
}
