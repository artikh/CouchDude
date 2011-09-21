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
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CouchDude.Api
{
	internal class AttachmentBag : IAttachmentBag
	{
		private const string AttachmentsPropertyName = "_attachments";
		private readonly JObject documentJObject;
		private JObject attachmentsJObject;
		
		/// <constructor />
		public AttachmentBag(JObject documentJObject) { this.documentJObject = documentJObject; }


		public IDocumentAttachment this[string attachmentId]
		{
			get
			{
				var attachmentJObject = GetAttachmentJObject(attachmentId);
				return attachmentJObject == null ? null : new DocumentAttachment(attachmentId, attachmentJObject);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public IEnumerator<IDocumentAttachment> GetEnumerator()
		{
			var attachmentsJObject = AttachmentsJObject;
			if (attachmentsJObject != null)
			{
				var attachments = from prop in attachmentsJObject.Properties()
				                  select new {prop.Name, Value = prop.Value as JObject}
				                  into prop
				                  where prop.Value != null
				                  select new DocumentAttachment(prop.Name, prop.Value);
				foreach (var attachment in attachments)
					yield return attachment;
			}
		}

		/// <inheritdoc />
		public void Add(IDocumentAttachment attachment)
		{
			if (attachment == null) throw new ArgumentNullException("attachment");

			var attachmentsJObject = AttachmentsJObject;
			if(attachmentsJObject == null)
				documentJObject[AttachmentsPropertyName] = attachmentsJObject = new JObject();

			var attachmentJObject = GetAttachmentJObject(attachment.Name);
			if (attachmentJObject != null)
				attachmentJObject.Remove();

			attachmentJObject = JObject.Parse(attachment.ToString());
			attachmentsJObject.Add(attachment.Name, attachmentJObject);
		}

		/// <inheritdoc />
		public void Clear()
		{
			var attachmentsJObject = documentJObject[AttachmentsPropertyName] as JObject;
			if (attachmentsJObject != null)
				attachmentsJObject.Remove();
		}

		public bool Contains(IDocumentAttachment attachment)
		{
			if (attachment == null) throw new ArgumentNullException("attachment");
			return GetAttachmentJObject(attachment.Name) != null;
		}

		public void CopyTo(IDocumentAttachment[] array, int arrayIndex)
		{
			this.ToList().CopyTo(array, arrayIndex);
		}

		public bool Remove(IDocumentAttachment attachment)
		{
			if (attachment == null) throw new ArgumentNullException("attachment");
			var attachmentJObject = GetAttachmentJObject(attachment.Name);
			if(attachmentJObject != null)
			{
				attachmentJObject.Remove();
				return true;
			}
			return false;
		}

		/// <inheritdoc />
		public int Count
		{
			get
			{
				// This should be dynamic as properties of _attachment property could be something other then objects resulting
				// in not counting them as attachment descriptors. Unlikely, but possible.
				var count = 0;
				using (var enumerator = GetEnumerator())
					checked
					{
						while (enumerator.MoveNext()) count++;
					}
				return count;
			}
		}

		public bool IsReadOnly { get { return false; } }

		private JObject GetAttachmentJObject(string attachmentId)
		{
			var attachments = AttachmentsJObject;
			if (attachments != null)
			{
				var attachmentProperty = attachments.Property(attachmentId);
				if (attachmentProperty != null) 
					return attachmentProperty.Value as JObject;
			}

			return null;
		}

		private JObject AttachmentsJObject
		{
			get
			{
				return attachmentsJObject ?? (attachmentsJObject = documentJObject[AttachmentsPropertyName] as JObject);
			}
		}
	}
}