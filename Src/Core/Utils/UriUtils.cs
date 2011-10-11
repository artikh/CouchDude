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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CouchDude.Utils
{
	/// <summary><see cref="Uri"/>-related utility class.</summary>
	public static class UriUtils
	{
		// System.UriSyntaxFlags is internal, so let's duplicate the flag privately
		private const int UnEscapeDotsAndSlashes = 0x2000000;
		private const int SimpleUserSyntax = 0x20000;

		/// <summary>Reverts default <see cref="Uri"/> behaviour of unescaping slashes and dots in path.</summary>
		public static Uri LeaveDotsAndSlashesEscaped(this Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			FieldInfo fieldInfo = uri.GetType().GetField("m_Syntax", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
				throw new MissingFieldException("'m_Syntax' field not found");

			object uriParser = fieldInfo.GetValue(uri);
			fieldInfo = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
				throw new MissingFieldException("'m_Flags' field not found");

			object uriSyntaxFlags = fieldInfo.GetValue(uriParser);

			// Clear the flag that we don't want
			uriSyntaxFlags = (int)uriSyntaxFlags & ~UnEscapeDotsAndSlashes;
			uriSyntaxFlags = (int)uriSyntaxFlags & ~SimpleUserSyntax;
			fieldInfo.SetValue(uriParser, uriSyntaxFlags);

			return uri;
		}
		
		/// <summary>Parses query string into dictionary.</summary>
		public static IDictionary<string, string> ParseQueryString(string queryString)
		{
			var matches = Regex.Matches(queryString, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
			return matches.Cast<Match>().ToDictionary(
					m => Uri.UnescapeDataString(m.Groups[2].Value),
					m => Uri.UnescapeDataString(m.Groups[3].Value)
			);
		}
	}
}