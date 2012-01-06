using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class DatabaseApiDeleteAttachmentTest
	{
		[Fact]
		public void ShouldSendDeleteRequestToCouchDB() 
		{
			var httpMock = MockHttpClient();
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			databaseApi.Synchronously.DeleteAttachment("attachment1", "doc1", "rev1");

			Assert.Equal("http://example.com:5984/testdb/doc1/attachment1?rev=rev1", httpMock.Request.RequestUri.ToString());
			Assert.Equal("DELETE", httpMock.Request.Method.ToString());
		}

		[Fact]
		public void ShouldParseDocumentInfo() 
		{
			var httpMock = MockHttpClient();
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			var docInfo = databaseApi.Synchronously.DeleteAttachment("attachment1", "doc1", "rev1");

			Assert.Equal("doc1", docInfo.Id);
			Assert.Equal("rev2", docInfo.Revision);
		}

		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnConflict()
		{
			var httpMock = new MockMessageHandler(
				HttpStatusCode.Conflict,
				new { error = "conflict", reason = "Document update conflict." }.ToJsonString());
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Throws<StaleObjectStateException>(
				() => databaseApi.Synchronously.DeleteAttachment("attachment1", "doc1", "rev1")
			);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOn500()
		{
			var httpMock = new MockMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Throws<CouchCommunicationException>(
				() => databaseApi.Synchronously.DeleteAttachment("attachment1", "doc1", "rev1")
			);
		}
		
		private static ICouchApi CreateCouchApi(MockMessageHandler handler = null)
		{
			handler = handler ?? MockHttpClient();
			return Factory.CreateCouchApi(new Uri("http://example.com:5984/"), handler);
		}

		private static MockMessageHandler MockHttpClient()
		{
			return new MockMessageHandler(new HttpResponseMessage
			{
				Content = new StringContent("{\"ok\":true, \"id\":\"doc1\", \"rev\":\"rev2\"}", Encoding.UTF8, "application/json")
			});
		}
	}
}