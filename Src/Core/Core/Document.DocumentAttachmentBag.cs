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
	public partial class Document
	{
		/// <summary>Document's attachment colloction.</summary>
		public class DocumentAttachmentBag : IEnumerable<Attachment>
		{
			readonly Document parentDocument;

			readonly ConditionalWeakTable<string, Attachment>
				documentAttachmentInstances = new ConditionalWeakTable<string, Attachment>();

			/// <constructor />
			public DocumentAttachmentBag(Document parentDocument) { this.parentDocument = parentDocument; }

			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

			/// <inheritdoc />
			public IEnumerator<Attachment> GetEnumerator()
			{
				return
					(from pair in GetDescriptorIdPairs() select GetOrCreateDocumentAttachment(pair.Key)).
						GetEnumerator();
			}

			/// <inheritdoc />
			public void Delete(string attachmentId)
			{
				if (string.IsNullOrWhiteSpace(attachmentId)) throw new ArgumentNullException("attachmentId");

				var attachmentsObject = parentDocument.RawJsonObject.GetObjectProperty(AttachmentsPropertyName);
				if (attachmentsObject == null || !attachmentsObject.ContainsKey(attachmentId))
					throw new KeyNotFoundException(
						string.Format("Attachment with id {0} have not found on the document", attachmentId));
				attachmentsObject.Remove(attachmentId);
			}

			/// <inheritdoc />
			public Attachment this[string attachmentId]
			{
				get
				{
					if (string.IsNullOrWhiteSpace(attachmentId)) throw new ArgumentNullException("attachmentId");

					var attachmentDescriptor =
						parentDocument.RawJsonObject.GetObjectProperty(AttachmentsPropertyName).GetObjectProperty(
							attachmentId);
					if (attachmentDescriptor != null)
						return GetOrCreateDocumentAttachment(attachmentId);
					return null;
				}
			}

			IEnumerable<KeyValuePair<string, JsonObject>> GetDescriptorIdPairs()
			{
				var attachmentsObject = parentDocument.RawJsonObject.GetObjectProperty(AttachmentsPropertyName);
				if (attachmentsObject == null)
					return Enumerable.Empty<KeyValuePair<string, JsonObject>>();
				return from property in attachmentsObject
				       let descriptor = property.Value as JsonObject
				       where descriptor != null
				       select new KeyValuePair<string, JsonObject>(property.Key, descriptor);
			}

			Attachment CreateNewAttachment(string attachmentId, string contentType = null)
			{
				var attachmensObject =
					parentDocument.RawJsonObject.GetOrCreateObjectProperty(AttachmentsPropertyName);
				if (attachmensObject.ContainsKey(attachmentId))
					throw new InvalidOperationException(
						string.Format("Attachment with ID:{0} already present on document", attachmentId));
				attachmensObject[attachmentId] = new JsonObject();

				var attachmet = GetOrCreateDocumentAttachment(attachmentId);
				if (contentType.HasValue())
					attachmet.ContentType = contentType;
				return attachmet;
			}

			/// <summary>Creates inline document attachment from provided string.</summary>
			public Attachment Create(
				string attachmentId, string stringData, string contentType = "text/plain")
			{
				if (string.IsNullOrWhiteSpace(attachmentId)) throw new ArgumentNullException("attachmentId");
				if (stringData == null) throw new ArgumentNullException("stringData");

				return Create(attachmentId, Encoding.UTF8.GetBytes(stringData), contentType);
			}

			/// <summary>Creates inline document attachment from provided byte array.</summary>
			public Attachment Create(
				string attachmentId, byte[] rawData, string contentType = "application/octet-stream")
			{
				if (string.IsNullOrWhiteSpace(attachmentId)) throw new ArgumentNullException("attachmentId");
				if (rawData == null) throw new ArgumentNullException("rawData");

				using (var memoryStream = new MemoryStream(rawData))
					return Create(attachmentId, memoryStream, contentType);
			}

			/// <summary>Creates new inline attachment reading data from provided stream.</summary>
			public Attachment Create(
				string attachmentId, Stream dataStream, string contentType = "application/octet-stream")
			{
				if (string.IsNullOrWhiteSpace(attachmentId)) throw new ArgumentNullException("attachmentId");
				if (dataStream == null) throw new ArgumentNullException("dataStream");

				var attachment = CreateNewAttachment(attachmentId, contentType);
				attachment.SetData(dataStream);
				return attachment;
			}

			Attachment GetOrCreateDocumentAttachment(string attachmentId)
			{
				return documentAttachmentInstances.GetValue(
					attachmentId, id => new WrappingAttachment(attachmentId, parentDocument));
			}
		}
	}
}