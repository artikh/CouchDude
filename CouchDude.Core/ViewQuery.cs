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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using CouchDude.Core.Api;

namespace CouchDude.Core
{
	/// <summary>Describes typed CouchDB view query.</summary>
	public class ViewQuery<T>: ViewQuery
	{
		/// <summary>Type of the row result item.</summary>
		public Type RowType { get { return typeof (T); } }
	}

	/// <summary>Describes CouchDB view query.</summary>
	/// <remarks>http://wiki.apache.org/couchdb/HTTP_view_API#Querying_Options</remarks>
	public class ViewQuery
	{
		/// <summary>Design document name (id without '_design/' prefix) to use view from.</summary>
		public string DesignDocumentName;

		/// <summary>View name.</summary>
		public string ViewName;

		/// <summary>Key to fetch view rows by.</summary>
		public object Key;

		/// <summary>Key to start view result fetching from.</summary>
		public object StartKey;

		/// <summary>Document id to start view result fetching from.</summary>
		/// <remarks>Should allways be used with <see cref="StartKey"/>.</remarks>
		public string StartDocumentId;

		/// <summary>Key to stop view result fetching by.</summary>
		public object EndKey;

		/// <summary>Document id to stop view result fetching by.</summary>
		/// <remarks>Should allways be used with <see cref="EndKey"/>.</remarks>
		public string EndDocumentId;

		/// <summary>Flag that indicates that query should run multiple reduce</summary>
		public bool Group;

		/// <summary>Indicates level of grouping which used when query executed</summary>
		public int? GroupLevel;

		/// <summary>Limit the number of view rows in the output.</summary>
		public int? Limit;

		/// <summary>Sets number of view rows to skip when fetching.</summary>
		/// <remarks>You should not set this to more then single digit values.</remarks>
		public int? Skip;

		/// <summary>CouchDB will not refresh the view even if it is stalled.</summary>
		public bool StaleViewIsOk;

		/// <summary>Fetches view backwards.</summary>
		/// <remarks>You should switch <see cref="StartKey"/> with <see cref="EndKey"/>
		/// when using this.</remarks>
		public bool FetchDescending;

		/// <summary>If set makes CouchDB to do not use reduce part of the view.</summary>
		public bool SuppressReduce;

		/// <summary>Prompts database to include corresponding document as a part of each
		/// view result row.</summary>
		public bool IncludeDocs;

		/// <summary>If set requires database to treat requested key range
		/// as exclusive at the end.</summary>
		public bool DoNotIncludeEndKey;

		/// <summary>Gets query URI.</summary>
		public string ToUri()
		{
			// http://wiki.apache.org/couchdb/HTTP_view_API#Querying_Options
			var uriBuilder = new ViewUriBuilder(DesignDocumentName, ViewName);
			uriBuilder.AddIfNotNull    (Key,                "key"                    );
			uriBuilder.AddIfNotNull    (StartKey,           "startkey"               );
			uriBuilder.AddIfNotNull    (StartDocumentId,    "startkey_docid"         );
			uriBuilder.AddIfNotNull    (EndKey,             "endkey"                 );
			uriBuilder.AddIfNotNull    (EndDocumentId,      "endkey_docid"           );
			uriBuilder.AddIfHasValue   (Limit,              "limit"                  );
			uriBuilder.AddIfHasValue   (Skip,               "skip"                   );
			uriBuilder.AddIfTrue       (StaleViewIsOk,      "stale",         "ok"    );
			uriBuilder.AddIfTrue       (FetchDescending,    "descending",    "true"  );
			uriBuilder.AddIfTrue       (SuppressReduce,     "reduce",        "false" );
			uriBuilder.AddIfTrue       (IncludeDocs,        "include_docs",  "true"  );
			uriBuilder.AddIfTrue       (DoNotIncludeEndKey, "inclusive_end", "false" );
			uriBuilder.AddIfTrue       (Group,              "group",         "true"  );
			uriBuilder.AddIfHasValue   (GroupLevel,         "group_level"            );
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