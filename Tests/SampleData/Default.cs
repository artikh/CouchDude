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

namespace CouchDude.Tests.SampleData
{
	static class Default
	{
		public static Settings Settings
		{
			get 
			{ 
				return ConfigureCouchDude.With()
					.ServerUri("http://127.0.0.1:5984")
					.DefaultDatabaseName("testdb")
					.MappingEntities()
						.FromAssemblyOf<IEntity>()
						.Implementing<IEntity>()
						.TranslatingEntityIdToDocumentIdAs(
							(id, type, documentType) => string.Concat(documentType, ".", id))
						.TranslatingDocumentIdToEntityIdAs((id, type, entityType) => id.Split('.')[1])
					.CreateSettings(); 
			}
		}
	}
}
