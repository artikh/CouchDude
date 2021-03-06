﻿#region Licence Info 
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
using System.Text;
using JetBrains.Annotations;

namespace CouchDude.Utils
{
	/// <summary>Utility methods for string manipulation.</summary>
	public static class StringUtils
	{
		/// <summary>Converts first letter of the string to </summary>
		[Pure]
		public static string ToCamelCase(this string self)
		{
			if(string.IsNullOrEmpty(self)) throw new ArgumentNullException("self");
			

			var firstLetter = self[0];
			if (!Char.IsUpper(firstLetter))
				return self;

			var output = new StringBuilder(self);
			output[0] = Char.ToLower(firstLetter);
			return output.ToString();
		}

		/// <summary>Inverts <see cref="string.IsNullOrEmpty"/> to be more convinient.</summary>
		[Pure]
		public static bool HasNoValue(this string self)
		{
			return string.IsNullOrEmpty(self);
		}

		/// <summary>Inverts <see cref="string.IsNullOrEmpty"/> and negates it to be more convinient.</summary>
		[Pure]
		public static bool HasValue(this string self)
		{
			return !string.IsNullOrEmpty(self);
		}
	}
}
