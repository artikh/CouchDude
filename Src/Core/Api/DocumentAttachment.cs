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
using CouchDude.Utils;
using Newtonsoft.Json.Linq;

namespace CouchDude.Api
{
	/// <summary>CouchDB attacment implemented by wrapping the <see cref="AttachmentDescriptor"/>.</summary>
	public class DocumentAttachment: IDocumentAttachment
	{
		private const string StubPropertyName = "stub";
		private const string DataPropertyName = "data";
		private const string ContentTypePropertyName = "content_type";
		private const string LengthPropertyName = "length";

		internal JObject AttachmentDescriptor;

		/// <summary>Creates attachment wrapping existing attachment 
		/// descriptor (probably loaded from CouchDB)</summary>
		internal DocumentAttachment(string name, JObject attachmentDescriptor)
		{
			if (name.HasNoValue())
				throw new ArgumentNullException("name");
			if (attachmentDescriptor == null)
				throw new ArgumentNullException("attachmentDescriptor");

			Initialize(name, attachmentDescriptor);
		}

		/// <constructor />
		public DocumentAttachment(string name)
		{
			if (name.HasNoValue())
				throw new ArgumentNullException("name");

			Initialize(name, new JObject());
		}

		/// <constructor />
		public DocumentAttachment(string name, string attachmentDescriptorJson)
		{
			if (name.HasNoValue())
				throw new ArgumentNullException("name");
			if (attachmentDescriptorJson.HasNoValue())
				throw new ArgumentNullException("attachmentDescriptorJson");

			var attachmentDescriptor = JsonFragment.Parse(attachmentDescriptorJson) as JObject;
			if(attachmentDescriptor == null)
				throw new ParseException("Provided string should be valid JSON object.");

			Initialize(name, attachmentDescriptor);
		}

		private void Initialize(string attachmentName, JObject attachmentDescriptor)
		{
			Name = attachmentName;
			AttachmentDescriptor = attachmentDescriptor;
			if (ContentType.HasNoValue())
				ContentType = "application/octet-stream";
		}

		/// <inheritdoc />
		public string Name { get; private set; }

		/// <inheritdoc />
		public string ContentType
		{
			get { return AttachmentDescriptor.Value<string>(ContentTypePropertyName); }
			set { AttachmentDescriptor[ContentTypePropertyName] = JToken.FromObject(value); }
		}

		/// <inheritdoc />
		public int Length
		{
			get { return AttachmentDescriptor.Value<int>(LengthPropertyName); }
			set { AttachmentDescriptor[LengthPropertyName] = JToken.FromObject(value); }
		}

		/// <inheritdoc />
		public bool Inline 
		{ 
			get
			{
				var stubProperty = AttachmentDescriptor.Property(StubPropertyName);
				if (stubProperty == null) return true;
				var stubPropertyValue = stubProperty.Value;
				return stubPropertyValue.Type != JTokenType.Boolean || !stubPropertyValue.Value<bool>();
			} 
			set
			{
				if (value)
					AttachmentDescriptor.Remove(StubPropertyName);
				else
					AttachmentDescriptor[StubPropertyName] = JToken.FromObject(true);
			} 
		}

		/// <inheritdoc />
		public byte[] InlineData
		{
			get
			{
				var base64String = AttachmentDescriptor.Value<string>(DataPropertyName);
				return base64String.HasNoValue()? null: Convert.FromBase64String(base64String);
			}
			set
			{
				AttachmentDescriptor[DataPropertyName] =
					value == null ? null : JToken.FromObject(Convert.ToBase64String(value));
			}
		}

		/// <inheritdoc />
		public override string ToString() { return AttachmentDescriptor.ToString(); }
	}
}