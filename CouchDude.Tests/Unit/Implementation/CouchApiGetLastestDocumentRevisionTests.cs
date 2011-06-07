using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
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
			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");
			var revision = couchApi.GetLastestDocumentRevision("doc1");

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
			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			Assert.Throws<ArgumentNullException>(() => couchApi.GetLastestDocumentRevision(""));
			Assert.Throws<ArgumentNullException>(() => couchApi.GetLastestDocumentRevision(null));
		}

		[Fact]
		public void ShouldThrowOnAbcentEtagGettingLastestDocumentRevision()
		{
			var response = ConstructOkResponse();
			var httpMock = new HttpClientMock(response);
			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			Assert.Throws<CouchResponseParseException>(() => couchApi.GetLastestDocumentRevision("doc1"));
		}

		[Fact]
		public void ShouldReturnNullIfNoDocumentFound()
		{
			var httpMock = new HttpClientMock(new HttpResponseMessage(HttpStatusCode.NotFound, "not found"));
			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			var version = couchApi.GetLastestDocumentRevision("doc1");
			Assert.Null(version);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenUpdatingDocumentInDb()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpMock = new HttpClientMock(webExeption);

			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			var couchCommunicationException =
				Assert.Throws<CouchCommunicationException>(
					() => couchApi.GetLastestDocumentRevision("doc1"));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}
	}
}
