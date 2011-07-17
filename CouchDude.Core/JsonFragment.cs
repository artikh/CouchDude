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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using CouchDude.Core.Impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CouchDude.Core
{
	/// <summary>Represents simple JSON fragment. Data could be accessed as </summary>
	public class JsonFragment : IDynamicMetaObjectProvider, IEquatable<JsonFragment>
	{
		/// <summary>Standard set of JSON value convertors.</summary>
		internal static readonly JsonConverter[] Converters =
			new JsonConverter[] { new IsoDateTimeConverter(), new StringEnumConverter() };

		private static readonly JsonSerializer Serializer = JsonSerializer.Create(CreateSerializerSettings());

		/// <summary>Underlying Newtonsoft Json.NET object.</summary>
		internal readonly JObject JsonObject;

		/// <constructor />
		public JsonFragment(): this(new JObject()) { }

		/// <summary>Parses JSON string.</summary>
		/// <exception cref="ArgumentNullException">Provided string is null or empty.</exception>
		/// <exception cref="ParseException">Provided string contains no or invalid JSON document.</exception>
		public JsonFragment(string jsonString): this(Parse(jsonString)) { }

		/// <summary>Loads JSON from provided text reader.</summary>
		/// <param name="textReader"><see cref="TextReader"/> to read JSON from. Should be closed (disposed) by caller.</param>
		/// <remarks>Text reader should be disposed outside of the constructor,</remarks>
		/// <exception cref="ArgumentNullException">Provided text reader is null.</exception>
		/// <exception cref="ParseException">Provided text reader is empty or not JSON.</exception>
		public JsonFragment(TextReader textReader) : this(Load(textReader)) { }

		/// <constructor />
		internal JsonFragment(JObject jsonObject)
		{
			JsonObject = jsonObject;
		}

		private static JObject Parse(string json)
		{
			if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException("json");

			JObject jsonObject;
			try
			{
				jsonObject = JObject.Parse(json);
			}
			catch (Exception e)
			{
				throw new ParseException(e, e.Message);
			}
			return jsonObject;
		}

		private static JObject Load(TextReader textReader)
		{
			if (textReader == null)
				throw new ArgumentNullException("textReader");
			Contract.EndContractBlock();

			JObject jsonDocument;
			try
			{
				using (var jsonReader = new JsonTextReader(textReader) { CloseInput = false})
					jsonDocument = JToken.ReadFrom(jsonReader) as JObject;
			}
			catch (Exception e)
			{
				throw new ParseException(e, "Error reading JSON recived from CouchDB: ", e.Message);
			}

			if (jsonDocument == null)
				throw new ParseException("CouchDB was expected to return JSON object.");

			return jsonDocument;
		}

		/// <summary>Converts document to JSON string.</summary>
		public override string ToString()
		{
			return JsonObject.ToString(Formatting.None, Converters);
		}

		/// <summary>Produces <see cref="TextReader"/> over content of the JSON fragmet.</summary>
		/// <remarks>Client code is responsible for disposing it.</remarks>
		public TextReader Read()
		{
			// TODO: There should be better way
			return new StringReader(ToString());
		}

		/// <summary>Deserializes current <see cref="JsonFragment"/> to object of provided <paramref name="type"/>.</summary>
		public object Deserialize(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			Contract.EndContractBlock();

			using (var jTokenReader = new JTokenReader(JsonObject))
				return Serializer.Deserialize(jTokenReader, type);
		}

		/// <summary>Serializes provided object to <see cref="JsonFragment"/>.</summary>
		public static JsonFragment Serialize(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");
			Contract.EndContractBlock();

			JObject jsonObject;
			using (var jTokenWriter = new JTokenWriter())
			{
				Serializer.Serialize(jTokenWriter, obj);
				jsonObject = (JObject)jTokenWriter.Token;
			}
			return new JsonFragment(jsonObject);
		}

		/// <summary>Creates standard serializen properties.</summary>
		protected static JsonSerializerSettings CreateSerializerSettings()
		{
			return new JsonSerializerSettings {
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new CamelCasePrivateSetterPropertyContractResolver(),
				Converters = Converters
			};
		}

		/// <summary>Resolves private setter properties writable.</summary>
		public class CamelCasePrivateSetterPropertyContractResolver : CamelCasePropertyNamesContractResolver
		{
			/// <inheritdoc />
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var jsonProperty = base.CreateProperty(member, memberSerialization);
				
				if (!jsonProperty.Writable)
				{
					var propertyInfo = member as PropertyInfo;
					if (propertyInfo != null)
					{
						var hasPrivateSetter = propertyInfo.GetSetMethod(true) != null;
						jsonProperty.Writable = hasPrivateSetter;
					}
				}

				return jsonProperty;
			}
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new ForwardingMetaObject(
				parameter, 
				BindingRestrictions.Empty, 
				this, 
				JsonObject,
			  // JObject's meta-object needs to know where to find the instance of JObject it is operating on.
			  // Assuming that an instance of Document is passed to the 'parameter' expression
			  // we get the corresponding instance of JObject by reading the "jsonObject" field.
			  document => Expression.Field(document, "jsonObject")
			);
		}

		/// <inheritdoc />
		public bool Equals(JsonFragment other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return JToken.DeepEquals(other.JsonObject, JsonObject);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (JsonFragment)) return false;
			return Equals((JsonFragment) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return JsonObject.GetHashCode();
		}

		/// <summary>Compares JSON fragments for equality.</summary>
		public static bool operator ==(JsonFragment left, JsonFragment right)
		{
			return Equals(left, right);
		}

		/// <summary>Compares JSON fragments for inequality.</summary>
		public static bool operator !=(JsonFragment left, JsonFragment right)
		{
			return !Equals(left, right);
		}

		/// <summary>Forward dynamic behaviour to another object.</summary>
		/// <remarks>See. http://matousek.wordpress.com/2009/11/07/forwarding-meta-object </remarks>
		private class ForwardingMetaObject : DynamicMetaObject
		{
			private readonly DynamicMetaObject metaForwardee;

			/// <constructor />
			public ForwardingMetaObject(
				Expression expression,
				BindingRestrictions restrictions,
				object forwarder,
				IDynamicMetaObjectProvider forwardee,
				Func<Expression, Expression> forwardeeGetter)
				: base(expression, restrictions, forwarder)
			{
				// We'll use forwardee's meta-object to bind dynamic operations.
				metaForwardee = forwardee.GetMetaObject(
					forwardeeGetter(
						Expression.Convert(expression, forwarder.GetType())   // [1]
						)
					);
			}

			// Restricts the target object's type to TForwarder. 
			// The meta-object we are forwarding to assumes that it gets an instance of TForwarder (see [1]).
			// We need to ensure that the assumption holds.
			private DynamicMetaObject AddRestrictions(DynamicMetaObject result)
			{
				return new DynamicMetaObject(
					result.Expression,
					BindingRestrictions.GetTypeRestriction(Expression, Value.GetType()).Merge(result.Restrictions),
					metaForwardee.Value
					);
			}

			// Forward all dynamic operations or some of them as needed //

			public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
			{
				return AddRestrictions(metaForwardee.BindGetMember(binder));
			}

			public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
			{
				return AddRestrictions(metaForwardee.BindInvokeMember(binder, args));
			}

			public override DynamicMetaObject BindConvert(ConvertBinder binder)
			{
				return AddRestrictions(metaForwardee.BindConvert(binder));
			}
		}
	}
}