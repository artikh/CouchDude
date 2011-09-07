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
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace CouchDude.Utils
{
	/// <summary>Commonly used checks.</summary>
	public static class CheckIf
	{
		private const string SpecialUserDb = "_users";
		private const string SpecialReplicatorDb = "_replicator";

		/// <summary>Checks if database name is valid.</summary>
		public static void DatabaseNameIsOk(string dbName, [InvokerParameterName]string paramName)
		{
			if (dbName != SpecialUserDb && dbName != SpecialReplicatorDb && !Regex.IsMatch(dbName, @"^[a-z][0-9a-z_$()+\-/]*$"))
				throw new ArgumentOutOfRangeException(
					paramName,
					dbName,
					"A database must be named with all lowercase letters (a-z), " +
					"digits (0-9), or any of the _$()+-/ characters and must end with a " +
					"slash in the URL. The name has to start with a lowercase letter (a-z).");
		}
	}
}
