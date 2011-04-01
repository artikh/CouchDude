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
	public class CouchApiGetDocumentFromDbByIdTests
	{
		[Fact]
		public void ShouldGetDocumentFromDbByIdCorrectly()
		{
			var r = TestInMockEnvironment(
				couchApi => couchApi.GetDocumentFromDbById("doc1"),
				response: new
				{
					_id = "doc1",
					_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
					name = "John Smith"
				}.ToJson()
			);

			Assert.Equal("http://example.com:5984/testdb/doc1", r.RequestedUri);
			Assert.Equal("GET", r.RequestedMethod);
			Assert.Equal(null, r.RequestBody);
			Utils.AssertSameJson(
				new
				{
					_id = "doc1",
					_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
					name = "John Smith"
				},
				r.Result
			);
		}

		[Fact]
		public void ShouldThrowOnIncorrectJsonGettingDocumentById()
		{
			Assert.Throws<CouchResponseParseException>(() =>
				TestInMockEnvironment(
					couchApi => couchApi.GetDocumentFromDbById("doc1"),
					response: "Some none-json [) content"
				)
			);
		}
		
		[Fact]
		public void ShouldThrowOnNullParametersGettingDocumentById()
		{
			Assert.Throws<ArgumentNullException>(() => TestInMockEnvironment(
					couchApi => couchApi.GetDocumentFromDbById(null)
			));
			Assert.Throws<ArgumentNullException>(() => TestInMockEnvironment(
					couchApi => couchApi.GetDocumentFromDbById("")
			));
		}

		[Fact]
		public void ShouldThrowOnEmptyResponseGettingDocumentById()
		{
			Assert.Throws<CouchResponseParseException>(() =>
				TestInMockEnvironment(
					couchApi => couchApi.GetDocumentFromDbById("doc1"),
					response: "    "
				)
			);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenGettingDocument()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpMock = new Mock<IHttp>();
			httpMock
				.Setup(h => h.RequestAndOpenTextReader(
					It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<TextReader>()))
				.Throws(webExeption);

			var couchApi = new CouchApi(httpMock.Object, new Uri("http://example.com/"));

			var couchCommunicationException = 
				Assert.Throws<CouchCommunicationException>(() => couchApi.GetDocumentFromDbById("doc1"));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}



		private static TestResult TestInMockEnvironment(
			Func<ICouchApi, JObject> doTest, string response = "")
		{
			Uri requestedUri = null;
			string requestedMethod = null;
			string requestBody = null;
			var httpMock = new Mock<IHttp>();
			httpMock
				.Setup(h => h.RequestAndOpenTextReader(
					It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<TextReader>()))
				.Returns(
					(Uri uri, string method, TextReader bodyReader) =>
					{
						requestedUri = uri;
						requestedMethod = method;
						requestBody = bodyReader == null ? null : bodyReader.ReadToEnd();

						return response.ToTextReader();
					});
			var result = doTest(
				new CouchApi(httpMock.Object, new Uri("http://example.com:5984/testdb/"))
			);

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
			public JObject Result;
		}
	}
}
