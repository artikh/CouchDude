#region Licence Info 
/*
	Copyright 2011 � Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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
using System.Text;
using CouchDude.Utils;
using JetBrains.Annotations;

namespace CouchDude.Api
{
	internal struct DbUriConstructor
	{
		public readonly string DatabaseName;
		public readonly Uri DatabaseUri;
		private readonly string databaseNameWithSlash;
		private readonly Uri databaseUriWithSlash;
		private readonly Uri databaseFtiEndpoint;
		private Uri bulkUpdateUri;
		private Uri securityDescriptorUri;

		public DbUriConstructor(UriConstructor parent, string databaseName): this()
		{
			DatabaseName = databaseName;
			databaseFtiEndpoint = new Uri(new Uri(parent.ServerUri, "_fti/"), databaseName);
			DatabaseUri = new Uri(parent.ServerUri, databaseName);
			databaseNameWithSlash = databaseName + "/";
			databaseUriWithSlash = new Uri(DatabaseUri + "/");
		}

		public Uri BulkUpdateUri
		{
			get { return bulkUpdateUri ?? (bulkUpdateUri = new Uri(databaseUriWithSlash, "_bulk_docs")); }
		}

		public Uri SecurityDescriptorUri
		{
			get { return securityDescriptorUri ?? (securityDescriptorUri = new Uri(databaseUriWithSlash, "_security")); }
		}

		[Pure]
		public Uri GetQueryUri(ViewQuery query)
		{
			return new Uri(databaseUriWithSlash, query.ToUri());
		}

		[Pure]
		public Uri GetQueryUri(LuceneQuery query)
		{
			var sectionFtiBaseUri = new Uri(databaseFtiEndpoint, query.SectionName + "/");
			var databaseFtiBaseUri = new Uri(sectionFtiBaseUri, databaseNameWithSlash);
			return new Uri(databaseFtiBaseUri, query.ToUri());
		}

		[Pure]
		public Uri GetFullDocumentUri(
			string docId, 
			string revision = null,
			AdditionalDocumentProperty additionalProperties = default(AdditionalDocumentProperty))
		{
			var documentUriString = GetDocumentUriString(docId, revision, additionalProperties);
			return new Uri(databaseUriWithSlash, documentUriString).LeaveDotsAndSlashesEscaped();
		}

		[Pure]
		public string GetDocumentUriString(
			string documentId, 
			string revision, 
			AdditionalDocumentProperty additionalProperties = default(AdditionalDocumentProperty))
		{
			var uriStringBuilder = new StringBuilder();
			AppendDocId(uriStringBuilder, documentId);

			var delimiter = '?';
			foreach (var documentQueryParam in GetDocumentQueryParams(revision, additionalProperties))
			{
				uriStringBuilder.Append(delimiter);
				if(delimiter == '?')
					delimiter = '&';
				uriStringBuilder.Append(documentQueryParam.Key).Append('=').Append(documentQueryParam.Value);
			}

			return uriStringBuilder.ToString();
		}

		[Pure]
		public Uri GetFullAttachmentUri(string attachmentId, string docId, string revision = null)
		{
			var attachmentUriString = GetAttachmentUriString(attachmentId, docId, revision);
			return new Uri(databaseUriWithSlash, attachmentUriString).LeaveDotsAndSlashesEscaped();
		}

		[Pure]
		private static string GetAttachmentUriString(string attachmentId, string docId, string revision = null)
		{
			var uriStringBuilder = new StringBuilder();
			AppendDocId(uriStringBuilder, docId);
			uriStringBuilder.Append("/");
			uriStringBuilder.Append(Uri.EscapeDataString(attachmentId));
			AppendRevisionIfNeeded(uriStringBuilder, revision);
			return uriStringBuilder.ToString();
		}

		private static IEnumerable<KeyValuePair<string, string>> GetDocumentQueryParams(
			string revision, AdditionalDocumentProperty additionalProperties)
		{
			if (!string.IsNullOrEmpty(revision))
				yield return new KeyValuePair<string, string>("rev", revision);

			if(additionalProperties == default(AdditionalDocumentProperty))
				yield break;
			
			if((additionalProperties & AdditionalDocumentProperty.Attachments) == AdditionalDocumentProperty.Attachments)
				yield return new KeyValuePair<string, string>("attachments", "true");

			if ((additionalProperties & AdditionalDocumentProperty.Conflicts) == AdditionalDocumentProperty.Conflicts)
				yield return new KeyValuePair<string, string>("conflicts", "true");

			if ((additionalProperties & AdditionalDocumentProperty.DeletedConflicts) == AdditionalDocumentProperty.DeletedConflicts)
				yield return new KeyValuePair<string, string>("deleted_conflicts", "true");

			if ((additionalProperties & AdditionalDocumentProperty.RevisionHistory) == AdditionalDocumentProperty.RevisionHistory)
				yield return new KeyValuePair<string, string>("revs", "true");

			if ((additionalProperties & AdditionalDocumentProperty.RevisionInfo) == AdditionalDocumentProperty.RevisionInfo)
				yield return new KeyValuePair<string, string>("revs_info", "true");
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
				uriStringBuilder.Append(Uri.EscapeDataString(docId.Substring(designDocumentPrefix.Length)));
			}
			else
				uriStringBuilder.Append(Uri.EscapeDataString(docId));
		}

		private static void AppendReplacingFowardSlash(StringBuilder stringBuilder, string stringToAppend)
		{
			var originalLength = stringBuilder.Length;
			stringBuilder.Append(stringToAppend);
			stringBuilder.Replace("/", "%2F", originalLength, stringToAppend.Length);
		}
	}
}