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
			var contractResolver = new CamelCasePropertyNamesContractResolver();
			contractResolver.DefaultMembersSearchFlags |= BindingFlags.NonPublic;

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
}