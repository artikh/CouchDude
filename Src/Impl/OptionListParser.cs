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
using System.Globalization;
using System.Json;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CouchDude.Utils;

namespace CouchDude.Impl
{
	interface IOption<in T>
	{
		string Name { get; }
		string DefaultValue { get; }
		string Get(T sourceObject, ISerializer serializer);
		void Set(T sourceObject, string stringValue);
	}

	internal abstract class OptionBase<T> : IOption<T>
	{
		private readonly string name;
		private readonly string defaultValue;
		public string Name { get { return name; } }
		public string DefaultValue { get { return defaultValue; } }
		public abstract string Get(T sourceObject, ISerializer serializer);
		public abstract void Set(T sourceObject, string stringValue);

		protected OptionBase(string name, string defaultValue)
		{
			this.name = name;
			this.defaultValue = defaultValue;
		}
	}

	abstract class Option<T, TValue>: OptionBase<T>
	{
		protected readonly Action<T, TValue> SetValue;
		protected readonly Func<T, TValue> GetValue;

		protected Option(Expression<Func<T, TValue>> getPropertyExpression, string name, string defaultValue)
			:base(name, defaultValue)
		{
			var member = (MemberExpression)getPropertyExpression.Body;
			var param = Expression.Parameter(typeof(TValue), "value");
			var setPropertyExpression = Expression.Lambda<Action<T, TValue>>(
					Expression.Assign(member, param), getPropertyExpression.Parameters[0], param);
			SetValue = setPropertyExpression.Compile();

			GetValue = getPropertyExpression.Compile();
		}

		protected abstract bool TryDeserialize(string stringValue, out TValue value);
		protected abstract string Serialize(TValue value, ISerializer serializer);

		public override string Get(T sourceObject, ISerializer serializer)
		{
			var stringValue = Serialize(GetValue(sourceObject), serializer);
			return stringValue != DefaultValue ? stringValue : null;
		}

		public override void Set(T sourceObject, string stringValue)
		{
			TValue value;
			if(TryDeserialize(stringValue, out value))
				SetValue(sourceObject, value);
		}
	}

	class BooleanOption<T> : Option<T, bool>
	{
		private readonly string trueValue;
		private readonly string falseValue;

		public BooleanOption(
			Expression<Func<T, bool>> getPropertyExpression, string name, string trueValue, string falseValue, string defaultValue)
			: base(getPropertyExpression, name, defaultValue)
		{
			this.trueValue = trueValue;
			this.falseValue = falseValue;
		}

		protected override bool TryDeserialize(string stringValue, out bool value)
		{
			if (stringValue == trueValue)
			{
				value = true;
				return true;
			}
			if (stringValue == falseValue)
			{
				value = false;
				return true;
			}

			value = false;
			return false;
		}

		protected override string Serialize(bool value, ISerializer serializer) { return value ? trueValue : falseValue; }
	}

	class CustomValueOption<T, TValue> : Option<T, TValue>
	{
		private readonly Func<string, TValue> deserialize;
		private readonly Func<TValue, ISerializer, string> serialize;

		public CustomValueOption(
			Expression<Func<T, TValue>> getPropertyExpression, string name, string defaultValue, 
			Func<string, TValue> deserialize, Func<TValue, ISerializer, string> serialize)
			: base(getPropertyExpression, name, defaultValue)
		{
			this.deserialize = deserialize;
			this.serialize = serialize;
		}

		protected override bool TryDeserialize(string stringValue, out TValue value)
		{
			value = deserialize(stringValue);
			return !Equals(value, default(TValue));
		}

		protected override string Serialize(TValue value, ISerializer serializer) { return serialize(value, serializer); }
	}

	class CustomOption<T, TValue>: Option<T, TValue>
	{
		private readonly Func<string, bool> isValid;
		private readonly Func<TValue, ISerializer, string> serialize;
		private readonly Func<string, TValue> deserialize; 

		public CustomOption(
			Expression<Func<T, TValue>> getPropertyExpression, string name, string defaultValue, 
			Func<string, bool> isValid, Func<TValue, ISerializer, string> serialize, Func<string, TValue> deserialize)
			: base(getPropertyExpression, name, defaultValue)
		{
			this.isValid = isValid;
			this.serialize = serialize;
			this.deserialize = deserialize;
		}

		protected override bool TryDeserialize(string stringValue, out TValue value)
		{
			if(isValid(stringValue))
			{
				value = deserialize(stringValue);
				return true;
			}
			value = default(TValue);
			return false;
		}

		protected override string Serialize(TValue value, ISerializer serializer) { return serialize(value, serializer); }
	}
	
	class PositiveIntegerOption<T> : Option<T, int?>
	{
		public PositiveIntegerOption(
			Expression<Func<T, int?>> getPropertyExpression, string name)
			: base(getPropertyExpression, name, null) { }

		protected override bool TryDeserialize(string stringValue, out int? value)
		{
			int val;
			if(Int32.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out val))
			{
				value = val;
				return true;
			}
			value = null;
			return false;
		}

		protected override string Serialize(int? value, ISerializer serializer)
		{
			return value.HasValue && value > 0
				? value.Value.ToString(CultureInfo.InvariantCulture): DefaultValue;
		}
	}

	class StringOption<T> : Option<T, string>
	{
		public StringOption(
			Expression<Func<T, string>> getPropertyExpression, string name) 
			: base(getPropertyExpression, name, null) {}

		protected override bool TryDeserialize(string stringValue, out string value)
		{
			value = stringValue;
			return true;
		}

		protected override string Serialize(string value, ISerializer serializer) { return value; }
	}

	class JsonOption<T> : Option<T, object>
	{
		public JsonOption(
			Expression<Func<T, object>> getPropertyExpression, string name) 
			: base(getPropertyExpression, name, null) {}

		protected override bool TryDeserialize(string stringValue, out object value)
		{
			try
			{
				value = JsonValue.Parse(stringValue);
				return true;
			}
			catch (Exception)
			{
				value = null;
				return false;
			}
		}

		protected override string Serialize(object value, ISerializer serializer)
		{
			if (value != null)
			{
				var jsonFragment = value as JsonValue ??  serializer.ConvertToJson(value, throwOnError: false);
				return jsonFragment.ToString();
			}
			return null;
		}
	}

	class CustomOption<T> : OptionBase<T>
	{
		private readonly Func<T, ISerializer, string> getStringValue; 
		private readonly Action<T, string> setStringValue;

		public CustomOption(string name, string defaultValue, Func<T, ISerializer, string> getStringValue, Action<T, string> setStringValue) 
			: base(name, defaultValue)
		{
			this.getStringValue = getStringValue;
			this.setStringValue = setStringValue;
		}

		public override string Get(T sourceObject, ISerializer serializer) { return getStringValue(sourceObject, serializer); }
		public override void Set(T sourceObject, string stringValue) { setStringValue(sourceObject, stringValue); }
	}
	

	/// <summary>Query-string serializable object (de)serializer.</summary>
	sealed class OptionListSerializer<T>
	{
		private readonly IOption<T>[] options;
		private readonly IDictionary<string, IOption<T>> optionsMap;

		public OptionListSerializer(params IOption<T>[] options)
		{
			this.options = options;
			optionsMap =
				new System.Collections.Concurrent.ConcurrentDictionary<string, IOption<T>>(
					options.ToDictionary(o => o.Name, o => o)
				);
		}

		public string ToQueryString(T optionListObject, ISerializer serializer)
		{
			var result = new StringBuilder();
			foreach (var option in options)
			{
				var stringValue = option.Get(optionListObject, serializer);
				if (stringValue != null)
				{
					var encodedValue = Uri.EscapeUriString(stringValue);
					result.Append(option.Name).Append("=").Append(encodedValue).Append("&");
				}
			}
			if (result.Length > 0)
				result.Remove(result.Length - 1, 1); // removing trailing '&'

			return result.ToString();
		}

		public void Parse(string queryString, ref T optionListObject)
		{
			var values = UriUtils.ParseQueryString(queryString);
			foreach (var kvp in values)
			{
				IOption<T> option;
				if (optionsMap.TryGetValue(kvp.Key, out option))
				{
					var stringValue = kvp.Value;
					if (stringValue != option.DefaultValue)
						option.Set(optionListObject, stringValue);
				}
			}
		}
	}
}
