using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using CouchDude.Api;
using CouchDude.Utils;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class DatabaseApiSaveAttachmentTest
	{
		[Fact]
		public void ShouldSendAttachmentToCouchDB() 
		{
			var httpMock = MockHttpClient();
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			var documentAttachment = new Attachment("attachment1") {
				ContentType = "text/html"
			};
			documentAttachment.SetData(new MemoryStream(Encoding.UTF8.GetBytes("<p>test</p>")));

			databaseApi.Synchronously.SaveAttachment(documentAttachment, "doc1", "rev1");

			Assert.Equal("http://example.com:5984/testdb/doc1/attachment1?rev=rev1", httpMock.Request.RequestUri.ToString());
			Assert.Equal("PUT", httpMock.Request.Method.ToString());
			Assert.Equal("text/html", httpMock.Request.Content.Headers.ContentType.MediaType);
			Assert.Equal("<p>test</p>", httpMock.RequestBodyString);
		}

		[Fact]
		public void ShouldParseDocumentInfo() 
		{
			var httpMock = MockHttpClient();
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			var documentAttachment = new Attachment("attachment1") { ContentType = "text/html" };
			documentAttachment.SetData(new MemoryStream(Encoding.UTF8.GetBytes("<p>test</p>")));

			var docInfo = databaseApi.Synchronously.SaveAttachment(documentAttachment, "doc1", "rev1");

			Assert.Equal("doc1", docInfo.Id);
			Assert.Equal("rev2", docInfo.Revision);
		}

		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnConflict()
		{
			var httpMock = new MockMessageHandler(
				HttpStatusCode.Conflict,
				new { error = "conflict", reason = "Document update conflict." }.ToJsonObject());
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Throws<StaleObjectStateException>(
				() => databaseApi.Synchronously.SaveAttachment(new Attachment("attachment1"), "doc1", "rev1")
			);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOn500()
		{
			var httpMock = new MockMessageHandler(HttpStatusCode.InternalServerError, string.Empty, MediaType.Json);
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Throws<CouchCommunicationException>(
				() => databaseApi.Synchronously.SaveAttachment(new Attachment("attachment1"), "doc1", "rev1")
			);
		}
		
		private static ICouchApi CreateCouchApi(MockMessageHandler handler = null)
		{
			handler = handler ?? MockHttpClient();
			return new CouchApi(new CouchApiSettings(new Uri("http://example.com:5984/")), handler);
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