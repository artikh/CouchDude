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

using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CouchDude.Api
{
	internal abstract class DocumentAttachmentBase : JsonObject, IDocumentAttachment
	{
		protected const string StubPropertyName = "stub";
		protected const string ContentTypePropertyName = "content_type";
		protected const string LengthPropertyName = "length";

		/// <constructor />
		protected DocumentAttachmentBase(string id) { Id = id; }

		/// <constructor />
		protected DocumentAttachmentBase(string id, JObject jObject): base(jObject) { Id = id; }

		/// <inheritdoc />
		public string Id { get; private set; }

		/// <inheritdoc />
		public string ContentType
		{
			get { return Value<string>(ContentTypePropertyName); }
			set { this[ContentTypePropertyName] = JToken.FromObject(value); }
		}

		/// <inheritdoc />
		public virtual int Length
		{
			get { return Value<int>(LengthPropertyName); }
			set { this[LengthPropertyName] = JToken.FromObject(value); }
		}

		/// <inheritdoc />
		public bool Inline 
		{ 
			get
			{
				var stubProperty = Property(StubPropertyName);
				if (stubProperty == null) return true;
				var stubPropertyValue = stubProperty.Value;
				return stubPropertyValue.Type != JTokenType.Boolean || !stubPropertyValue.Value<bool>();
			} 
			protected set
			{
				if (value)
					Remove(StubPropertyName);
				else
					this[StubPropertyName] = JToken.FromObject(true);
			} 
		}

		public ISyncronousDocumentAttachment Syncronously { get { return new SyncronousDocumentAttachmentWrapper(this); } }

		/// <inheritdoc />
		public abstract Task<Stream> OpenRead();

		public abstract void SetData(Stream dataStream);
	}
}