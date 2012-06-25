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
		public class WrappingAttachment : Attachment
		{
			/// <summary><see cref="WrappingAttachment"/> data storage options.</summary>
			public enum DataStorage
			{
				/// <summary>Attachment data stored in database and should be requested separatly.</summary>
				InDatabase,
				/// <summary>Data included as base64 string within document.</summary>
				Inline,
				/// <summary>Data is stored elseware. Data getter should be set prior setting </summary>
				InMultipart
			}

			/// <summary>Inline data property name.</summary>
			protected const string DataPropertyName = "data";

			/// <summary>Inline indicator property name.</summary>
			protected const string StubPropertyName = "stub";

			/// <summary>Content type property name.</summary>
			protected const string ContentTypePropertyName = "content_type";

			/// <summary>Length property name.</summary>
			protected const string LengthPropertyName = "length";

			const string FollowsPropertyName = "follows";

			static readonly byte[] EmptyBuffer = new byte[0];
			readonly Document parentDocument;
			Func<Task<Stream>> dataGetter;

			/// <summary>Creates attachment wrapping existing attachment 
			/// descriptor (probably loaded from CouchDB)</summary>
			protected internal WrappingAttachment(string id, Document parentDocument)
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
			public override string ContentType
			{
				get { return AttachmentDescriptor.GetPrimitiveProperty<string>(ContentTypePropertyName) ?? DefaultContentType; } 
				set { AttachmentDescriptor[ContentTypePropertyName] = value; }
			}

			/// <summary>Content length.</summary>
			public override long Length
			{
				get
				{
					var data = AttachmentDescriptor.GetPrimitiveProperty<string>(DataPropertyName);
					if(data != null)
					{
						var baseLength = 3*(data.Length/4);
						if (baseLength == 0)
							return 0;
						if (data[data.Length - 2] == '=')
							return baseLength - 2;
						if (data[data.Length - 1] == '=')
							return baseLength - 1;
						return baseLength;
					}

					return AttachmentDescriptor.GetPrimitiveProperty<int>(LengthPropertyName);
				}
			}

			/// <summary>Indicates where attachment data is stored.</summary>
			public DataStorage Storage
			{
				get
				{
					var follows = AttachmentDescriptor.GetPrimitiveProperty<bool>(FollowsPropertyName);
					var stub = AttachmentDescriptor.GetPrimitiveProperty<bool>(StubPropertyName);

					if (follows)
						return DataStorage.InMultipart;
					if (stub)
						return DataStorage.InDatabase;
					return DataStorage.Inline;
				}
				set
				{
					switch (value)
					{
						case DataStorage.InDatabase:
							AttachmentDescriptor.Remove(FollowsPropertyName);
							AttachmentDescriptor[StubPropertyName] = true;
							break;
						case DataStorage.Inline:
							AttachmentDescriptor.Remove(FollowsPropertyName);
							AttachmentDescriptor.Remove(StubPropertyName);
							break;
						case DataStorage.InMultipart:
							AttachmentDescriptor[FollowsPropertyName] = true;
							AttachmentDescriptor.Remove(StubPropertyName);
							break;
						default:
							throw new ArgumentOutOfRangeException("value");
					}
				}
			}

			/// <summary>Open attachment data stream for read.</summary>
			public override Task<Stream> OpenRead()
			{
				switch (Storage)
				{
					case DataStorage.Inline:
						return OpenInlineData();
					case DataStorage.InDatabase:
						return OpenDataFromDatabase(Id, parentDocument.Id, parentDocument.Revision);
					case DataStorage.InMultipart:
						if (dataGetter == null)
							throw new InvalidOperationException("Attachment data storage is set to 'InMultipart', but it lacks data getter");
						return dataGetter();
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			Task<Stream> OpenInlineData()
			{
				var base64String = AttachmentDescriptor.GetPrimitiveProperty<string>(DataPropertyName);
				return Task.Factory.StartNew(
					() => {
						var inlineData = base64String.HasNoValue()? EmptyBuffer: Convert.FromBase64String(base64String);
						return new MemoryStream(inlineData) as Stream;
					});
			}

			async Task<Stream> OpenDataFromDatabase(string attachmentId, string documentId, string documentRevision)
			{
				var databaseApi = parentDocument
					.DatabaseApiReference
					.GetOrThrowIfUnavaliable(operationName: () => string.Format("load attachment {0} from document {1}(rev:{2})", attachmentId, documentId, documentRevision));

				var recivedAttachment = 
					await databaseApi.RequestAttachment(attachmentId, documentId, documentRevision).ConfigureAwait(false);

				AttachmentDescriptor[LengthPropertyName] = recivedAttachment.Length;
				ContentType = recivedAttachment.ContentType;
				return await recivedAttachment.OpenRead().ConfigureAwait(false);
			}

			/// <summary>Converts sets attachment data (inline). Attachment gets saved with parent document.</summary>
			public override void SetData(Stream dataStream)
			{
				if (dataStream == null) throw new ArgumentNullException("dataStream");
				if (!dataStream.CanRead)
					throw new ArgumentOutOfRangeException("dataStream", dataStream, "Stream should be readable");

				Storage = DataStorage.Inline;

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

			/// <summary>Sets implict data getter multipart loading data.</summary>
			public void SetDataGetter(Func<Task<Stream>> dataGetter) { this.dataGetter = dataGetter; }
		}
	}
}