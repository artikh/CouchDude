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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CouchDude.Utils;
using JetBrains.Annotations;

namespace CouchDude.Impl
{
	/// <summary>Converts <see cref="LuceneQuery"/> to <see cref="Uri"/>, <see cref="string"/> and back.</summary>
	public class LuceneQueryUriConverter : TypeConverter
	{
		private static readonly OptionListSerializer<LuceneQuery> Serializer =
			new OptionListSerializer<LuceneQuery>(
				new StringOption<LuceneQuery>(q => q.Query, "q"),
				new StringOption<LuceneQuery>(q => q.Analyzer, "analyzer"),
				new BooleanOption<LuceneQuery>(q => q.SuppressCaching, "debug", "true", "false", defaultValue: "false"),
				new BooleanOption<LuceneQuery>(q => q.UseConjunctionSematics, "default_operator", "AND", "OR", defaultValue: "OR"),
				new BooleanOption<LuceneQuery>(q => q.IncludeDocs, "include_docs", "true", "false", defaultValue: "false"),
				new CustomValueOption<LuceneQuery, ICollection<string>>(
					q => q.Fields, "include_fields", defaultValue: null,
					deserialize: stringValue => {
					  var fields = stringValue.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
					  return fields.Length == 0? null: fields;
					  },
					serialize: value => value == null? null: string.Join(",", value)
				),
				new PositiveIntegerOption<LuceneQuery>(q => q.Limit, "limit"),
				new PositiveIntegerOption<LuceneQuery>(q => q.Skip, "skip"),
				new CustomValueOption<LuceneQuery, IList<LuceneSort>>(
					q => q.Sort, "sort", defaultValue: null,
					deserialize: stringValue => {
					  var sorts = stringValue
					    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
					    .Select(
					      s => {
					        LuceneSort sort;
					        if (LuceneSort.TryParse(s, out sort))
					          return sort;
					        return null;
					      })
					    .Where(s => s != null)
					    .ToArray();
					  return sorts.Length == 0 ? null : sorts;
					  },
					serialize: value => value == null? null: string.Join(",", value.Select(s => s.ToString()))
				),
				new BooleanOption<LuceneQuery>(q => q.DoNotBlockIfStale, "stale", "ok", null, defaultValue: null)
			);


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


		internal static LuceneQuery TryParse(string uriString)
		{
			Uri uri;
			return Uri.TryCreate(uriString, UriKind.Relative, out uri) ? TryParse(uri) : null;
		}

		internal static LuceneQuery TryParse(Uri uri)
		{
			LuceneQuery viewQuery;
			return TryParse(uri, out viewQuery) ? viewQuery : null;
		}

		internal static bool TryParse(string uriString, out LuceneQuery viewQuery)
		{
			Uri uri;
			viewQuery = null;
			return Uri.TryCreate(uriString, UriKind.Relative, out uri) && TryParse(uri, out viewQuery);
		}

		internal static bool TryParse(Uri uri, out LuceneQuery viewQuery)
		{
			if (uri.IsAbsoluteUri)
			{
				viewQuery = null;
				return false;
			}

			var match = Regex.Match(
				uri.ToString(), "^_fti/_design/(?<designDocName>.*?)/(?<indexName>.*?)(?:\\?(?<queryString>.*))?$");
			if (match.Success)
			{
				viewQuery = new LuceneQuery {
					DesignDocumentName = match.Groups["designDocName"].Value,
					IndexName = match.Groups["indexName"].Value
				};

				var queryStringGroup = match.Groups["queryString"];
				if (queryStringGroup.Success)
					Serializer.Parse(queryStringGroup.Value, ref viewQuery);

				return true;
			}
			viewQuery = null;
			return false;
		}

		[Pure]
		internal static Uri ToUri(LuceneQuery viewQuery)
		{
			var uriString = ToUriString(viewQuery);
			return uriString == null? null: new Uri(uriString, UriKind.Relative);
		}

		internal static string ToUriString(LuceneQuery viewQuery)
		{
			if (viewQuery.DesignDocumentName.HasNoValue() || viewQuery.IndexName.HasNoValue())
				return null;

			var uri = new StringBuilder();
			uri.Append("_fti/_design/").Append(viewQuery.DesignDocumentName).Append("/").Append(viewQuery.IndexName);

			var queryString = Serializer.ToQueryString(viewQuery);
			if (queryString.Length > 0)
				uri.Append("?").Append(queryString);

			return uri.ToString();
		}
	}
}