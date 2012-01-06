using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class DatabaseApiRequestAttachmentTest
	{
		[Fact]
		public void ShouldRequestDocumentAttachment()
		{
			var httpMock = MockHttpClient();
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			databaseApi.Synchronously.RequestAttachment("attachment1", "doc1", "rev1");

			Assert.Equal("http://example.com:5984/testdb/doc1/attachment1?rev=rev1", httpMock.Request.RequestUri.ToString());
			Assert.Equal("GET", httpMock.Request.Method.ToString());
		}

		[Fact]
		public void ShouldParseDocumentAttachment()
		{
			var httpMock = MockHttpClient();
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			var attachment = databaseApi.Synchronously.RequestAttachment("attachment1", "doc1", "rev1");

			Assert.Equal(4, attachment.Length);
			Assert.False(attachment.Inline);
			Assert.Equal("attachment1", attachment.Id);
			Assert.Equal("text/plain", attachment.ContentType);
		}

		[Fact]
		public void ShouldReturnNullIfAttachmentHaveNotFoundOnExistingDocument() 
		{
			var httpMock = new MockMessageHandler(
				HttpStatusCode.NotFound, 
				new { error = "not_found", reason = "Document is missing attachment" }.ToJsonString());
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Null(databaseApi.Synchronously.RequestAttachment("attachment1", "doc1", "rev1"));
		}

		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnConflict() 
		{
			var httpMock = new MockMessageHandler(
				HttpStatusCode.Conflict, new { error = "conflict", reason = "Document update conflict." }.ToJsonString());
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Throws<StaleObjectStateException>(
				() => databaseApi.Synchronously.RequestAttachment("attachment1", "doc1", "rev1")
			);
		}

		[Fact] 
		public void ShouldThrowDocumentNotFoundException() 
		{
			var httpMock = new MockMessageHandler(
				HttpStatusCode.NotFound, new { error = "not_found", reason = "missing" }.ToJsonString());
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Throws<DocumentNotFoundException>(
				() => databaseApi.Synchronously.RequestAttachment("attachment1", "doc1", "rev1")
			);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOn500() 
		{
			var httpMock = new MockMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Throws<CouchCommunicationException>(
				() => databaseApi.Synchronously.RequestAttachment("attachment1", "doc1", "rev1")
			);
		}

		private static ICouchApi CreateCouchApi(MockMessageHandler handler = null)
		{
			handler = handler ?? MockHttpClient();
			return Factory.CreateCouchApi(new Uri("http://example.com:5984/"), handler);
		}

		private static MockMessageHandler MockHttpClient()
		{
			return new MockMessageHandler(new HttpResponseMessage {
				Content = new StringContent("test", Encoding.UTF8, "text/plain")
			});
		}
	}
}