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
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDude.Api
{
	/// <summary><see cref="DocumentAttachment"/> implementation returning from attachment request.</summary>
	internal class HttpResponseMessageDocumentAttachment: DocumentAttachment
	{
		private readonly HttpContent httpContent;
		
		/// <constructor />
		public HttpResponseMessageDocumentAttachment(string id, HttpResponseMessage responseMessage): base(id)
		{
			httpContent = responseMessage.Content;
		}
		
		[JetBrains.Annotations.TerminatesProgram]
		private static void ThrowNotImplemented() { throw new NotImplementedException("Instanece is read-only"); }
		
		public override Task<Stream> OpenRead() { return httpContent.ReadAsStreamAsync(); }

		public override void SetData(Stream dataStream) { ThrowNotImplemented(); }

		public override string ContentType
		{
			get { return httpContent.Headers.ContentType.MediaType; } 
			// ReSharper disable ValueParameterNotUsed
			set { ThrowNotImplemented(); }
			// ReSharper restore ValueParameterNotUsed
		}

		public override long Length { get { return httpContent.Headers.ContentLength ?? 0; } }

		public override bool Inline
		{
			get { return false; }
			// ReSharper disable ValueParameterNotUsed
			set { ThrowNotImplemented(); }
			// ReSharper restore ValueParameterNotUsed
		}
	}
}