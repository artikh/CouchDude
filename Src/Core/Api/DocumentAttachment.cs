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

		private readonly string name;
		internal JObject AttachmentDescriptor;

		/// <summary>Creates attachment wrapping existing attachment 
		/// descriptor (probably loaded from CouchDB)</summary>
		internal DocumentAttachment(string name, JObject attachmentDescriptor)
		{
			if (name.HasNoValue())
				throw new ArgumentNullException("name");
			if (attachmentDescriptor == null)
				throw new ArgumentNullException("attachmentDescriptor");


			this.name = name;
			AttachmentDescriptor = attachmentDescriptor;
		}

		/// <constructor />
		public DocumentAttachment(string name): this(name, new JObject())
		{
			if (name.HasNoValue())
				throw new ArgumentNullException("name");

			this.name = name;
			AttachmentDescriptor = new JObject();
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
			this.name = name;
			AttachmentDescriptor = attachmentDescriptor;
		}

		/// <inheritdoc />
		public string Name { get { return name; } }

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
		public Stream OpenRead()
		{
			var dataString = AttachmentDescriptor.Value<string>(DataPropertyName);
			var bytes = Convert.FromBase64String(dataString);
			return new MemoryStream(bytes);
		}

		/// <inheritdoc />
		public Stream OpenWrite()
		{
			Inline = true;
			return new DataStreamWrapper(this);
		}

		/// <inheritdoc />
		public override string ToString() { return AttachmentDescriptor.ToString(); }

		private void SaveData(byte[] buffer, int length) 
		{ 
			var base64String = Convert.ToBase64String(buffer, offset: 0, length: length);
			AttachmentDescriptor[DataPropertyName] = JToken.FromObject(base64String);
		}

		private class DataStreamWrapper: MemoryStream
		{
			private readonly DocumentAttachment parent;
			public DataStreamWrapper(DocumentAttachment parent) { this.parent = parent; }

			public override void Flush()
			{
				parent.SaveData(GetBuffer(), (int)Length);
				base.Flush();
			}
		}
	}
}