#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
	Licensed under the Apache License, Version 2.0 (the "License");					
	you may not use this file except in compliance with the License.					
	You may obtain a copy of the License at																	
																																					
	    http://www.apache.org/licenses/LICENSE-2.0														
																																					
	Unless required by applicable law or agreed to in writing, software			
	distributed under the License is distributed on an "AS IS" BASIS,				
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
	See the License for the specific language governing permissions and			
	limitations under the License.																						
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CouchDude.SchemeManager;
using CouchDude.Http;

using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.SchemeManager
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
				new HttpClientMock(),
				Mock.Of<IDesignDocumentExtractor>(), 
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDesignDocumentMap(docA)));

			var generatedJsonStingDocs = engine.Generate().ToArray();

			Assert.Equal(1, generatedJsonStingDocs.Length);
			Assert.Equal(docA.ToString(), generatedJsonStingDocs[0], new JTokenStringCompairer());
		}

		[Fact]
		public void ShouldReturnFalseIfHaveNotChanged()
		{
			var engine = new Engine(
				new HttpClientMock(),
				Mock.Of<IDesignDocumentExtractor>(
					e => e.Extract(It.IsAny<TextReader>()) == CreateDesignDocumentMap(docA, docB, docC)
					),
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDesignDocumentMap(docAWithoutRev, docBWithoutRev, docCWithoutRev)));

			Assert.False(engine.CheckIfChanged(new Uri("http://example.com")));
		}

		[Fact]
		public void ShouldReturnTrueIfThereAreMoreDocumentOnDisk()
		{
			var engine = new Engine(
				new HttpClientMock(),
				Mock.Of<IDesignDocumentExtractor>(
					e => e.Extract(It.IsAny<TextReader>()) == CreateDesignDocumentMap(docA, docB)
					), 
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDesignDocumentMap(docAWithoutRev, docBWithoutRev, docCWithoutRev)));

			Assert.True(engine.CheckIfChanged(new Uri("http://example.com")));
		}

		[Fact]
		public void ShouldReturnTrueIfDocumentOnDiskHaveChanged()
		{
			var engine = new Engine(
				new HttpClientMock(),
				Mock.Of<IDesignDocumentExtractor>(
					e => e.Extract(It.IsAny<TextReader>()) == CreateDesignDocumentMap(docA, docB, docC)
					), 
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDesignDocumentMap(docAWithoutRev, docB2WithoutRev, docCWithoutRev)));

			Assert.True(engine.CheckIfChanged(new Uri("http://example.com")));
		}

		[Fact]
		public void ShouldPushNewDocumensWithoutRevisionsWithPut()
		{
			var httpClientMock = new HttpClientMock();
			
			var engine = new Engine(
				httpClientMock,
				Mock.Of<IDesignDocumentExtractor>(
					e => e.Extract(It.IsAny<TextReader>()) == new Dictionary<string, DesignDocument>(0)
				),
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDesignDocumentMap(docAWithoutRev))
			);

			engine.PushIfChanged(new Uri("http://example.com"));

			Assert.Equal("http://example.com/_design/doc1", httpClientMock.Request.RequestUri.ToString());
			Assert.Equal("PUT", httpClientMock.Request.Method.ToString());
			Assert.Equal(docAWithoutRev.ToString(), httpClientMock.Request.Content.GetTextReader().ReadToEnd(), new JTokenStringCompairer());
		}
		
		[Fact]
		public void ShouldPushUpdatedDocumensWithDbDocumentRevisionWithPut()
		{
			var httpClientMock = new HttpClientMock();
			
			var engine = new Engine(
				httpClientMock,
				Mock.Of<IDesignDocumentExtractor>(
					e => e.Extract(It.IsAny<TextReader>()) == CreateDesignDocumentMap(docB)
				),
				Mock.Of<IDesignDocumentAssembler>(a => a.Assemble() == CreateDesignDocumentMap(docB2WithoutRev))
			);

			engine.PushIfChanged(new Uri("http://example.com"));

			Assert.Equal("http://example.com/_design/doc2", httpClientMock.Request.RequestUri.ToString());
			Assert.Equal("PUT", httpClientMock.Request.Method.ToString());
			
			var expectedDoc = (JObject)docB2WithoutRev.DeepClone();
			expectedDoc["_rev"] = docB["_rev"];
			Assert.Equal(expectedDoc.ToString(), httpClientMock.Request.Content.GetTextReader().ReadToEnd(), new JTokenStringCompairer());
		}

		private static Dictionary<string, DesignDocument> CreateDesignDocumentMap(params JObject[] objects)
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
