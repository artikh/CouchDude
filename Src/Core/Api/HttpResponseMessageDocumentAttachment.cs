#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov																					
																																					
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
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CouchDude.Api
{
	/// <summary><see cref="IDocumentAttachment"/> implementation returning from attachment request.</summary>
	internal class HttpResponseMessageDocumentAttachment: JsonFragment, IDocumentAttachment
	{
		private readonly string id;
		private readonly HttpResponseMessage responseMessage;
		private readonly HttpContentHeaders contentHeaders;

		/// <constructor />
		public HttpResponseMessageDocumentAttachment(string id, HttpResponseMessage responseMessage)
		{
			this.id = id;
			this.responseMessage = responseMessage;
			contentHeaders = responseMessage.Content.Headers;
		}

		public string Id { get { return id; } }

		// ReSharper disable ValueParameterNotUsed
		public string ContentType { get { return contentHeaders.ContentType.MediaType; } set { ThrowNotImplemented(); } }

		public int Length { get { return (int)contentHeaders.ContentLength.GetValueOrDefault(0); } set { ThrowNotImplemented(); } }
		// ReSharper restore ValueParameterNotUsed

		[JetBrains.Annotations.TerminatesProgram]
		private void ThrowNotImplemented() { throw new NotImplementedException("Instanece is read-only"); }

		public bool Inline { get { return false; } }

		public Task<Stream> OpenRead()
		{
			return responseMessage.Content.LoadIntoBufferAsync().ContinueWith(lt => responseMessage.Content.ContentReadStream);
		}

		public void SetData(Stream dataStream) { ThrowNotImplemented(); }

		public ISyncronousDocumentAttachment Syncronously { get { return new SyncronousDocumentAttachmentWrapper(this); } }

		private JToken jsonToken;
		protected override JToken JsonToken
		{
			get
			{
				return jsonToken ?? (jsonToken = JObject.FromObject(new {
					content_type = ContentType,
					length = Length,
					stub = true
				}));
			}
		}
	}
}