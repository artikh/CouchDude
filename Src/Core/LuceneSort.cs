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
using System.Text;
using System.Text.RegularExpressions;

namespace CouchDude
{
	/// <summary>Lucene query result sort order.</summary>
	public class LuceneSort
	{
		/// <summary>Name of feild to sort on</summary>
		public readonly string FieldName;

		/// <summary>Sort order</summary>
		public readonly bool SortDescending;

		/// <summary>Sort ordering type</summary>
		public readonly LuceneType? Type;

		/// <contructor/>
		public LuceneSort(string fieldName, bool sortDescending = false, LuceneType? type = null)
		{
			FieldName = fieldName;
			SortDescending = sortDescending;
			Type = type;
		}

		/// <summary>Attemps to parse sort descriptor.</summary>
		public static bool TryParse(string sortString, out LuceneSort sort)
		{
			var match = Regex.Match(sortString, @"^(?<sortDirection>[\\/])?(?<fieldName>.*?)(?:<(?<type>.*?)>)?$");
			if(match.Success)
			{
				var sortDescending = false;
				var sortDirectionGroup = match.Groups["sortDirection"];
				if (sortDirectionGroup.Success && sortDirectionGroup.Value == "\\")
					sortDescending = true;

				LuceneType? type = null;
				var typeGroup = match.Groups["type"];
				if(typeGroup.Success)
				{
					LuceneType luceneType;
					if (Enum.TryParse(typeGroup.Value, ignoreCase: true, result: out luceneType))
						type = luceneType;
				}

				sort = new LuceneSort(match.Groups["fieldName"].Value, sortDescending, type);
				return true;
			}

			sort = null;
			return false;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			var result = new StringBuilder();
			if (SortDescending)
				result.Append("\\");
			if (result.Length == 0 && Type == null)
				return FieldName;
			result.Append(FieldName);
			if (Type != null)
				result.Append("<").Append(Type.Value.ToString().ToLower()).Append(">");
			return result.ToString();
		}
	}
}