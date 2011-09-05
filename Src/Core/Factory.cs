﻿#region Licence Info 
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
using CouchDude.Api;
using CouchDude.Http;
using CouchDude.Impl;

namespace CouchDude
{
	/// <summary>Factory class for main CouchDude API classes.</summary>
	public static class Factory
	{

		/// <summary>Creates session factory from provided <paramref name="settings"/> 
		/// and <paramref name="httpClient"/> if provided.</summary>
		public static ISessionFactory CreateSessionFactory(this Settings settings, IHttpClient httpClient = null)
		{
			if(settings == null) throw new ArgumentNullException("settings");
			if(settings.Incomplete) throw new ArgumentException("Settings object initalization have not finished yet.", "settings");

			return new CouchSessionFactory(settings, CreateCouchApi(settings.ServerUri, httpClient));
		}

		/// <summary>Creates standard implementation of <see cref="IHttpClient"/>.</summary>
		public static IHttpClient CreateHttpClient()
		{
			return new HttpClientImpl();
		}

		/// <summary>Creates new <see cref="ICouchApi"/> instance associated with provided server address using <paramref name="httpClient"/> if provided.</summary>
		public static ICouchApi CreateCouchApi(string serverAddress, IHttpClient httpClient = null)
		{
			return CreateCouchApi(new Uri(serverAddress, UriKind.RelativeOrAbsolute), httpClient);
		}

		/// <summary>Creates new <see cref="ICouchApi"/> instance associated with provided server address using <paramref name="httpClient"/> if provided.</summary>
		public static ICouchApi CreateCouchApi(Uri serverAddress, IHttpClient httpClient = null)
		{
			if (serverAddress == null) throw new ArgumentNullException("serverAddress");
			if (!serverAddress.IsAbsoluteUri) throw new ArgumentException("Server address should be absolute URI.", "serverAddress");

			return new CouchApi(httpClient ?? CreateHttpClient(), serverAddress);
		}
	}
}
