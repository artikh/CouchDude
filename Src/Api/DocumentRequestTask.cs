using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDude.Utils;

namespace CouchDude.Api
{
	static class DocumentRequestTask
	{
		public static async Task<Document> Start(
			DbUriConstructor uriConstructor,
			DatabaseApi databaseApi,
			CouchApi couchApi,
			string documentId, 
			string revision, 
			AdditionalDocumentProperty additionalProperties)
		{
			var documentUri = uriConstructor.GetFullDocumentUri(documentId, revision, additionalProperties);
			var request = new HttpRequestMessage(HttpMethod.Get, documentUri);

			var response = await couchApi.RequestCouchDb(request).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(couchApi.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				if (response.StatusCode == HttpStatusCode.NotFound)
					return null;
				error.ThrowCouchCommunicationException();
			}

			var content = response.Content;
			var mediaType = content.Headers.ContentType != null? content.Headers.ContentType.MediaType: "<unknown>";
			switch (mediaType)
			{
				case MediaType.Json:
					return ReadDocument(databaseApi, await content.ReadAsUtf8TextReaderAsync());
				case MediaType.Multipart:
					return await ReadMultipart(databaseApi, content);
				default:
					throw new CouchCommunicationException(
						"Unexpected media type response recived requesting CouchDB document: {0}", mediaType);
			}
		}

		static async Task<Document> ReadMultipart(DatabaseApi couchApi, HttpContent content)
		{
			var multipart = (await content.ReadAsMultipartAsync().ConfigureAwait(false)).ToArray();
			var jsonPart = multipart.FirstOrDefault(
				part => part.Headers.ContentType != null && part.Headers.ContentType.MediaType == MediaType.Json);
			if (jsonPart == null)
				return null;

			var document = ReadDocument(couchApi, await jsonPart.ReadAsUtf8TextReaderAsync());
			PrefillAttachmentDataGetters(multipart, document);
			return document;
		}

		static void PrefillAttachmentDataGetters(IEnumerable<HttpContent> multipart, Document document)
		{
			var noneJsonParts =
				from part in multipart
				where part.Headers.ContentType == null || part.Headers.ContentType.MediaType != MediaType.Json
				select part;

			var followedUpAttachmentsAndParts = document.Attachments
				.OfType<Document.WrappingAttachment>()
				.Where(a => a.Storage == Document.WrappingAttachment.DataStorage.InMultipart)
				.Zip(noneJsonParts, (attachment, part) => new {attachment, part});

			foreach (var pair in followedUpAttachmentsAndParts)
			{
				var part = pair.part;
				pair.attachment.SetDataGetter(part.ReadAsStreamAsync);
			}
		}

		static Document ReadDocument(DatabaseApi couchApi, TextReader reader)
		{
			using (reader)
				return new Document(reader) { DatabaseApiReference = new DatabaseApiReference(couchApi) };
		}

	}
}