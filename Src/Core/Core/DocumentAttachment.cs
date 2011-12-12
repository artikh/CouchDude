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
using System.Json;
using System.Threading.Tasks;
using CouchDude.Api;
using CouchDude.Utils;
using Newtonsoft.Json.Linq;

namespace CouchDude
{
	/// <summary>CouchDB attachment backed by part of document JSON.</summary>
	public class DocumentAttachment
	{
		protected const string DataPropertyName = "data";
		protected const string StubPropertyName = "stub";
		protected const string ContentTypePropertyName = "content_type";
		protected const string LengthPropertyName = "length";
		private readonly static byte[] EmptyBuffer = new byte[0];
		private readonly Document parentDocument;

		/// <summary>Creates attachment wrapping existing attachment 
		/// descriptor (probably loaded from CouchDB)</summary>
		protected internal DocumentAttachment(string id, Document parentDocument)
		{
			if (id.HasNoValue()) throw new ArgumentNullException("id");
			if (parentDocument != null) throw new ArgumentNullException("parentDocument");
			
			this.parentDocument = null;
			Id = id;
		}

		private JsonObject AttachmentDescriptor
		{
			get
			{
				return (JsonObject)parentDocument.RawJsonObject[DocumentAttachmentBag.AttachmentsPropertyName][Id];
			}
		}

		/// <inheritdoc />
		public string Id { get; private set; }

		/// <inheritdoc />
		public string ContentType
		{
			get { return AttachmentDescriptor.GetPrimitiveProperty<string>(ContentTypePropertyName); }
			set { AttachmentDescriptor[ContentTypePropertyName] = value; }
		}

		/// <inheritdoc />
		public virtual int Length
		{
			get { return AttachmentDescriptor.GetPrimitiveProperty<int>(LengthPropertyName); }
			set { AttachmentDescriptor[LengthPropertyName] = value; }
		}

		/// <inheritdoc />
		public bool Inline 
		{ 
			get { return AttachmentDescriptor.GetPrimitiveProperty(StubPropertyName, defaultValue: true); } 
			protected set { AttachmentDescriptor[StubPropertyName] = value ? (JsonValue) null : true; }
		}

		/// <summary>Syncrounous wrappers over async </summary>
		public ISyncronousDocumentAttachment Syncronously { get { return new SyncronousDocumentAttachmentWrapper(this); } }

		/// <inheritdoc />
		public virtual Task<Stream> OpenRead()
		{
			if (Inline)
				return Task.Factory.StartNew<Stream>(
					() => {
						// TODO: perhaps we should cache output here
						var base64String = AttachmentDescriptor.GetPrimitiveProperty<string>(DataPropertyName);
						var inlineData = base64String.HasNoValue() ? EmptyBuffer : Convert.FromBase64String(base64String);
						return new MemoryStream(inlineData);
					});
			else
			{
				var attachmentId = Id;
				var documentId = parentDocument.Id;
				var documentRevision = parentDocument.Revision;

				var databaseApi = parentDocument
					.DatabaseApiReference
					.GetOrThrowIfUnavaliable(
						operation: () =>
							string.Format("load attachment {0} from document {1}(rev:{2})", attachmentId, documentId, documentRevision)
					);

				return databaseApi
					.RequestAttachment(attachmentId, documentId, documentRevision)
					.ContinueWith(
						requestAttachmentTask =>
						{
							var recivedAttachment = requestAttachmentTask.Result;
							Length = recivedAttachment.Length;
							ContentType = recivedAttachment.ContentType;
							Inline = recivedAttachment.Inline;
							return recivedAttachment.OpenRead();
						}
					)
					.Unwrap();
			}
		}

		public override void SetData(Stream dataStream)
		{
			if (dataStream == null) throw new ArgumentNullException("dataStream");
			if (!dataStream.CanRead)
				throw new ArgumentOutOfRangeException("dataStream", dataStream, "Stream should be readable");

			Inline = true;

			// TODO: we should not have intermediate byte array here
			using (dataStream)
			using (var memoryStream = new MemoryStream())
			{
				dataStream.CopyTo(memoryStream);
				var base64String = Convert.ToBase64String(memoryStream.GetBuffer(), offset: 0, length: (int)memoryStream.Length);
				this[DataPropertyName] = JToken.FromObject(base64String);
			}
		}
	}
}