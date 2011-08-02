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
using System.ComponentModel;

namespace CouchDude.Core
{
	/// <summary>Describes CouchDB view query.</summary>
	/// <remarks>http://wiki.apache.org/couchdb/HTTP_view_API#Querying_Options</remarks>
	[TypeConverter(typeof(ViewQueryUriConverter))]
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
		public override string ToString()
		{
			return ViewQueryUriConverter.ToUriString(this);
		}

		/// <summary>Gets query URI.</summary>
		public Uri ToUri()
		{
			return new Uri(ViewQueryUriConverter.ToUriString(this), UriKind.Relative);
		}
	}
}