using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CouchDude.Core.Utils
{
	internal static class JsonSerializer
	{
		private static readonly ThreadLocal<Newtonsoft.Json.JsonSerializer> Serializer = new ThreadLocal<Newtonsoft.Json.JsonSerializer>(CreateSerializer);

		public static Newtonsoft.Json.JsonSerializer Instance
		{
			get { return Serializer.Value; }
		}

		internal static Newtonsoft.Json.JsonSerializer CreateSerializer()
		{
			var contractResolver = new ContractResolver();
			var settings = new JsonSerializerSettings
			{
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = contractResolver,
				Converters = { new IsoDateTimeConverter(), new StringEnumConverter() }
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