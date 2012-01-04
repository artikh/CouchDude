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
using System.Threading.Tasks;
using CouchDude.Api;

namespace CouchDude
{
	/// <summary>Document attachment.</summary>
	public class DocumentAttachment
	{
		static readonly MemoryStream EmptyStream = new MemoryStream(0);

		Stream setStream = EmptyStream;

		/// <constructor />
		public DocumentAttachment(string id) { Id = id; }

		/// <summary>Unique (within documnet) identifier of the attachment.</summary>
		public string Id { get; private set; }

		/// <summary>Attachment content (MIME) type.</summary>
		public virtual string ContentType { get; set; }

		/// <summary>Content length.</summary>
		public virtual long Length { get; private set; }

		/// <summary>Indicates wether attachment is included as base64 string within document or should 
		/// be requested separatly.</summary>
		public virtual bool Inline { get; set; }

		/// <summary>Syncrounous wrappers over async </summary>
		public ISyncronousDocumentAttachment Syncronously { get { return new SyncronousDocumentAttachmentWrapper(this); } }

		/// <summary>Open attachment data stream for read.</summary>
		public virtual Task<Stream> OpenRead()
		{
			return TaskEx.FromResult(setStream);
		}

		/// <summary>Converts sets attachment data (inline). Attachment gets saved with parent document.</summary>
		public virtual void SetData(Stream dataStream)
		{
			setStream = dataStream;
			try
			{
				Length = setStream.Length;
			}
			catch (NotSupportedException) { }
		}
	}
}