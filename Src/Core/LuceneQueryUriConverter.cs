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
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Web;

namespace CouchDude
{
	/// <summary>Converts <see cref="LuceneQuery"/> to <see cref="Uri"/>, <see cref="string"/> and back.</summary>
	public class LuceneQueryUriConverter : TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof (string)
			       || destinationType == typeof (Uri)
			       || base.CanConvertTo(context, destinationType);
		}

		/// <inheritdoc/>
		public override object ConvertTo(
			ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string))
				return ToUriString((LuceneQuery)value);
			else if (destinationType == typeof (Uri))
			{
				var uriString = ToUriString((LuceneQuery)value);
				Uri uri;
				return Uri.TryCreate(uriString, UriKind.Relative, out uri) ? uri : null;
			}
			else
				return base.ConvertTo(context, culture, value, destinationType);
		}

		internal static string ToUriString(LuceneQuery viewQuery)
		{
			var uriBuilder = new StringBuilder();
			uriBuilder.Append("_fti/_design/" + viewQuery.DesignDocumentName + "/");
			uriBuilder.Append(viewQuery.IndexName);
			uriBuilder.Append("?");
			uriBuilder.Append("q=" + HttpUtility.UrlEncode(viewQuery.Query));
			if (viewQuery.IncludeDocs)
				uriBuilder.Append("&include_docs=true");
			if (viewQuery.Analyzer != null)
				uriBuilder.Append("&analyzer=" + HttpUtility.UrlEncode(viewQuery.Analyzer));
			if (viewQuery.Sort != null)
			{
				uriBuilder.Append("&sort=");
				var oneItemInList = true;
				foreach (var luceneSort in viewQuery.Sort)
				{
					var sortLuceneString = (!oneItemInList ? "," : "") + (luceneSort.SortDescending ? "\\" : "/") + luceneSort.FieldName;
					uriBuilder.Append(sortLuceneString);
					oneItemInList = false;
				}
			}
			return uriBuilder.ToString();
		}
	}
}