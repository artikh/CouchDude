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
using System.Json;
using System.Text;
using Newtonsoft.Json;

namespace CouchDude.Utils
{
	/// <summary><see cref="JsonWriter"/> implementation writing to <see cref="JsonValue"/> object.</summary>
	public class SystemJsonValueWriter: JsonWriter
	{
		class JsonContainer
		{
			readonly JsonObject jsonObject;
			readonly JsonArray jsonArray;

			public JsonValue JsonValue { get { return jsonObject ?? jsonArray as JsonValue; } }

			public string CurrentPropertyName { get; set; }

			public JsonContainer(JsonArray jsonArray) 
			{
				this.jsonArray = jsonArray;
				jsonObject = null;
			}

			public JsonContainer(JsonObject jsonObject) 
			{
				jsonArray = null;
				this.jsonObject = jsonObject;
			}

			public void AddChild(JsonValue jsonValue)
			{
				if (jsonArray != null)
					jsonArray.Add(jsonValue);
				else
				{
					jsonObject.Add(CurrentPropertyName, jsonValue);
					CurrentPropertyName = null;
				}
			}
		}

		readonly Stack<JsonContainer> stack = new Stack<JsonContainer>();
		JsonValue root;
		
		void AddPrimitive(JsonPrimitive jsonPrimitive)
		{
			if (stack.Count == 0)
				root = jsonPrimitive;
			else
			{
				var parentContainer = stack.Peek();
				parentContainer.AddChild(jsonPrimitive);
			}
		}

		void AddContainer(JsonContainer container)
		{
			if (stack.Count != 0)
			{
				var parentContainer = stack.Peek();
				parentContainer.AddChild(container.JsonValue);
			}
			stack.Push(container);

			if (stack.Count == 1)
				root = stack.Peek().JsonValue;
		}

		void RemoveContainer()
		{
			if (stack.Count != 0)
				stack.Pop();
		}
		
		/// <inheritdoc />
		public override void WriteStartObject()
		{
			base.WriteStartObject();
			AddContainer(new JsonContainer(new JsonObject()));
		}

		/// <inheritdoc />
		public override void WriteEndObject()
		{
			base.WriteEndObject();
			RemoveContainer();
		}
		/// <inheritdoc />
		public override void WriteStartArray()
		{
			base.WriteStartArray();
			AddContainer(new JsonContainer(new JsonArray()));
		}
		/// <inheritdoc />
		public override void WriteEndArray()
		{
			base.WriteEndArray();
			RemoveContainer();
		}

		/// <inheritdoc />
		public override void WritePropertyName(string name)
		{
			base.WritePropertyName(name);
			if (stack.Count != 0)
			{
				var parentContainer = stack.Peek();
				parentContainer.CurrentPropertyName = name;
			}
		}

		/// <inheritdoc />
		public override void WriteNull()
		{
			base.WriteNull();
			AddPrimitive(null);
		}

		/// <inheritdoc />
		public override void WriteUndefined()
		{
			base.WriteUndefined();
			AddPrimitive(null);
		}

		/// <inheritdoc />
		public override void WriteValue(string value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(int value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(uint value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(long value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(ulong value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(float value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(double value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(bool value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(short value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(ushort value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(char value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(byte value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(sbyte value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(decimal value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(DateTime value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(DateTimeOffset value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}
		/// <inheritdoc />
		public override void WriteValue(Guid value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void WriteValue(TimeSpan value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value.TotalMilliseconds));
		}

		/// <inheritdoc />
		public override void WriteValue(byte[] value)
		{
			base.WriteValue(value);

			var hexString = new StringBuilder(value.Length*2);
			foreach (var b in value)
			{
				hexString.AppendFormat("{0:x}", b);
			}

			AddPrimitive(new JsonPrimitive(hexString.ToString()));
		}

		/// <inheritdoc />
		public override void WriteValue(Uri value)
		{
			base.WriteValue(value);
			AddPrimitive(new JsonPrimitive(value));
		}

		/// <inheritdoc />
		public override void Flush() {  }

		/// <summary>Returs <see cref="JsonValue"/> object representing JSON written to writer so far.</summary>
		public JsonValue JsonValue { get { return root; } }
	}
}