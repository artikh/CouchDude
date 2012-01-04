using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class DatabaseApiSaveAttachmentTest
	{
		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnConflict()
		{
			var httpMock = new MockMessageHandler(
				HttpStatusCode.Conflict,
				new { error = "conflict", reason = "Document update conflict." }.ToJsonString());
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Throws<StaleObjectStateException>(
				() => databaseApi.Synchronously.SaveAttachment(new DocumentAttachment("attachment1"), "doc1", "rev1")
			);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOn500()
		{
			var httpMock = new MockMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
			var databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Throws<CouchCommunicationException>(
				() => databaseApi.Synchronously.SaveAttachment(new DocumentAttachment("attachment1"), "doc1", "rev1")
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
				Content = new StringContent("{\"ok\":true}", Encoding.UTF8, "application/json")
			});
		}
	}
}