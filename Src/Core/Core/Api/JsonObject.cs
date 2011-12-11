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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Api
{
	/// <summary>Implements <see cref="IJsonObject"/> wrapping <see cref="JToken"/>.</summary>
	public class JsonObject : JObject, IEquatable<JsonObject>, IJsonObject
	{
		/// <constructor />
		public JsonObject() { }

		/// <summary>Parses JSON string.</summary>
		/// <exception cref="ArgumentNullException">Provided string is null or empty.</exception>
		/// <exception cref="ParseException">Provided string contains no or invalid JSON document.</exception>
		public JsonObject(string jsonString): this(ParseJsonString(jsonString))
		{
			if(string.IsNullOrWhiteSpace(jsonString)) throw new ArgumentNullException("jsonString");
		}

		/// <summary>Loads JSON from provided text reader.</summary>
		/// <param name="textReader"><see cref="TextReader"/> to read JSON from. Should be closed (disposed) by caller.</param>
		/// <remarks>Text reader should be disposed outside of the constructor,</remarks>
		/// <exception cref="ArgumentNullException">Provided text reader is null.</exception>
		/// <exception cref="ParseException">Provided text reader is empty or not JSON.</exception>
		public JsonObject(TextReader textReader): this(LoadJsonTextReader(textReader))
		{
			if (textReader == null) throw new ArgumentNullException("textReader");
		}

		/// <constructor />
		internal JsonObject(JObject jsonToken)
		{
			if (jsonToken == null) throw new ArgumentNullException("jsonToken");

			foreach (var property in jsonToken.Properties())
				Add(property.Name, property.Value);
		}

		internal static JObject ParseJsonString(string jsonString)
		{
			if (string.IsNullOrWhiteSpace(jsonString)) throw new ArgumentNullException("jsonString");
			
			JObject jsonToken;
			try
			{
				jsonToken = Parse(jsonString);
			}
			catch (Exception e)
			{
				throw new ParseException(e, e.Message);
			}
			return jsonToken;
		}

		private static JObject LoadJsonTextReader(TextReader textReader)
		{
			if (textReader == null)
				throw new ArgumentNullException("textReader");
			

			JObject jsonToken;
			try
			{
				using (var jsonReader = new JsonTextReader(textReader) { CloseInput = false})
					jsonToken = (JObject)ReadFrom(jsonReader);
			}
			catch (Exception e)
			{
				throw new ParseException(e, "Error reading JSON recived from CouchDB: ", e.Message);
			}

			if (jsonToken == null)
				throw new ParseException("CouchDB was expected to return JSON object.");

			return jsonToken;
		}

		/// <summary>Converts document to JSON string.</summary>
		public override string ToString()
		{
			return NewtonsoftSerializer.ToString(this);
		}

		/// <summary>Produces <see cref="TextReader"/> over content of the JSON fragmet.</summary>
		/// <remarks>Client code is responsible for disposing it.</remarks>
		public TextReader Read()
		{
			// TODO: There should be better way
			return new StringReader(ToString());
		}

		/// <summary>Deserializes current <see cref="JsonObject"/> to object of provided <paramref name="type"/>.</summary>
		public object Deserialize(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			return NewtonsoftSerializer.Deserialize(this, type);
		}

		/// <summary>Deserializes current <see cref="JsonObject"/> to object of provided <paramref name="type"/> returning
		/// <c>null</c> if deserialization was unsuccessful..</summary>
		public object TryDeserialize(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			
			return NewtonsoftSerializer.TryDeserialize(this, type);
		}
		
		/// <summary>Writes JSON string to provided text writer.</summary>
		public void WriteTo(TextWriter writer)
		{
			NewtonsoftSerializer.WriteJsonToTextWriter(this, writer);
		}


		/// <inheritdoc />
		public bool Equals(JsonObject other)
		{
			return DeepEquals(this, other);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return DeepEquals(this, obj as JsonObject);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			// NOTE: It's somewhat unefficient, but it's a simplest way for deep equals to work
			return base.ToString().GetHashCode();
		}
	}
}