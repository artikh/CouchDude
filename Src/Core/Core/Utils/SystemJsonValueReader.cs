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
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace CouchDude.Utils
{
	/// <summary><see cref="JsonReader"/> implementation reading <see cref="JsonValue"/> object.</summary>
	public class SystemJsonValueReader : JsonReader 
	{
		class ArrayFrame: Frame
		{
			enum State
			{
				Begin,
				ReadingProperties,
				End
			}

			State state = State.Begin;
			readonly JsonReader parent;
			readonly JsonArray jsonValue;

			public ArrayFrame(JsonReader parent, JsonArray jsonValue)
			{
				this.parent = parent;
				this.jsonValue = jsonValue;
			}

			public override bool Read()
			{
				if(state == State.Begin)
				{
					
				}
			}
		}

		class PrimitiveFrame: Frame
		{
			private readonly SystemJsonValueReader parent;
			private JsonPrimitive jsonPrimitive;
			public PrimitiveFrame(SystemJsonValueReader parent, JsonPrimitive jsonPrimitive)
			{
				this.parent = parent;
				this.jsonPrimitive = jsonPrimitive;
			}

			public override bool Read()
			{
				if (jsonPrimitive == null)
					return false;

				switch (jsonPrimitive.JsonType)
				{
					case JsonType.String:
						parent.SetToken(JsonToken.String, jsonPrimitive.Value);
						break;
					case JsonType.Number:
						parent.SetToken(jsonPrimitive.Value is int? JsonToken.Integer: JsonToken.Float, jsonPrimitive.Value);
						break;
					case JsonType.Boolean:
						parent.SetToken(JsonToken.Boolean, jsonPrimitive.Value);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				jsonPrimitive = null;
			}
		}

		class ObjectFrame: Frame
		{
			enum State
			{
				Begin,
				ReadingProperties,
				End
			}

			State state = State.Begin;
			KeyValuePair<string, JsonValue> currentProperty;
			readonly SystemJsonValueReader parent;
			private readonly JsonObject jsonObject;

			public ObjectFrame(SystemJsonValueReader parent, JsonObject jsonObject)
			{
				this.parent = parent;
				this.jsonObject = jsonObject;
			}

			public override bool Read()
			{
				switch (state)
				{
					case State.Begin:
						parent.SetToken(JsonToken.StartObject);
						state = jsonObject.Count == 0 ? State.End : State.ReadingProperties;
						currentProperty = jsonObject.FirstOrDefault()
						return true;
					case State.ReadingProperties:

						break;
					case State.End:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		abstract class Frame
		{
			public abstract bool Read();
		}

		

		readonly JsonValue rootValue;
		readonly Stack<Frame> frames = new Stack<Frame>();  

		/// <constructor />
		public SystemJsonValueReader(JsonValue rootValue)
		{
			if(rootValue == null) throw new ArgumentNullException("rootValue");
			if (rootValue.JsonType == JsonType.Default) throw new ArgumentNullException("'Default' JSON values could not be read.", "rootValue");

			this.rootValue = rootValue;
		}

		public override bool Read()
		{
			if (frames.Count == 0)
			{
				switch (rootValue)
				{
						
				}
				return true;
			}
		}

		private void CreateNewFrame(JsonValue jsonValue)
		{
			switch (jsonValue.JsonType)
			{
				case JsonType.String:
				case JsonType.Number:
				case JsonType.Boolean:
					frames.Push(new PrimitiveFrame(this, (JsonPrimitive)jsonValue));
					break;
				case JsonType.Object:
					frames.Push(new ObjectFrame(this, (JsonObject)jsonValue));
					break;
				case JsonType.Array:
					frames.Push(new ArrayFrame(this, (JsonArray)jsonValue));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override byte[] ReadAsBytes() { throw new NotImplementedException(); }
		public override decimal? ReadAsDecimal() { throw new NotImplementedException(); }
		public override DateTimeOffset? ReadAsDateTimeOffset() { throw new NotImplementedException(); }
	}
}
