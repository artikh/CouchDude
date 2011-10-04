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
using CouchDude.Utils;
using JetBrains.Annotations;

namespace CouchDude.Api
{
	internal struct DbUriConstructor
	{
		public readonly string DatabaseName;
		public Uri DatabaseUri;
			
		public DbUriConstructor(UriConstructor parent, string databaseName): this()
		{
			DatabaseName = databaseName;
			DatabaseUri = new Uri(parent.ServerUri, databaseName + "/");
		}

		[Pure]
		public Uri GetFullDocumentUri(string docId, string revision = null)
		{
			var documentUriString = GetDocumentUriString(docId, revision);
			return new Uri(DatabaseUri, documentUriString).LeaveDotsAndSlashesEscaped();
		}

		[Pure]
		public Uri GetFullAttachmentUri(string attachmentId, string docId, string revision = null)
		{
			var attachmentUriString = GetAttachmentUriString(attachmentId, docId, revision);
			return new Uri(DatabaseUri, attachmentUriString).LeaveDotsAndSlashesEscaped();
		}

		[Pure]
		public string GetDocumentUriString(string docId, string revision)
		{
			var uriStringBuilder = new StringBuilder();
			AppendDocId(uriStringBuilder, docId);
			AppendRevisionIfNeeded(uriStringBuilder, revision);
			return uriStringBuilder.ToString();
		}

		[Pure]
		private string GetAttachmentUriString(string attachmentId, string docId, string revision = null)
		{
			var uriStringBuilder = new StringBuilder();
			AppendDocId(uriStringBuilder, docId);
			uriStringBuilder.Append("/");
			AppendReplacingFowardSlash(uriStringBuilder, attachmentId);
			AppendRevisionIfNeeded(uriStringBuilder, revision);
			return uriStringBuilder.ToString();
		}

		private static void AppendRevisionIfNeeded(StringBuilder uriStringBuilder, string revision)
		{
			if (!String.IsNullOrEmpty(revision))
				uriStringBuilder.Append("?rev=").Append(revision);
		}

		private static void AppendDocId(StringBuilder uriStringBuilder, string docId)
		{
			const string designDocumentPrefix = "_design/";
			if (docId.StartsWith(designDocumentPrefix))
			{
				uriStringBuilder.Append(designDocumentPrefix);
				AppendReplacingFowardSlash(uriStringBuilder, docId.Substring(designDocumentPrefix.Length));
			}
			else
				uriStringBuilder.Append(docId);
		}

		private static void AppendReplacingFowardSlash(StringBuilder stringBuilder, string stringToAppend)
		{
			var originalLength = stringBuilder.Length;
			stringBuilder.Append(stringToAppend);
			stringBuilder.Replace("/", "%2F", originalLength, stringToAppend.Length);
		}

		[Pure]
		public Uri GetQueryUri(IQuery query)
		{
			return new Uri(DatabaseUri, query.ToUri());
		}
	}
}