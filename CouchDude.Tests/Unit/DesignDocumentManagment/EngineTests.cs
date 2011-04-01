using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CouchDude.Core;
using CouchDude.Core.DesignDocumentManagment;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.DesignDocumentManagment
{
	public class EngineTests
	{
		private readonly JObject docA =
			JObject.Parse(@"{ ""_id"": ""_design/doc1"", ""_rev"": ""1"", ""prop"": ""prop value of doc1"" }");

		private readonly JObject docAWithoutRev =
			JObject.Parse(@"{ ""_id"": ""_design/doc1"", ""prop"": ""prop value of doc1"" }");

		private readonly JObject docB =
			JObject.Parse(@"{ ""_id"": ""_design/doc2"", ""_rev"": ""1"", ""prop"": ""prop value of doc2"" }");

		private readonly JObject docBWithoutRev =
			JObject.Parse(@"{ ""_id"": ""_design/doc2"", ""prop"": ""prop value of doc2"" }");

		private readonly JObject docB2WithoutRev =
			JObject.Parse(@"{ ""_id"": ""_design/doc2"", ""_rev"": ""1"", ""prop"": ""prop value of doc2, version 2"" }");

		private readonly JObject docC =
			JObject.Parse(@"{ ""_id"": ""_design/doc3"", ""_rev"": ""1"", ""prop"": ""prop value of doc3"" }");

		private readonly JObject docCWithoutRev =
			JObject.Parse(@"{ ""_id"": ""_design/doc3"", ""prop"": ""prop value of doc3"" }");

		[Fact]
		public void ShuldPassGenerateRequestThroughTo()
		{
			var engine = new Engine(
				Mock.Of<IHttp>(),
				Mock.Of<IDesignDocumentExtractor>(), 
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDDMap(docA)));

			var generatedJsonStingDocs = engine.Generate().ToArray();

			Assert.Equal(1, generatedJsonStingDocs.Length);
			Assert.Equal(docA.ToString(), generatedJsonStingDocs[0], new JTokenStringCompairer());
		}

		[Fact]
		public void ShouldReturnFalseIfHaveNotChanged()
		{
			var engine = new Engine(
				Mock.Of<IHttp>(p => p.RequestAndOpenTextReader(It.IsAny<Uri>(), "GET", null) == new StringReader(string.Empty)),
				Mock.Of<IDesignDocumentExtractor>(
					e => e.Extract(It.IsAny<TextReader>()) == CreateDDMap(docA, docB, docC)
					),
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDDMap(docAWithoutRev, docBWithoutRev, docCWithoutRev)));

			Assert.False(engine.CheckIfChanged(new Uri("http://example.com")));
		}

		[Fact]
		public void ShouldReturnTrueIfThereAreMoreDocumentOnDisk()
		{
			var engine = new Engine(
				Mock.Of<IHttp>(p => p.RequestAndOpenTextReader(It.IsAny<Uri>(), "GET", null) == new StringReader(string.Empty)),
				Mock.Of<IDesignDocumentExtractor>(
					e => e.Extract(It.IsAny<TextReader>()) == CreateDDMap(docA, docB)
					), 
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDDMap(docAWithoutRev, docBWithoutRev, docCWithoutRev)));

			Assert.True(engine.CheckIfChanged(new Uri("http://example.com")));
		}

		[Fact]
		public void ShouldReturnTrueIfDocumentOnDiskHaveChanged()
		{
			var engine = new Engine(
				Mock.Of<IHttp>(p => p.RequestAndOpenTextReader(It.IsAny<Uri>(), "GET", null) == new StringReader(string.Empty)),
				Mock.Of<IDesignDocumentExtractor>(
					e => e.Extract(It.IsAny<TextReader>()) == CreateDDMap(docA, docB, docC)
					), 
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDDMap(docAWithoutRev, docB2WithoutRev, docCWithoutRev)));

			Assert.True(engine.CheckIfChanged(new Uri("http://example.com")));
		}

		[Fact]
		public void ShouldPushNewDocumensWithoutRevisionsWithPut()
		{
			Uri requestUri = null;
			string method = null;
			string body = null;

			var couchProxyMock = new Mock<IHttp>();
			couchProxyMock
				.Setup(p => p.RequestAndOpenTextReader(It.IsAny<Uri>(), "GET", null))
				.Returns(new StringReader(string.Empty));
			couchProxyMock
				.Setup(p => p.Request(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<TextReader>()))
				.Callback<Uri, string, TextReader>((u, m, b) =>
				{
					requestUri = u;
					method = m;
					body = b == null? null: b.ReadToEnd();
				});
			
			var engine = new Engine(
				couchProxyMock.Object,
				Mock.Of<IDesignDocumentExtractor>(
					e => e.Extract(It.IsAny<TextReader>()) == new Dictionary<string, DesignDocument>(0)
				),
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDDMap(docAWithoutRev))
			);

			engine.PushIfChanged(new Uri("http://example.com"));

			Assert.Equal(new Uri("http://example.com/_design/doc1"), requestUri);
			Assert.Equal("PUT", method);
			Assert.Equal(docAWithoutRev.ToString(), body, new JTokenStringCompairer());
		}
		
		[Fact]
		public void ShouldPushUpdatedDocumensWithDbDocumentRevisionWithPut()
		{
			Uri requestUri = null;
			string method = null;
			string body = null;

			var couchProxyMock = new Mock<IHttp>();
			couchProxyMock.Setup(p => p.RequestAndOpenTextReader(It.IsAny<Uri>(), "GET", null)).Returns(new StringReader(string.Empty));
			couchProxyMock
				.Setup(p => p.Request(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<TextReader>()))
				.Callback<Uri, string, TextReader>((u, m, b) =>
				{
					requestUri = u;
					method = m;
					body = b == null? null: b.ReadToEnd();
				});
			
			var engine = new Engine(
				couchProxyMock.Object,
				Mock.Of<IDesignDocumentExtractor>(
					e => e.Extract(It.IsAny<TextReader>()) == CreateDDMap(docB)
				),
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDDMap(docB2WithoutRev))
			);

			engine.PushIfChanged(new Uri("http://example.com"));

			Assert.Equal(new Uri("http://example.com/_design/doc2"), requestUri);
			Assert.Equal("PUT", method);
			
			var expectedDoc = (JObject)docB2WithoutRev.DeepClone();
			expectedDoc["_rev"] = docB["_rev"];
			Assert.Equal(expectedDoc.ToString(), body, new JTokenStringCompairer());
		}

		private static Dictionary<string, DesignDocument> CreateDDMap(params JObject[] objects)
		{
			var map = new Dictionary<string, DesignDocument>(objects.Length);
			foreach (var jObject in objects)
			{
				var id = jObject["_id"].Value<string>();
				if (jObject.Property("_rev") != null)
				{
					var rev = jObject["_rev"].Value<string>();
					map.Add(id, new DesignDocument(jObject, id, rev));
				}
				else
					map.Add(id, new DesignDocument(jObject, id));
			}
			return map;
		}
	}
}
