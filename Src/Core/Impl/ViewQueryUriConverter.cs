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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CouchDude.Utils;
using JetBrains.Annotations;

namespace CouchDude.Impl
{
	/// <summary>Converts <see cref="ViewQuery"/> to <see cref="Uri"/>, <see cref="string"/> and back.</summary>
	public class ViewQueryUriConverter: TypeConverter
	{
		private static readonly OptionListSerializer<ViewQuery> Serializer =
			new OptionListSerializer<ViewQuery>(
				new JsonOption<ViewQuery>(q => q.Key, "key"),
				new JsonOption<ViewQuery>(q => q.StartKey, "startkey"),
				new StringOption<ViewQuery>(q => q.StartDocumentId, "startkey_docid"),
				new JsonOption<ViewQuery>(q => q.EndKey, "endkey"),
				new StringOption<ViewQuery>(q => q.EndDocumentId, "endkey_docid"),
				new PositiveIntegerOption<ViewQuery>(q => q.Limit, "limit"),
				new PositiveIntegerOption<ViewQuery>(q => q.Skip, "skip"),
				new BooleanOption<ViewQuery>(q => q.FetchDescending, "descending", "true", "false", defaultValue: "false"),
				new BooleanOption<ViewQuery>(q => q.IncludeDocs, "include_docs", "true", "false", defaultValue: "false"),
				new BooleanOption<ViewQuery>(q => q.IncludeUpdateSequenceNumber, "update_seq", "true", "false", defaultValue: "false"),
				new BooleanOption<ViewQuery>(q => q.DoNotIncludeEndKey, "inclusive_end", "false", "true", defaultValue: "true"),
				new BooleanOption<ViewQuery>(q => q.Group, "group", "true", "false", defaultValue: "false"),
				new PositiveIntegerOption<ViewQuery>(q => q.GroupLevel, "group_level"),
				new BooleanOption<ViewQuery>(q => q.SuppressReduce, "reduce", "false", "true", defaultValue: "true"),
				new CustomOption<ViewQuery>(
					"stale",
					defaultValue: null,
					getStringValue: query => query.StaleViewIsOk ? (query.UpdateIfStale ? "update_after" : "ok") : null,
					setStringValue: 
						(query, stringValue) => {
							switch (stringValue)
							{
								case "ok":
									query.StaleViewIsOk = true;
									query.UpdateIfStale = false;
									break;
								case "update_after":
									query.StaleViewIsOk = true;
									query.UpdateIfStale = true;
									break;
								default:
									query.StaleViewIsOk = false;
									query.UpdateIfStale = false;
									break;
							}
						})
				);

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
			ViewQuery viewQuery;
			return TryParse(uri, out viewQuery) ? viewQuery : null;
		}

		internal static bool TryParse(string uriString, out ViewQuery viewQuery)
		{
			Uri uri;
			viewQuery = null;
			return Uri.TryCreate(uriString, UriKind.Relative, out uri) && TryParse(uri, out viewQuery);
		}

		internal static bool TryParse(Uri uri, out ViewQuery viewQuery)
		{
			if (uri.IsAbsoluteUri)
			{
				viewQuery = null;
				return false;
			}

			var match = Regex.Match(
				uri.ToString(), "^(?:(?<specialView>_all_docs)|_design/(?<designDocName>.*?)/_view/(?<viewName>.*?))(?:\\?(?<queryString>.*))?$");
			if (match.Success)
			{
				viewQuery = new ViewQuery();
				var specialViewGroup = match.Groups["specialView"];
				if (specialViewGroup.Success)
				{
					var specialViewName = specialViewGroup.Value;
					if(!SpecialViewNames.Any(n => n == specialViewName))
						throw new QueryException("Design document name is required for view {0} to be queried", specialViewName);
					viewQuery.ViewName = specialViewName;
				}
				else
				{
					viewQuery.DesignDocumentName = match.Groups["designDocName"].Value;
					viewQuery.ViewName = match.Groups["viewName"].Value;
				}

				var queryStringGroup = match.Groups["queryString"];
				if (queryStringGroup.Success)
					Serializer.Parse(queryStringGroup.Value, ref viewQuery);

				return true;
			}
			viewQuery = null;
			return false;
		}

		[Pure]
		internal static Uri ToUri(ViewQuery viewQuery)
		{
			var uriString = ToUriString(viewQuery);
			return uriString == null? null: new Uri(uriString, UriKind.Relative);
		}

		internal static string ToUriString(ViewQuery viewQuery)
		{
			if (viewQuery.ViewName.HasNoValue() || (viewQuery.DesignDocumentName.HasNoValue() && !SpecialViewNames.Any(n => n == viewQuery.ViewName)))
				return null;

			var uri = new StringBuilder();
			if (!String.IsNullOrEmpty(viewQuery.DesignDocumentName))
				uri.Append("_design/").Append(viewQuery.DesignDocumentName).Append("/_view/");
			uri.Append(viewQuery.ViewName);

			var queryString = Serializer.ToQueryString(viewQuery);
			if (queryString.Length > 0)
				uri.Append("?").Append(queryString);

			return uri.ToString();
		}
	}
}