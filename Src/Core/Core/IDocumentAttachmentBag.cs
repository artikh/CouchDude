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

using System.Collections.Generic;
using System.IO;

namespace CouchDude
{
	/// <summary>Document attachments.</summary>
	public interface IDocumentAttachmentBag: IEnumerable<IDocumentAttachment>
	{
		/// <summary>Creates new text attachment.</summary>
		/// <remarks>New attachment's data is embeded to parent document and being saved when documeint is saved. You
		/// should use <see cref="IDatabaseApi.SaveAttachment"/> if attachment is massive.</remarks>
		IDocumentAttachment Create(string id, string stringData, string contentType = "text/plain");

		/// <summary>Creates new binary attachment using provided buffer.</summary>
		/// <remarks>New attachment's data is embeded to parent document and being saved when documeint is saved. You
		/// should use <see cref="IDatabaseApi.SaveAttachment"/> if attachment is massive.</remarks>
		IDocumentAttachment Create(string id, byte[] rawData, string contentType = "application/octet-stream");

		/// <summary>Creates new binary attachment using provided stream.</summary>
		/// <remarks>New attachment's data is embeded to parent document and being saved when documeint is saved. You
		/// should use <see cref="IDatabaseApi.SaveAttachment"/> if attachment is massive.</remarks>
		IDocumentAttachment Create(string id, Stream dataStream, string contentType = "application/octet-stream");

		/// <summary>Removes attachment descriptor resulting in deletion of attachment itself when document is saved.</summary>
		void Delete(string id);

		/// <summary>Provides access to collection of document's attachments.</summary>
		IDocumentAttachment this[string attachmentId] { get; }
	}
}