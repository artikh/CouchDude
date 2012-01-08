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
using System.Text;
using System.Threading.Tasks;
using CouchDude.Utils;

namespace CouchDude
{
	/// <summary>CouchDB document attachment.</summary>
	public class Attachment
	{
		/// <summary>Default MIME type document attachment should declare.</summary>
		protected const string DefaultContentType = "application/octet-stream";

		string contentType;
		Stream setStream;

		/// <constructor />
		public Attachment(string id) { Id = id; }

		/// <summary>Unique (within documnet) identifier of the attachment.</summary>
		public string Id { get; private set; }

		/// <summary>Attachment content (MIME) type.</summary>
		public virtual string ContentType
		{
			get { return contentType ?? DefaultContentType; } set { contentType = value; }
		}

		/// <summary>Attachment data length.</summary>
		public virtual long Length { get; private set; }

		/// <summary>Syncrounous versions of async <see cref="Attachment"/> methods.</summary>
		public SyncronousDocumentAttachmentMethods Syncronously
		{
			get { return new SyncronousDocumentAttachmentMethods(this); }
		}

		/// <summary>Open attachment data stream for read.</summary>
		/// <remarks>Returned stream could be unseakable. 
		/// Same instance of stream could be returned every time method invoked.</remarks>
		public virtual Task<Stream> OpenRead()
		{
			return TaskEx.FromResult(setStream ?? new MemoryStream(0));
		}

		/// <summary>Sets attachment data. Bytes are written to database with parent document.</summary>
		public virtual void SetData(Stream dataStream)
		{
			setStream = dataStream;
			try { Length = setStream.Length; }
			catch (NotSupportedException) { }
		}

		/// <summary>Sets attachment data. Bytes are written to database with parent document.</summary>
		public void SetData(byte[] data)
		{
			SetData(new MemoryStream(data));
		}

		/// <summary>Sets attachment data as UTF-8 string. 
		/// Bytes are written to database with parent document.</summary>
		public void SetData(string data)
		{
			SetData(Encoding.UTF8.GetBytes(data));
		}

		/// <summary>Synchronous version of <see cref="Attachment"/> methods.</summary>
		public struct SyncronousDocumentAttachmentMethods
		{
			private readonly Attachment parent;

			/// <constructor />
			public SyncronousDocumentAttachmentMethods(Attachment parent) : this() { this.parent = parent; }

			/// <summary>Open attachment data stream for read and waits for result.</summary>
			/// <remarks>Returned stream could be unseakable. 
			/// Same instance of stream could be returned every time method invoked.</remarks>
			public Stream OpenRead() { return parent.OpenRead().WaitForResult(); }
		}
	}
}