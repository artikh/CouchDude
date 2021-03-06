#region Licence Info 
/*
	Copyright 2011 � Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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
using CouchDude.Configuration;

namespace CouchDude.Configuration.Builders
{
	/// <summary>Helper builder for <see cref="SettingsBuilder"/></summary>
	public class SingleEntityConfigBuilder: SingleEntityConfigBuilder<SingleEntityConfigBuilder>
	{
		/// <constructor />
		public SingleEntityConfigBuilder(Type entityType, SettingsBuilder parent) : base(entityType, parent) { }
	}

	/// <summary>Helper builder for <see cref="SettingsBuilder"/></summary>
	public class SingleEntityConfigBuilder<TSelf> : EntityConfigBuilder<TSelf> where TSelf: SingleEntityConfigBuilder<TSelf>
	{
		/// <constructor />
		public SingleEntityConfigBuilder(Type entityType, SettingsBuilder parent): base(parent)
		{
			Predicates.Add(t => t == entityType);
			AssembliesToScan.Add(entityType.Assembly);
		}
	}
}