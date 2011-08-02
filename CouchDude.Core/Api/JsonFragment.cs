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
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CouchDude.Core.Api
{
	/// <summary>Implements <see cref="IJsonFragment"/> wrapping <see cref="JToken"/>.</summary>
	public class JsonFragment : IEquatable<JsonFragment>, IJsonFragment
	{
		/// <summary>Standard set of JSON value convertors.</summary>
		internal static readonly JsonConverter[] Converters =
			new JsonConverter[] { new IsoDateTimeConverter(), new StringEnumConverter() };

		private static readonly JsonSerializer Serializer = JsonSerializer.Create(CreateSerializerSettings());

		/// <summary>Underlying Newtonsoft Json.NET token.</summary>
		protected readonly JToken JsonToken;

		/// <constructor />
		public JsonFragment(): this(new JObject()) { }

		/// <summary>Parses JSON string.</summary>
		/// <exception cref="ArgumentNullException">Provided string is null or empty.</exception>
		/// <exception cref="ParseException">Provided string contains no or invalid JSON document.</exception>
		public JsonFragment(string jsonString): this(Parse(jsonString))
		{
			Contract.Requires(!string.IsNullOrWhiteSpace(jsonString));
		}

		/// <summary>Loads JSON from provided text reader.</summary>
		/// <param name="textReader"><see cref="TextReader"/> to read JSON from. Should be closed (disposed) by caller.</param>
		/// <remarks>Text reader should be disposed outside of the constructor,</remarks>
		/// <exception cref="ArgumentNullException">Provided text reader is null.</exception>
		/// <exception cref="ParseException">Provided text reader is empty or not JSON.</exception>
		public JsonFragment(TextReader textReader): this(Load(textReader))
		{
			Contract.Requires(textReader != null);
		}

		/// <constructor />
		internal JsonFragment(JToken jsonToken)
		{
			if (jsonToken == null) throw new ArgumentNullException("jsonToken");
			Contract.EndContractBlock();

			JsonToken = jsonToken;
		}

		private static JToken Parse(string jsonString)
		{
			if (string.IsNullOrWhiteSpace(jsonString)) throw new ArgumentNullException("jsonString");
			Contract.EndContractBlock();

			JToken jsonToken;
			try
			{
				jsonToken = JToken.Parse(jsonString);
			}
			catch (Exception e)
			{
				throw new ParseException(e, e.Message);
			}
			return jsonToken;
		}

		private static JToken Load(TextReader textReader)
		{
			if (textReader == null)
				throw new ArgumentNullException("textReader");
			Contract.EndContractBlock();

			JToken jsonToken;
			try
			{
				using (var jsonReader = new JsonTextReader(textReader) { CloseInput = false})
					jsonToken = JToken.ReadFrom(jsonReader);
			}
			catch (Exception e)
			{
				throw new ParseException(e, "Error reading JSON recived from CouchDB: ", e.Message);
			}

			if (jsonToken == null)
				throw new ParseException("CouchDB was expected to return JSON object.");

			return jsonToken;
		}

		/// <summary>Grabs required property value throwing
		/// <see cref="ParseException"/> if not found or empty.</summary>
		public string GetRequiredProperty(string name, string additionalMessage = null)
		{
			var propertyValue = JsonToken[name] as JValue;
			if (propertyValue == null)
				throw new ParseException(
					"Required field '{0}' have not found on document{1}:\n {2}",
					name,
					additionalMessage == null ? string.Empty : ". " + additionalMessage,
					ToString()

				);
			var value = propertyValue.Value<string>();
			if (string.IsNullOrWhiteSpace(value))
				throw new ParseException("Required field '{0}' is empty", name);

			return value;
		}

		/// <summary>Converts document to JSON string.</summary>
		public override string ToString()
		{
			return JsonToken.ToString(Formatting.None, Converters);
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

			using (var jTokenReader = new JTokenReader(JsonToken))
				try
				{
					return Serializer.Deserialize(jTokenReader, type);
				}
				catch (JsonSerializationException e)
				{
					throw new ParseException(e, "Error deserialising JSON fragment");
				}
		}

		/// <summary>Deserializes current <see cref="JsonFragment"/> to object of provided <paramref name="type"/> returning
		/// <c>null</c> if deserialization was unsuccessful..</summary>
		public object TryDeserialize(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			Contract.EndContractBlock();

			using (var jTokenReader = new JTokenReader(JsonToken))
				try
				{
					return Serializer.Deserialize(jTokenReader, type);
				}
				catch (JsonSerializationException)
				{
					return null;
				}
		}

		/// <summary>Serializes provided object to <see cref="JsonFragment"/>.</summary>
		public static IJsonFragment Serialize(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");
			Contract.EndContractBlock();

			JToken jsonToken;
			using (var jTokenWriter = new JTokenWriter())
			{
				Serializer.Serialize(jTokenWriter, obj);
				jsonToken = jTokenWriter.Token;
			}
			return new JsonFragment(jsonToken);
		}

		/// <summary>Creates standard serializen properties.</summary>
		protected internal static JsonSerializerSettings CreateSerializerSettings()
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

		/// <summary>Writes JSON string to provided text writer.</summary>
		public void WriteTo(TextWriter writer)
		{
			using(var jTokenWriter = new JsonTextWriter(writer) { CloseOutput = false})
				JsonToken.WriteTo(jTokenWriter, Converters);
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new ForwardingMetaObject(
				parameter, 
				BindingRestrictions.Empty, 
				this,
				JsonToken,
			  // JObject's meta-object needs to know where to find the instance of JObject it is operating on.
			  // Assuming that an instance of Document is passed to the 'parameter' expression
			  // we get the corresponding instance of JObject by reading the "jsonObject" field.
				document => Expression.Field(document, "JsonToken")
			);
		}

		/// <inheritdoc />
		public bool Equals(JsonFragment other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return JToken.DeepEquals(other.JsonToken, JsonToken);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (!(obj is JsonFragment)) return false;
			return Equals((JsonFragment) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return JsonToken.GetHashCode();
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