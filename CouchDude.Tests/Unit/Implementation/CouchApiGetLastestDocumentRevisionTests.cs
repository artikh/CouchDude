using System;
using System.IO;
using System.Net;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchApiGetLastestDocumentRevisionTests
	{
		[Fact]
		public void ShouldGetLastestDocumentRevisionCorrectly()
		{
			var r = TestInMockEnvironment(
				"doc1", new WebHeaderCollection {{ "Etag", "1-1a517022a0c2d4814d51abfedf9bfee7"}}
			);

			Assert.Equal("http://example.com:5984/testdb/doc1", r.RequestedUri);
			Assert.Equal("HEAD", r.RequestedMethod);
			Assert.Equal(null, r.RequestBody);
			Assert.Equal("1-1a517022a0c2d4814d51abfedf9bfee7", r.Result);
		}

		[Fact]
		public void ShouldThrowOnNullParametersGettingLastestDocumentRevision()
		{
			Assert.Throws<ArgumentNullException>(() => TestInMockEnvironment(""));
			Assert.Throws<ArgumentNullException>(() => TestInMockEnvironment(null));
		}

		[Fact]
		public void ShouldThrowOnAbcentEtagGettingLastestDocumentRevision()
		{
			Assert.Throws<CouchResponseParseException>(() =>
				TestInMockEnvironment("doc1", new WebHeaderCollection()));
			Assert.Throws<CouchResponseParseException>(() =>
				TestInMockEnvironment("doc1", new WebHeaderCollection{{ "Etag", ""}}));
		}
		
		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenUpdatingDocumentInDb()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpMock = new Mock<IHttp>();
			httpMock
				.Setup(h => h.RequestAndGetHeaders(
					It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<TextReader>()))
				.Throws(webExeption);

			var couchApi = new CouchApi(httpMock.Object, new Uri("http://example.com/"));

			var couchCommunicationException =
				Assert.Throws<CouchCommunicationException>(
					() => couchApi.GetLastestDocumentRevision("doc1"));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}

		private static TestResult TestInMockEnvironment(
			string docId, WebHeaderCollection response = null)
		{
			Uri requestedUri = null;
			string requestedMethod = null;
			string requestBody = null;
			var httpMock = new Mock<IHttp>();
			httpMock
				.Setup(h => h.RequestAndGetHeaders(
					It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<TextReader>()))
				.Returns(
					(Uri uri, string method, TextReader bodyReader) =>
					{
						requestedUri = uri;
						requestedMethod = method;
						requestBody = bodyReader == null ? null : bodyReader.ReadToEnd();

						return response ?? new WebHeaderCollection();
					});
			var couchApi = new CouchApi(httpMock.Object, new Uri("http://example.com:5984/testdb/"));
			var result = couchApi.GetLastestDocumentRevision(docId);

			return new TestResult
			{
				RequestedUri = requestedUri == null ? null : requestedUri.ToString(),
				RequestedMethod = requestedMethod,
				RequestBody = requestBody,
				Result = result
			};
		}

		private class TestResult
		{
			public string RequestedUri;
			public string RequestedMethod;
			public string RequestBody;
			public string Result;
		}
	}
}
