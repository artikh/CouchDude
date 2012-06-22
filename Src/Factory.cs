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
using System.Net.Http;
using CouchDude.Api;
using CouchDude.Impl;
using JetBrains.Annotations;

namespace CouchDude
{
	/// <summary>Factory class for main CouchDude API classes.</summary>
	public static class Factory
	{
		/// <summary>Creates session factory from provided <paramref name="settings"/>.</summary>
		public static ISessionFactory CreateSessionFactory(this Settings settings)
		{
			if(settings == null) throw new ArgumentNullException("settings");
			if(settings.Incomplete) throw new ArgumentException("Settings object initalization have not finished yet.", "settings");

			return new CouchSessionFactory(settings, s => new CouchApi(s.CouchApiSettings));
		}

		/// <summary>Creates <see cref="ICouchApi"/> instance from settings.</summary>
		public static ICouchApi CreateCouchApi([NotNull] this CouchApiSettings settings) { return CreateCouchApi(settings, null); }

		/// <summary>Creates <see cref="ICouchApi"/> instance from settings.</summary>
		public static ICouchApi CreateCouchApi([NotNull] this CouchApiSettings settings, HttpMessageHandler handler)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			return new CouchApi(settings, handler);
		}
	}
}
