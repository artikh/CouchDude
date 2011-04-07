using System.Collections.Specialized;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core
{
	/// <summary>Describes CouchDB view query.</summary>
	public class ViewQuery
	{
		/// <summary>Design document name (id without '_design/' prefix) to use view from.</summary>
		public string DesignDocumentName;

		/// <summary>View name.</summary>
		public string ViewName;

		/// <summary>Key to fetch view rows by.</summary>
		public JToken Key;

		/// <summary>Key to start view result fetching from.</summary>
		public JToken StartKey;

		/// <summary>Document id to start view result fetching from.</summary>
		/// <remarks>Should allways be used with <see cref="StartKey"/>.</remarks>
		public string StartDocumentId;

		/// <summary>Key to stop view result fetching by.</summary>
		public JToken EndKey;

		/// <summary>Document id to stop view result fetching by.</summary>
		/// <remarks>Should allways be used with <see cref="EndKey"/>.</remarks>
		public string EndDocumentId;

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
			uriBuilder.AddIfNotNull	(	Key,								"key"											);
			uriBuilder.AddIfNotNull	(	StartKey,						"startkey"								);
			uriBuilder.AddIfNotNull	(	StartDocumentId,		"startkey_docid"					);
			uriBuilder.AddIfNotNull	(	EndKey,							"endkey"									);
			uriBuilder.AddIfNotNull	(	EndDocumentId,			"endkey_docid"						);
			uriBuilder.AddIfHasValue(	Limit,							"limit"										);
			uriBuilder.AddIfHasValue(	Skip,								"skip"										);
			uriBuilder.AddIfTrue		(	StaleViewIsOk,			"stale",					"ok"		);
			uriBuilder.AddIfTrue		(	FetchDescending,		"descending",			"true"	);
			uriBuilder.AddIfTrue		(	SuppressReduce,			"reduce",					"false"	);
			uriBuilder.AddIfTrue		(	IncludeDocs,				"include_docs",		"true"	);
			uriBuilder.AddIfTrue		(	DoNotIncludeEndKey,	"inclusive_end",	"false"	);
			return uriBuilder.ToUri();
		}

		private class ViewUriBuilder
		{
			readonly NameValueCollection querySring = new NameValueCollection();
			private readonly string designDocumentName;
			private readonly string viewName;

			public ViewUriBuilder(string designDocumentName, string viewName)
			{
				this.designDocumentName = designDocumentName;
				this.viewName = viewName;
			}

			public void AddIfNotNull<TValue>(TValue value, string key) where TValue: class 
			{
				if (value != null)
					querySring[key] = value.ToString();
			}

			public void AddIfNotNull(JToken value, string key)
			{
				if (value != null)
					querySring[key] = value.ToString(Formatting.None);
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
					uri.Append("_design/").Append(designDocumentName).Append("/");
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