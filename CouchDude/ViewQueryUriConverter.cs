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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Web;
using CouchDude.Api;

namespace CouchDude
{
	/// <summary>Converts <see cref="ViewQuery"/> to <see cref="Uri"/>, <see cref="string"/> and back.</summary>
	public class ViewQueryUriConverter: TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string)
			       || destinationType == typeof(Uri)
			       || base.CanConvertTo(context, destinationType);
		}

		/// <inheritdoc/>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
				return ToUriString((ViewQuery)value);
			else if (destinationType == typeof (Uri))
			{
				var uriString = ToUriString((ViewQuery)value);
				Uri uri;
				return Uri.TryCreate(uriString, UriKind.Relative, out uri)? uri: null;
			}
			else
				return base.ConvertTo(context, culture, value, destinationType);
		}

		internal static string ToUriString(ViewQuery viewQuery)
		{
			// http://wiki.apache.org/couchdb/HTTP_view_API#Querying_Options
			var uriBuilder = new ViewUriBuilder(viewQuery.DesignDocumentName, viewQuery.ViewName);
			uriBuilder.AddIfNotNull    (viewQuery.Key,                "key"                    );
			uriBuilder.AddIfNotNull    (viewQuery.StartKey,           "startkey"               );
			uriBuilder.AddIfNotNull    (viewQuery.StartDocumentId,    "startkey_docid"         );
			uriBuilder.AddIfNotNull    (viewQuery.EndKey,             "endkey"                 );
			uriBuilder.AddIfNotNull    (viewQuery.EndDocumentId,      "endkey_docid"           );
			uriBuilder.AddIfHasValue   (viewQuery.Limit,              "limit"                  );
			uriBuilder.AddIfHasValue   (viewQuery.Skip,               "skip"                   );
			uriBuilder.AddIfTrue       (viewQuery.StaleViewIsOk,      "stale",         "ok"    );
			uriBuilder.AddIfTrue       (viewQuery.FetchDescending,    "descending",    "true"  );
			uriBuilder.AddIfTrue       (viewQuery.SuppressReduce,     "reduce",        "false" );
			uriBuilder.AddIfTrue       (viewQuery.IncludeDocs,        "include_docs",  "true"  );
			uriBuilder.AddIfTrue       (viewQuery.DoNotIncludeEndKey, "inclusive_end", "false" );
			uriBuilder.AddIfTrue       (viewQuery.Group,              "group",         "true"  );
			uriBuilder.AddIfHasValue   (viewQuery.GroupLevel,         "group_level"            );
			return uriBuilder.ToUri();
		}

		private class ViewUriBuilder
		{
			private static readonly string[] SpecialViewNames = new[] { "_all_docs" }; 

			private readonly NameValueCollection querySring = new NameValueCollection();
			private readonly string designDocumentName;
			private readonly string viewName;

			public ViewUriBuilder(string designDocumentName, string viewName)
			{
				if(string.IsNullOrEmpty(viewName))
					throw new QueryException("View name is required.");
				if (designDocumentName == null && !SpecialViewNames.Contains(viewName))
					throw new QueryException("Querying view {0} requires design document name to be specified.", viewName);
				Contract.EndContractBlock();

				this.designDocumentName = designDocumentName;
				this.viewName = viewName;
			}

			public void AddIfNotNull<TValue>(TValue value, string key) where TValue: class 
			{
				if (value != null)
					querySring[key] = value.ToString();
			}

			public void AddIfNotNull(object value, string key)
			{
				if (value != null)
					querySring[key] = JsonFragment.Serialize(value).ToString();
			}

			public void AddIfHasValue<TValue>(TValue? value, string key) where TValue: struct 
			{
				if (value.HasValue)
					querySring[key] = value.Value.ToString();
			}

			public void AddIfTrue(bool value, string key, string valueString)
			{
				if (value)
					querySring[key] = valueString;
			}

			public string ToUri()
			{
				var uri = new StringBuilder();
				if (!string.IsNullOrEmpty(designDocumentName))
					uri.Append("_design/").Append(designDocumentName).Append("/_view/");
				uri.Append(viewName);

				if(querySring.Count > 0)
					uri.Append("?");

				foreach (string key in querySring)
				{
					var stringValue = querySring[key];
					uri.Append(key);
					if (!string.IsNullOrEmpty(stringValue))
						uri.Append("=").Append(HttpUtility.UrlEncode(stringValue));
					uri.Append("&");
				}


				if (querySring.Count > 0)
					uri.Remove(uri.Length - 1, 1); // Removing last '&'

				return uri.ToString();
			}
		}
	}
}