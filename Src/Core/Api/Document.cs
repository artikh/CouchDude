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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CouchDude.Api
{
	/// <summary>CouchDB document implemented wrapping <see cref="JObject"/>.</summary>
	/// <remarks>Document object is not thread safe and should not be used concurrently.</remarks>
	public partial class Document : JsonFragment, IDocument
	{
		internal const string RevisionPropertyName = "_rev";
		internal const string TypePropertyName = "type";
		internal const string IdPropertyName = "_id";

		/// <summary>Underlying Newtonsoft Json.NET object.</summary>
		internal readonly JObject JsonObject;

		/// <constructor />
		public Document()
		{
			JsonObject = (JObject)JsonToken;
		}

		/// <summary>Parses CouchDB document string.</summary>
		/// <exception cref="ArgumentNullException">Provided string is null or empty.</exception>
		/// <exception cref="ParseException">Provided string contains no or invalid JSON document.</exception>
		public Document(string jsonString): base(jsonString)
		{
			JsonObject = (JObject)JsonToken;
		}

		/// <summary>Loads CouchDB document from provided text reader.</summary>
		/// <param name="textReader"><see cref="TextReader"/> to read JSON from. Should be closed (disposed) by caller.</param>
		/// <remarks>Text reader should be disposed outside of the constructor,</remarks>
		/// <exception cref="ArgumentNullException">Provided text reader is null.</exception>
		/// <exception cref="ParseException">Provided text reader is empty or not JSON.</exception>
		public Document(TextReader textReader): base(textReader)
		{
			JsonObject = (JObject)JsonToken;
		}
		
		/// <constructor />
		internal Document(JObject jsonToken): base(jsonToken)
		{
			if (jsonToken == null) throw new ArgumentNullException("jsonToken");
			JsonObject = (JObject)JsonToken;
		}

		/// <summary>Document identifier or <c>null</c> if no _id property 
		/// found or it's empty.</summary>
		public string Id
		{
			get { return JsonObject.Value<string>(IdPropertyName); }
			set
			{
				if (Id != value)
					SetOrCreateAndInsertSpecialProperty(
						IdPropertyName,
						value,
						JsonObject,
						GetIdProperty,
						insertAfterIndex: -1,
						propertyGettersInOrder: new Func<JProperty>[] { GetIdProperty, GetRevisionProperty, GetTypeProperty });
			}
		}

		/// <summary>Revision of the document or <c>null</c> if no _rev property 
		/// found or it's empty.</summary>
		public string Revision
		{
			get { return JsonObject.Value<string>(RevisionPropertyName); }
			set
			{
				if (Revision != value)
					SetOrCreateAndInsertSpecialProperty(
						RevisionPropertyName,
						value,
						JsonObject,
						GetRevisionProperty,
						insertAfterIndex: 0,
						propertyGettersInOrder: new Func<JProperty>[] {GetIdProperty, GetRevisionProperty, GetTypeProperty});
			}
		}

		/// <summary>Type of the document or <c>null</c> if no type property 
		/// found or it's empty.</summary>
		public string Type
		{
			get { return JsonObject.Value<string>(TypePropertyName); }
			set
			{
				if (Type != value)
					SetOrCreateAndInsertSpecialProperty(
						TypePropertyName,
						value,
						JsonObject,
						GetTypeProperty,
						insertAfterIndex: 1,
						propertyGettersInOrder: new Func<JProperty>[] {GetIdProperty, GetRevisionProperty, GetTypeProperty});
			}
		}

		private IAttachmentBag attachmentBag;

		/// <inheritdoc />
		public IAttachmentBag Attachments
		{
			get { return attachmentBag ?? (attachmentBag = new AttachmentBag(JsonObject)); }
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return Equals(obj as JsonFragment);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		private static void SetOrCreateAndInsertSpecialProperty(
			string name, string stringValue, JObject document, Func<JProperty> getProperty, 
			int insertAfterIndex, Func<JProperty>[] propertyGettersInOrder)
		{
			var property = getProperty();
			var value = JValue.CreateString(stringValue);
			if (property != null)
				property.Value = value;
			else
			{
				var newProperty = new JProperty(name, value);

				if (insertAfterIndex != -1)
				{
					for (var i = insertAfterIndex; i >= 0; i--)
					{
						var propertyToInsertAfter = propertyGettersInOrder[i]();
						if (propertyToInsertAfter != null)
						{
							propertyToInsertAfter.AddAfterSelf(newProperty);
							return;
						}
					}

					for (var i = insertAfterIndex + 1; i < propertyGettersInOrder.Length; i++)
					{
						var propertyToInsertBefore = propertyGettersInOrder[i]();
						if (propertyToInsertBefore != null)
						{
							propertyToInsertBefore.AddBeforeSelf(newProperty);
							return;
						}
					}
				}
				document.AddFirst(newProperty);
			}
		}

		private JProperty GetRevisionProperty()
		{
			return JsonObject.Property(RevisionPropertyName);
		}

		private JProperty GetIdProperty()
		{
			return JsonObject.Property(IdPropertyName);
		}

		private JProperty GetTypeProperty()
		{
			return JsonObject.Property(TypePropertyName);
		}
	}
}