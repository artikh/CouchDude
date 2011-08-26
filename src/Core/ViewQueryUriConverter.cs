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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CouchDude.Api;

namespace CouchDude
{
	/// <summary>Converts <see cref="ViewQuery"/> to <see cref="Uri"/>, <see cref="string"/> and back.</summary>
	public class ViewQueryUriConverter: TypeConverter
	{
		private static readonly string[] SpecialViewNames = new[] { "_all_docs" };

		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string) || sourceType == typeof(Uri))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}

		/// <inheritdoc/>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var uri = value as Uri;
			if (uri != null)
				return TryParse(uri);

			var uriString = value as string;
			if (uriString != null)
				return TryParse(uriString);

			return base.ConvertFrom(context, culture, value);
		}

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string)
			       || destinationType == typeof(Uri)
			       || base.CanConvertTo(context, destinationType);
		}

		/// <inheritdoc/>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			var viewQuery = value as ViewQuery;
			if (viewQuery != null)
			{
				if (destinationType == typeof (string))
					return ToUriString(viewQuery);

				if (destinationType == typeof (Uri))
					return ToUriString(viewQuery);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		internal static ViewQuery TryParse(string uriString)
		{
			Uri uri;
			return Uri.TryCreate(uriString, UriKind.Relative, out uri) ? TryParse(uri): null;
		}

		internal static ViewQuery TryParse(Uri uri)
		{
			var viewQuery = new ViewQuery();
			return TryParse(uri, viewQuery) ? viewQuery : null;
		}

		internal static bool TryParse(string uriString, ViewQuery viewQuery)
		{
			Uri uri;
			return Uri.TryCreate(uriString, UriKind.Relative, out uri) && TryParse(uri, viewQuery);
		}

		internal static bool TryParse(Uri uri, ViewQuery viewQuery)
		{
			if (uri.IsAbsoluteUri) return false;

			var match = Regex.Match(
				uri.ToString(), "^(?:(?<specialView>_all_docs)|_design/(?<designDocName>.*?)/_view/(?<viewName>.*?))(?:\\?(?<queryString>.*))?$");
			if (match.Success)
			{
				var specialViewGroup = match.Groups["specialView"];
				if (specialViewGroup.Success)
					viewQuery.ViewName = specialViewGroup.Value;
				else
				{
					viewQuery.DesignDocumentName = match.Groups["designDocName"].Value;
					viewQuery.ViewName = match.Groups["viewName"].Value;
				}

				var queryStringGroup = match.Groups["queryString"];
				if (queryStringGroup.Success)
				{
					var queryString = queryStringGroup.Value;
					ParseQueryString(viewQuery, queryString);
				}

				return true;
			}
			return false;
		}

		private static dynamic TryParseKey(string keyString)
		{
			try
			{
				return new JsonFragment(keyString);
			}
			catch
			{
				return null;
			}
		}

		private static int? TryParseInt(string intString)
		{
			int limit;
			return int.TryParse(intString, NumberStyles.Integer, CultureInfo.InvariantCulture, out limit)
				? limit
				: (int?)null;
		}
		
		private static void ParseQueryString(ViewQuery viewQuery, string queryString)
		{
			var values = HttpUtility.ParseQueryString(queryString);
			foreach (string key in values)
			{
				var value = values[key];
				switch (key)
				{
					case "key":
						viewQuery.Key = TryParseKey(value);
						break;
					case "startkey":
						viewQuery.StartKey = TryParseKey(value);
						break;
					case "startkey_docid":
						viewQuery.StartDocumentId = value;
						break;
					case "endkey":
						viewQuery.EndKey = TryParseKey(value);
						break;
					case "endkey_docid":
						viewQuery.EndDocumentId = value;
						break;
					case "limit":
						viewQuery.Limit = TryParseInt(value);
						break;
					case "skip":
						viewQuery.Skip = TryParseInt(value);
						break;
					case "stale":
						if (value == "update_after")
						{
							viewQuery.UpdateIfStale = true;
							viewQuery.StaleViewIsOk = true;
						}
						else if (value == "ok") 
							viewQuery.StaleViewIsOk = true;
						break;
					case "descending":
						if (value == "true") 
							viewQuery.FetchDescending = true;
						break;
					case "reduce":
						if (value == "false")
							viewQuery.SuppressReduce = true;
						break;
					case "include_docs":
						if (value == "true")
							viewQuery.IncludeDocs = true;
						break;
					case "inclusive_end":
						if (value == "false")
							viewQuery.DoNotIncludeEndKey = true;
						break;
					case "group":
						if (value == "true")
							viewQuery.Group = true;
						break;
					case "group_level":
						viewQuery.GroupLevel = TryParseInt(value);
						break;
				}
			}
		}

		internal static Uri ToUri(ViewQuery viewQuery)
		{
			return new Uri(ToUriString(viewQuery), UriKind.Relative);
		}

		internal static string ToUriString(ViewQuery viewQuery)
		{
			// http://wiki.apache.org/couchdb/HTTP_view_API#Querying_Options
			var uriBuilder = new ViewUriBuilder(viewQuery.DesignDocumentName, viewQuery.ViewName);
			uriBuilder.AddIfNotNull  (viewQuery.Key,                "key"                    );
			uriBuilder.AddIfNotNull  (viewQuery.StartKey,           "startkey"               );
			uriBuilder.AddIfNotNull  (viewQuery.StartDocumentId,    "startkey_docid"         );
			uriBuilder.AddIfNotNull  (viewQuery.EndKey,             "endkey"                 );
			uriBuilder.AddIfNotNull  (viewQuery.EndDocumentId,      "endkey_docid"           );
			uriBuilder.AddIfHasValue (viewQuery.Limit,              "limit"                  );
			uriBuilder.AddIfHasValue (viewQuery.Skip,               "skip"                   );
			uriBuilder.AddIfTrue     (viewQuery.StaleViewIsOk,      "stale",         viewQuery.UpdateIfStale? "update_after": "ok"    );
			uriBuilder.AddIfTrue     (viewQuery.FetchDescending,    "descending",    "true"  );
			uriBuilder.AddIfTrue     (viewQuery.SuppressReduce,     "reduce",        "false" );
			uriBuilder.AddIfTrue     (viewQuery.IncludeDocs,        "include_docs",  "true"  );
			uriBuilder.AddIfTrue     (viewQuery.DoNotIncludeEndKey, "inclusive_end", "false" );
			uriBuilder.AddIfTrue     (viewQuery.Group,              "group",         "true"  );
			uriBuilder.AddIfHasValue (viewQuery.GroupLevel,         "group_level"            );
			return uriBuilder.ToUriString();
		}

		private class ViewUriBuilder
		{
			private readonly NameValueCollection querySring = new NameValueCollection();
			private readonly string designDocumentName;
			private readonly string viewName;

			public ViewUriBuilder(string designDocumentName, string viewName)
			{
				if(String.IsNullOrEmpty(viewName))
					throw new QueryException("View name is required.");
				if (designDocumentName == null && !SpecialViewNames.Contains(viewName))
					throw new QueryException("Querying view {0} requires design document name to be specified.", viewName);
				

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
				{
					var jsonFragment = value as IJsonFragment ?? JsonFragment.Serialize(value);
					querySring[key] = jsonFragment.ToString();
				}
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

			public string ToUriString()
			{
				var uri = new StringBuilder();
				if (!String.IsNullOrEmpty(designDocumentName))
					uri.Append("_design/").Append(designDocumentName).Append("/_view/");
				uri.Append(viewName);

				if(querySring.Count > 0)
					uri.Append("?");

				foreach (string key in querySring)
				{
					var stringValue = querySring[key];
					uri.Append(key);
					if (!String.IsNullOrEmpty(stringValue))
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