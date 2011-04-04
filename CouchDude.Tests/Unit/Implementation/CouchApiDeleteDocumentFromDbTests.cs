using System;
using System.Net;
using CouchDude.Core;
using CouchDude.Core.HttpClient;
using CouchDude.Core.Implementation;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchApiDeleteDocumentFromDbTests
	{
		[Fact]
		public void ShouldSendDeleteRequestOnDeletion()
		{
			var httpMock = new HttpClientMock(new { ok = true }.ToJson());
			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			var resultObject = couchApi.DeleteDocument(docId: "doc1", revision: "1-1a517022a0c2d4814d51abfedf9bfee7");

			Assert.Equal(new Uri("http://example.com:5984/testdb/doc1?rev=1-1a517022a0c2d4814d51abfedf9bfee7"), httpMock.Request.Uri);
			Assert.Equal("DELETE", httpMock.Request.Method);
			Utils.AssertSameJson(new { ok = true }.ToJObject(), resultObject);
		}

		[Fact]
		public void ShouldThrowOnNullArguments()
		{
			var httpMock = new HttpClientMock(new { ok = true }.ToJson());
			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			Assert.Throws<ArgumentNullException>(() => couchApi.DeleteDocument(docId: "doc1", revision: null));
			Assert.Throws<ArgumentNullException>(() => couchApi.DeleteDocument(docId: null, revision: "1-1a517022a0c2d4814d51abfedf9bfee7"));
		}

		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnConflict()
		{
			var httpMock = new HttpClientMock(new HttpResponse {
				Status = HttpStatusCode.Conflict
			});
			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			Assert.Throws<StaleObjectStateException>(
				() => couchApi.DeleteDocument(docId: "doc1", revision: "1-1a517022a0c2d4814d51abfedf9bfee7"));
		}
	}
}