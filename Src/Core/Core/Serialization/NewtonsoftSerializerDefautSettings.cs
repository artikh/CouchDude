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

using CouchDude.Api;
using CouchDude.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CouchDude.Serialization
{
	/// <summary>Encapsulates default Newtonsoft Json.NET serializer settings for CouchDude.</summary>
	public static class NewtonsoftSerializerDefautSettings 
	{
		/// <summary>Standard set of JSON value convertors.</summary>
		private static readonly JsonConverter[] Converters =
			new JsonConverter[] {
				new IsoUtcDateTimeConverter(), new StringEnumConverter(), new StringEnumConverter(), new UriConverter()
			};

		/// <summary>Creates default serializer settings.</summary>
		public static JsonSerializerSettings CreateDefaultSerializerSettingsDefault()
		{
			return new JsonSerializerSettings {
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new JsonFragment.CamelCasePrivateSetterPropertyContractResolver(),
				Converters = Converters
			};
		}

		/// <summary>Creates specific entity type serializer settings.</summary>
		public static JsonSerializerSettings CreateEntitySerializerSettingsDefault(IEntityConfig entityConfig)
		{
			var settings = CreateDefaultSerializerSettingsDefault();
			settings.ContractResolver = new EntityContractResolver(entityConfig.EntityType, entityConfig.IgnoredMembers);
			return settings;
		}
	}
}