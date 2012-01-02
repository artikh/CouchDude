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
using System.IO;
using System.Json;
using CouchDude.Configuration;
using CouchDude.Serialization;
using JetBrains.Annotations;

namespace CouchDude
{
	/// <summary>CouchDude main serializer interface. 
	/// Default (and only included) implementation is <see cref="NewtonsoftSerializer"/>.</summary>
	public interface ISerializer
	{
		/// <summary>Serializes provided object of provided type to provided to instance of <see cref="TextWriter"/>.</summary>
		void Serialize([NotNull]TextWriter target, [NotNull]object source, bool throwOnError);

		/// <summary>Deserializes object of provided type from provided instance of <see cref="TextReader"/>.</summary>
		object Deserialize([NotNull]Type targetType, [NotNull]TextReader source, bool throwOnError);

		/// <summary>Converts provided object of provided type to JSON.</summary>
		JsonValue ConvertToJson([NotNull]object source, bool throwOnError = false);

		/// <summary>Converts provided JSON to object of provided type.</summary>
		object ConvertFromJson([NotNull]Type targetType, [NotNull]JsonValue source, bool throwOnError);

		/// <summary>Converts provided entity using provided <see cref="IEntityConfig"/> to JSON.</summary>
		JsonObject ConvertToJson([NotNull]object sourceEntity, [NotNull]IEntityConfig entityConfig, bool throwOnError);

		/// <summary>Converts provided JSON to entity using provided <see cref="IEntityConfig"/>.</summary>
		object ConvertFromJson([NotNull]IEntityConfig entityConfig, [NotNull]JsonObject source, bool throwOnError);
	}
}
