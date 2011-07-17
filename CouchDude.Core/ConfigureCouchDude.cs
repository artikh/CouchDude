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
using CouchDude.Core.Impl;
using CouchDude.Core.Configuration.Builders;

namespace CouchDude.Core
{
	/// <summary>Start point of fluent configuration of CouchDude.</summary>
	public static class ConfigureCouchDude
	{
		/// <summary>Starts CouchDude configuration.</summary>
		public static SettingsBuilder With()
		{
			return new SettingsBuilder();
		}

		/// <summary>Creates session factory from provided setting.</summary>
		public static ISessionFactory CreateSessionFactory(this Settings settings)
		{
			return new CouchSessionFactory(settings);
		}

		/// <summary>Retrives service from service provider using type argument.</summary>
		public static TService GetService<TService>(this IServiceProvider serviceProvider)
		{
			return (TService) serviceProvider.GetService(typeof (TService));
		}
	}
}