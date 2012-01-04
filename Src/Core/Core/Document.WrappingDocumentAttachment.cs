using System;
using System.IO;
using System.Json;
using System.Threading.Tasks;
using CouchDude.Utils;
using JetBrains.Annotations;

namespace CouchDude
{
	public partial class Document
	{
		/// <summary>CouchDB attachment backed by part of document JSON.</summary>
		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		public class WrappingDocumentAttachment : DocumentAttachment
		{
			/// <summary>Inline data property name.</summary>
			protected const string DataPropertyName = "data";

			/// <summary>Inline indicator property name.</summary>
			protected const string StubPropertyName = "stub";

			/// <summary>Content type property name.</summary>
			protected const string ContentTypePropertyName = "content_type";

			/// <summary>Length property name.</summary>
			protected const string LengthPropertyName = "length";

			static readonly byte[] EmptyBuffer = new byte[0];
			readonly Document parentDocument;

			/// <summary>Creates attachment wrapping existing attachment 
			/// descriptor (probably loaded from CouchDB)</summary>
			protected internal WrappingDocumentAttachment(string id, Document parentDocument)
				: base(id)
			{
				if (id.HasNoValue()) throw new ArgumentNullException("id");
				if (parentDocument == null) throw new ArgumentNullException("parentDocument");

				this.parentDocument = parentDocument;
			}

			JsonObject AttachmentDescriptor
			{
				get
				{
					return parentDocument
						.RawJsonObject
						.GetOrCreateObjectProperty(AttachmentsPropertyName)
						.GetOrCreateObjectProperty(Id);
				}
			}

			/// <summary>Attachment content (MIME) type.</summary>
			public override string ContentType { get { return AttachmentDescriptor.GetPrimitiveProperty<string>(ContentTypePropertyName); } set { AttachmentDescriptor[ContentTypePropertyName] = value; } }

			/// <summary>Content length.</summary>
			public override long Length { get { return AttachmentDescriptor.GetPrimitiveProperty<int>(LengthPropertyName); } }

			/// <summary>Indicates wether attachment is included as base64 string within document or should 
			/// be requested separatly.</summary>
			public override bool Inline
			{
				get { return !AttachmentDescriptor.GetPrimitiveProperty(StubPropertyName, defaultValue: false); }
				set
				{
					if (value)
						AttachmentDescriptor.Remove(StubPropertyName);
					else
						AttachmentDescriptor[StubPropertyName] = true;
				}
			}

			/// <summary>Open attachment data stream for read.</summary>
			public override Task<Stream> OpenRead()
			{
				if (Inline)
				{
					var base64String = AttachmentDescriptor.GetPrimitiveProperty<string>(DataPropertyName);
					var inlineData = base64String.HasNoValue()
						? EmptyBuffer
						: Convert.FromBase64String(base64String);
					Stream inlineDataStream = new MemoryStream(inlineData);
					return TaskEx.FromResult(inlineDataStream);
				}
				else
				{
					var attachmentId = Id;
					var documentId = parentDocument.Id;
					var documentRevision = parentDocument.Revision;

					var databaseApi = parentDocument
						.DatabaseApiReference
						.GetOrThrowIfUnavaliable(
							operation: () =>
								string.Format(
									"load attachment {0} from document {1}(rev:{2})", attachmentId, documentId,
									documentRevision)
						);

					return databaseApi
						.RequestAttachment(attachmentId, documentId, documentRevision)
						.ContinueWith(
							requestAttachmentTask => {
								var recivedAttachment = requestAttachmentTask.Result;
								AttachmentDescriptor[LengthPropertyName] = recivedAttachment.Length;
								ContentType = recivedAttachment.ContentType;
								Inline = recivedAttachment.Inline;
								return recivedAttachment.OpenRead();
							}
						)
						.Unwrap();
				}
			}

			/// <summary>Converts sets attachment data (inline). Attachment gets saved with parent document.</summary>
			public override void SetData(Stream dataStream)
			{
				if (dataStream == null) throw new ArgumentNullException("dataStream");
				if (!dataStream.CanRead)
					throw new ArgumentOutOfRangeException("dataStream", dataStream, "Stream should be readable");

				Inline = true;

				// TODO: we should not have intermediate byte array here
				using (dataStream)
				using (var memoryStream = new MemoryStream())
				{
					dataStream.CopyTo(memoryStream);
					var base64String = Convert.ToBase64String(
						memoryStream.GetBuffer(), offset: 0, length: (int) memoryStream.Length);
					AttachmentDescriptor[DataPropertyName] = base64String;
				}
			}
		}
	}
}