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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CouchDude.Utils;

namespace CouchDude
{
	/// <summary></summary>
	public class DocumentAttachmentBag: IEnumerable<DocumentAttachment>
	{
		internal const string AttachmentsPropertyName = "_attachments";

		private readonly Document parentDocument;
		private readonly ConditionalWeakTable<string, DocumentAttachment> documentAttachmentInstances =
			new ConditionalWeakTable<string, DocumentAttachment>();
		
		/// <constructor />
		public DocumentAttachmentBag(Document parentDocument) { this.parentDocument = parentDocument; }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public IEnumerator<DocumentAttachment> GetEnumerator()
		{
			var attachments =
				from pair in GetDescriptorIdPairs()
				select GetOrCreateDocumentAttachment(pair.Key);
			return attachments.GetEnumerator();
		}

		/// <inheritdoc />
		public void Delete(string id)
		{
			var attachmentsObject = GetAttachmentsObject();
			if (attachmentsObject != null)
				attachmentsObject.Remove(id);
		}

		/// <inheritdoc />
		public DocumentAttachment this[string attachmentId]
		{
			get
			{
				var attachmentsObject = GetAttachmentsObject();
				if (attachmentsObject != null)
				{
					var attachmentDescriptor = attachmentsObject[attachmentId] as JsonObject;
					if (attachmentDescriptor != null)
						return GetOrCreateDocumentAttachment(attachmentId);
				}
				return null;
			}
		}

		private IEnumerable<KeyValuePair<string, JsonObject>> GetDescriptorIdPairs()
		{
			var attachmentsObject = GetAttachmentsObject();
			if (attachmentsObject == null)
				return Enumerable.Empty<KeyValuePair<string, JsonObject>>();
			return from property in attachmentsObject
						 let descriptor = property.Value as JsonObject
						 where descriptor != null
						 select new KeyValuePair<string, JsonObject>(property.Key, descriptor);
		}

		private DocumentAttachment CreateNewAttachment(string id, string contentType = null)
		{
			if (id.HasNoValue()) throw new ArgumentNullException("id");
			var attachmensObject = GetOrCreateAttachmentsObject();
			if (attachmensObject.ContainsKey(id)) 
				throw new InvalidOperationException(string.Format("Attachment with ID:{0} already present on document", id));

			var attachmet = new DocumentAttachment(id, parentDocument);
			if (contentType.HasValue())
				attachmet.ContentType = contentType;
			return attachmet;
		}

		/// <summary>Creates inline document attachment from provided string.</summary>
		public DocumentAttachment Create(string id, string stringData, string contentType = "text/plain")
		{
			if (stringData == null) throw new ArgumentNullException("stringData");
			
			return Create(id, Encoding.UTF8.GetBytes(stringData), contentType);
		}

		/// <summary>Creates inline document attachment from provided byte array.</summary>
		public DocumentAttachment Create(string id, byte[] rawData, string contentType = "application/octet-stream")
		{
			if (rawData == null) throw new ArgumentNullException("rawData");
			
			using (var memoryStream = new MemoryStream(rawData))
				return Create(id, memoryStream, contentType);
		}

		/// <summary>Creates new inline attachment reading data from provided stream.</summary>
		public DocumentAttachment Create(string id, Stream dataStream, string contentType = "application/octet-stream")
		{
			if (dataStream == null) throw new ArgumentNullException("dataStream");

			var attachment = CreateNewAttachment(id, contentType);
			attachment.SetData(dataStream);
			return attachment;
		}

		private DocumentAttachment GetOrCreateDocumentAttachment(string attachmentId)
		{
			return documentAttachmentInstances.GetValue(
				attachmentId, id => new DocumentAttachment(attachmentId, parentDocument));
		}

		private JsonObject GetAttachmentsObject()
		{
			return parentDocument.RawJsonObject[AttachmentsPropertyName] as JsonObject;
		}

		private JsonObject GetOrCreateAttachmentsObject()
		{
			var attachmentsObject = GetAttachmentsObject();
			if (attachmentsObject == null)
				parentDocument.RawJsonObject[AttachmentsPropertyName] = attachmentsObject = new JsonObject();
			return attachmentsObject;
		}
	}
}