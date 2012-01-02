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
using System.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDude.Api
{
	/// <summary><see cref="DocumentAttachment"/> implementation returning from attachment request.</summary>
	internal class HttpResponseMessageDocumentAttachment: DocumentAttachment
	{
		private readonly HttpContent content;

		/// <constructor />
		public HttpResponseMessageDocumentAttachment(string id, HttpResponseMessage responseMessage)
			: this (id, new Document(), responseMessage) { }

		/// <constructor />
		private HttpResponseMessageDocumentAttachment(
			string id, Document fakeDocument, HttpResponseMessage responseMessage): base(id, fakeDocument)
		{
			content = responseMessage.Content;

			var contentHeaders = responseMessage.Content.Headers;
			var fakeDescriptor = new JsonObject(
				new KeyValuePair<string, JsonValue>(LengthPropertyName, (int)contentHeaders.ContentLength.GetValueOrDefault(0)),
				new KeyValuePair<string, JsonValue>(StubPropertyName, true),
				new KeyValuePair<string, JsonValue>(ContentTypePropertyName, contentHeaders.ContentType.MediaType)
			);
			fakeDocument.RawJsonObject[DocumentAttachmentBag.AttachmentsPropertyName] =
				new JsonObject(new KeyValuePair<string, JsonValue>(id, fakeDescriptor));
		}

		[JetBrains.Annotations.TerminatesProgram]
		private void ThrowNotImplemented() { throw new NotImplementedException("Instanece is read-only"); }
		
		public override Task<Stream> OpenRead() { return content.ReadAsStreamAsync(); }

		public override void SetData(Stream dataStream) { ThrowNotImplemented(); }
	}
}