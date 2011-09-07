#region Licence Info 
/*
	Copyright 2011 � Artem Tikhomirov																					
																																					
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
using CouchDude.Configuration;

namespace CouchDude
{
	/// <summary>Describes CouchDB document.</summary>
	public interface IDocument : IDynamicMetaObjectProvider
	{
		/// <summary>Document identifier or <c>null</c> if no _id property 
		/// found or it's empty.</summary>
		string Id { get; set; }

		/// <summary>Revision of the document or <c>null</c> if no _rev property 
		/// found or it's empty.</summary>
		string Revision { get; set; }

		/// <summary>Type of the document or <c>null</c> if no type property 
		/// found or it's empty.</summary>
		string Type { get; set; }

		/// <summary>Deserializes document to new entity object.</summary>
		/// <param name="entityConfig">Entity configuration used to deserialize it properly.</param>
		object Deserialize(IEntityConfig entityConfig);

		/// <summary>Deserializes document to new entity object returning <c>null</c> insted of exception if
		/// it is impossible.</summary>
		/// <param name="entityConfig">Entity configuration used to deserialize it properly.</param>
		object TryDeserialize(IEntityConfig entityConfig);

		/// <summary>Produces <see cref="TextReader"/> over content of the JSON fragmet.</summary>
		/// <remarks>Client code is responsible for disposing it.</remarks>
		TextReader Read();

		/// <summary>Deserializes current <see cref="JsonFragment"/> to object of provided <paramref name="type"/>.</summary>
		object Deserialize(Type type);

		/// <summary>Writes JSON string to provided text writer.</summary>
		void WriteTo(TextWriter writer);

		/// <summary>Sets and gets string properties.</summary>
		IJsonFragment this[string propertyName] { get; set; }

		/// <summary>Creates new copy of the document.</summary>
		IDocument DeepClone();
	}
}