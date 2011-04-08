using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CouchDude.Core.Implementation
{
	internal static class JsonSerializer
	{
		[ThreadStatic]
		private static Newtonsoft.Json.JsonSerializer serializer;

		public static Newtonsoft.Json.JsonSerializer Instance
		{
			get { return serializer ?? (serializer = CreateSerializer()); }
		}

		private static Newtonsoft.Json.JsonSerializer CreateSerializer()
		{
			var contractResolver = new ContractResolver();
			var settings = new JsonSerializerSettings
			{
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = contractResolver,
				Converters = { new IsoDateTimeConverter() }
			};
			return Newtonsoft.Json.JsonSerializer.Create(settings);
		}
	}

	internal class ContractResolver : CamelCasePropertyNamesContractResolver
	{
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

}