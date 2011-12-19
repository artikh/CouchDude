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
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using CouchDude.Api;
using CouchDude.Utils;

namespace CouchDude
{
	/// <summary>CouchDB document implemented wrapping <see cref="JsonObject"/>.</summary>
	/// <remarks>Document object is not thread safe and should not be used concurrently.</remarks>
	public class Document
	{
		internal const string RevisionPropertyName = "_rev";
		internal const string TypePropertyName = "type";
		internal const string IdPropertyName = "_id";

		private DocumentAttachmentBag documentAttachmentBag;

		/// <summary>Underlying <see cref="JsonObject"/> instance.</summary>
		public JsonObject RawJsonObject { get; private set; }

		/// <summary>Returs underlying JSON data object as dynamic.</summary>
		public dynamic AsDynamic() { return RawJsonObject.AsDynamic(); }

		/// <constructor />
		public Document() { }

		/// <summary>Parses CouchDB document string.</summary>
		/// <exception cref="ArgumentNullException">Provided string is null or empty.</exception>
		/// <exception cref="ParseException">Provided string contains no or invalid JSON document.</exception>
		public Document(string jsonString) : this(ParseString(jsonString)) { }

		/// <summary>Loads CouchDB document from provided text reader.</summary>
		/// <param name="textReader"><see cref="TextReader"/> to read JSON from. Should be closed (disposed) by caller.</param>
		/// <remarks>Text reader should be disposed outside of the constructor,</remarks>
		/// <exception cref="ArgumentNullException">Provided text reader is null.</exception>
		/// <exception cref="ParseException">Provided text reader is empty or not JSON.</exception>
		public Document(TextReader textReader) : this(LoadTextReader(textReader)) { }

		private static JsonObject ParseString(string jsonString)
		{
			try
			{
				return (JsonObject)JsonValue.Parse(jsonString);
			}
			catch (Exception e)
			{
				throw new ParseException(e, "Error parsing document JSON: {0}", jsonString);
			}
		}

		private static JsonObject LoadTextReader(TextReader textReader)
		{
			try
			{
				return (JsonObject)JsonValue.Load(textReader);
			}
			catch (Exception e)
			{
				throw new ParseException(e, "Error parsing documnet JSON text reader");
			}
		}

		/// <constructor />
		public Document(JsonObject jsonObject)
		{
			if (jsonObject == null) throw new ArgumentNullException("jsonObject");
			RawJsonObject = jsonObject;
		}

		/// <summary>Document identifier or <c>null</c> if no _id property 
		/// found or it's empty.</summary>
		public string Id
		{
			get { return RawJsonObject.GetPrimitiveProperty<string>(IdPropertyName); }
			set { SetValue(value, IdPropertyName); }
		}

		/// <summary>Revision of the document or <c>null</c> if no _rev property 
		/// found or it's empty.</summary>
		public string Revision
		{
			get { return RawJsonObject.GetPrimitiveProperty<string>(RevisionPropertyName); }
			set { SetValue(value, RevisionPropertyName); }
		}

		/// <summary>Type of the document or <c>null</c> if no type property 
		/// found or it's empty.</summary>
		public string Type
		{
			get { return RawJsonObject.GetPrimitiveProperty<string>(TypePropertyName); } 
			set { SetValue(value, TypePropertyName); }
		}
		
		/// <summary>Attachment collection.</summary>
		public DocumentAttachmentBag DocumentAttachments
		{
			get { return documentAttachmentBag ?? (documentAttachmentBag = new DocumentAttachmentBag(this)); }
		}

		/// <summary>References (weakly) parent database API instance.</summary>
		public DatabaseApiReference DatabaseApiReference = DatabaseApiReference.Empty;
		
		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			var otherDocument = obj as Document;
			if (ReferenceEquals(null, otherDocument)) return false;
			if (ReferenceEquals(this, otherDocument)) return true;

			//HACK: Implement normal visiter comparition
			return otherDocument.RawJsonObject.ToString().Equals(RawJsonObject.ToString());
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			//HACK: Implement normal visiter 
			return RawJsonObject.ToString().GetHashCode();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return RawJsonObject.ToString();
		}

		private void SetValue(string value, string propertyName)
		{
			if (value != Id)
				if (value == null)
					RawJsonObject.Remove(propertyName);
				else
				{
					RawJsonObject[propertyName] = value;
					Reorder();
				}
		}

		private void Reorder()
		{
			var values = RawJsonObject.OfType<KeyValuePair<string, JsonValue>>().OrderBy(
				kvp => {
					switch (kvp.Key)
					{
						case IdPropertyName: return 0;
						case RevisionPropertyName: return 1;
						case TypePropertyName: return 2;
						default: return 3;
					}
				}).ToArray();
			RawJsonObject.Clear();
			foreach (var value in values)
				RawJsonObject[value.Key] = value.Value;
		}
	}

}