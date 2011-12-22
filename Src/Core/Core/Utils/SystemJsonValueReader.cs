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
using Newtonsoft.Json;

namespace CouchDude.Utils
{
	/// <summary><see cref="JsonReader"/> implementation reading <see cref="JsonValue"/> object.</summary>
	public class SystemJsonValueReader : JsonReader 
	{
		class ArrayFrame: Frame
		{
			readonly SystemJsonValueReader parent;
			readonly JsonArray jsonArray;

			IEnumerator<JsonValue> enumerator;
			bool haveMovedOk;

			public ArrayFrame(SystemJsonValueReader parent, JsonArray jsonArray)
			{
				this.parent = parent;
				this.jsonArray = jsonArray;
			}

			public override bool Read()
			{
				if(enumerator == null)
				{
					enumerator = jsonArray.GetEnumerator();
					return (haveMovedOk = enumerator.MoveNext());
				}

				if (haveMovedOk)
				{
					haveMovedOk = enumerator.MoveNext();
					parent.CreateNewFrame(enumerator.Current);
					return haveMovedOk;
				}

				return false;
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
				return true;
			}
		}

		class ObjectFrame: Frame
		{
			readonly SystemJsonValueReader parent;
			readonly JsonObject jsonObject;

			IEnumerator<KeyValuePair<string, JsonValue>> enumerator;
			bool emittedPropertyName;
			bool haveMovedOk;

			public ObjectFrame(SystemJsonValueReader parent, JsonObject jsonObject)
			{
				this.parent = parent;
				this.jsonObject = jsonObject;
			}

			public override bool Read()
			{
				if (enumerator == null)
				{
					enumerator = jsonObject.GetEnumerator();
					return (haveMovedOk = enumerator.MoveNext());
				}

				if (haveMovedOk)
				{
					haveMovedOk = enumerator.MoveNext();

					if (haveMovedOk)
					{
						if (emittedPropertyName)
						{
							parent.CreateNewFrame(enumerator.Current.Value);
							emittedPropertyName = false;
						}
						else
						{
							parent.SetToken(JsonToken.PropertyName, enumerator.Current.Key);
							emittedPropertyName = true;
							return true;
						}
					}
					return haveMovedOk;
				}

				return false;
			}
		}

		abstract class Frame
		{
			public abstract bool Read();
		}
		
		readonly JsonValue rootValue;
		private Stack<Frame> frames;

		/// <constructor />
		public SystemJsonValueReader(JsonValue rootValue)
		{
			if(rootValue == null) throw new ArgumentNullException("rootValue");
			if (rootValue.JsonType == JsonType.Default) throw new ArgumentNullException("'Default' JSON values could not be read.", "rootValue");

			this.rootValue = rootValue;
		}

		/// <inheritdoc />
		public override bool Read()
		{
			if(frames == null)
			{
				frames = new Stack<Frame>();
				CreateNewFrame(rootValue);
			}

			while (frames.Count > 0)
			{
				var currentFrame = frames.Peek();
				var currentFrameHaveMoved = currentFrame.Read();
				if (currentFrameHaveMoved) 
					return true;
				
				frames.Pop();
			}
			return false;
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

		/// <inheritdoc />
		public override byte[] ReadAsBytes() { throw new NotImplementedException(); }

		/// <inheritdoc />
		public override decimal? ReadAsDecimal() { throw new NotImplementedException(); }

		/// <inheritdoc />
		public override DateTimeOffset? ReadAsDateTimeOffset() { throw new NotImplementedException(); }
	}
}
