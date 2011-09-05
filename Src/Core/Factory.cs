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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CouchDude.Http;
using CouchDude.Impl;

namespace CouchDude
{
	/// <summary>Factory class for main CouchDude API classes.</summary>
	public static class Factory
	{
		/// <summary>Creates session factory from provided settings.</summary>
		public static ISessionFactory CreateSessionFactory(this Settings settings)
		{
			return new CouchSessionFactory(settings);
		}

		/// <summary>Creates session factory from provided settings and <see cref="IHttpClient"/>.</summary>
		public static ISessionFactory CreateSessionFactory(this Settings settings, IHttpClient httpClient)
		{
			return new CouchSessionFactory(settings);
		}
	}
}
