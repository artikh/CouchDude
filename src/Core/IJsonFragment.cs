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
using System.Dynamic;
using System.IO;
using CouchDude.Api;

namespace CouchDude
{
	/// <summary>Represents simple JSON fragment. Data could be accessed as dynamic.</summary>
	public interface IJsonFragment : IDynamicMetaObjectProvider
	{
		/// <summary>Produces <see cref="TextReader"/> over content of the JSON fragmet.</summary>
		/// <remarks>Client code is responsible for disposing it.</remarks>
		TextReader Read();

		/// <summary>Deserializes current <see cref="JsonFragment"/> to object of provided <paramref name="type"/>.</summary>
		object Deserialize(Type type);

		/// <summary>Writes JSON string to provided text writer.</summary>
		void WriteTo(TextWriter writer);

		/// <summary>Grabs required property value throwing
		/// <see cref="ParseException"/> if not found or empty.</summary>
		string GetRequiredProperty(string name, string additionalMessage = null);

		/// <summary>Deserializes current <see cref="JsonFragment"/> to object of provided <paramref name="type"/> returning
		/// <c>null</c> if deserialization was unsuccessful..</summary>
		object TryDeserialize(Type type);
	}
}