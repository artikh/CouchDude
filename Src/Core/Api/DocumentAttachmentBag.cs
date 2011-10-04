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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CouchDude.Utils;
using Newtonsoft.Json.Linq;

namespace CouchDude.Api
{
	internal class DocumentAttachmentBag : IDocumentAttachmentBag
	{
		private const string AttachmentsPropertyName = "_attachments";

		private readonly Document parentDocument;
		private readonly ConditionalWeakTable<JObject, IDocumentAttachment> documentAttachmentInstances =
 			new ConditionalWeakTable<JObject, IDocumentAttachment>();
		
		/// <constructor />
		public DocumentAttachmentBag(Document parentDocument) { this.parentDocument = parentDocument; }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		
		public IEnumerator<IDocumentAttachment> GetEnumerator()
		{
			var attachments =
				from pair in GetDescriptorIdPairs()
				select GetOrCreateDocumentAttachment(pair.Key, pair.Value);
			return attachments.GetEnumerator();
		}

		public void Delete(string id)
		{
			var attachmentsObject = GetAttachmentsObject();
			if (attachmentsObject != null)
				attachmentsObject.Remove(id);
		}

		public IDocumentAttachment this[string attachmentId]
		{
			get
			{
				var attachmentsObject = GetAttachmentsObject();
				if (attachmentsObject != null)
				{
					var attachmentDescriptor = attachmentsObject[attachmentId] as JObject;
					if (attachmentDescriptor != null)
						return GetOrCreateDocumentAttachment(attachmentId, attachmentDescriptor);
				}
				return null;
			}
		}

		private IEnumerable<KeyValuePair<string, JObject>> GetDescriptorIdPairs()
		{
			var attachmentsObject = GetAttachmentsObject();
			if (attachmentsObject == null)
				return Enumerable.Empty<KeyValuePair<string, JObject>>();
			return from property in attachmentsObject.Properties()
						 let descriptor = property.Value as JObject
						 where descriptor != null
						 select new KeyValuePair<string, JObject>(property.Name, descriptor);
		}

		private IDocumentAttachment CreateNewAttachment(string id, string contentType = null)
		{
			if (id.HasNoValue()) throw new ArgumentNullException("id");
			var attachmensObject = GetOrCreateAttachmentsObject();
			if (attachmensObject.Property(id) != null) 
				throw new InvalidOperationException(string.Format("Attachment with ID:{0} already present on document", id));

			var attachmet = new WrappingDocumentAttachment(id, parentDocument);
			documentAttachmentInstances.Add(attachmet.JsonObject, attachmet);
			attachmensObject[id] = attachmet.JsonObject;	
			if (contentType.HasValue())
				attachmet.ContentType = contentType;
			return attachmet;
		}

		public IDocumentAttachment Create(string id, string stringData, string contentType = "text/plain")
		{
			if (stringData == null) throw new ArgumentNullException("stringData");
			
			return Create(id, Encoding.UTF8.GetBytes(stringData), contentType);
		}

		public IDocumentAttachment Create(string id, byte[] rawData, string contentType = "application/octet-stream")
		{
			if (rawData == null) throw new ArgumentNullException("rawData");
			
			using (var memoryStream = new MemoryStream(rawData))
				return Create(id, memoryStream, contentType);
		}
		
		public IDocumentAttachment Create(string id, Stream dataStream, string contentType = "application/octet-stream")
		{
			if (dataStream == null) throw new ArgumentNullException("dataStream");

			var attachment = CreateNewAttachment(id, contentType);
			attachment.SetData(dataStream);
			return attachment;
		}

		private IDocumentAttachment GetOrCreateDocumentAttachment(string id, JObject descriptor)
		{
			return documentAttachmentInstances.GetValue(descriptor, _ => new WrappingDocumentAttachment(id, parentDocument));
		}

		private JObject GetAttachmentsObject()
		{
			return parentDocument.JsonObject[AttachmentsPropertyName] as JObject;
		}

		private JObject GetOrCreateAttachmentsObject()
		{
			var attachmentsObject = GetAttachmentsObject();
			if (attachmentsObject == null)
				parentDocument.JsonObject[AttachmentsPropertyName] = attachmentsObject = new JObject();
			return attachmentsObject;
		}
	}
}