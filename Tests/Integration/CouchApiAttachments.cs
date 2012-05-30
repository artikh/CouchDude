using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class CouchApiAttachments
	{
		readonly string documentId = Guid.NewGuid().ToString();
		readonly string attachmentId = Guid.NewGuid().ToString();
		string lastKnownRevision;
		byte[] attachmentDataBuffer;
		readonly IDatabaseApi dbApi = Factory.CreateCouchApi("http://127.0.0.1:5984/").Db("testdb");

		public CouchApiAttachments()
		{
			GenerateNewAttachmentDataBuffer();
			dbApi.Synchronously.Create(throwIfExists: false);
		}

		[Fact]
		public void ShouldCreateRequestRequestAsInlineAndDeleteDocumentAttachment()
		{
			SaveNewAttachmentToNonexistingDocument();
			LoadAttachmentDirectly();
			LoadAttachmentAsAPartOfTheDocument();
			LoadAttachmentAsAInlinePartOfTheDocument();
			UpdateAttachment();
			LoadAttachmentDirectly();
			LoadAttachmentAsAPartOfTheDocument();
			LoadAttachmentAsAInlinePartOfTheDocument();
			DeleteAttachment();
			CheckIfAttachmentHaveBeenDeleted();
		}

		void SaveNewAttachmentToNonexistingDocument()
		{
			var attachment = new Attachment(attachmentId);
			attachment.SetData(attachmentDataBuffer);
			attachment.ContentType = "application/x-file";
			var docInfo = dbApi.Synchronously.SaveAttachment(attachment, documentId);
			lastKnownRevision = docInfo.Revision;
		}

		void LoadAttachmentDirectly()
		{
			var attachment = dbApi.Synchronously.RequestAttachment(attachmentId, documentId);
			CheckAttachment(attachment);
		}

		void LoadAttachmentAsAPartOfTheDocument() 
		{
			var document = dbApi.Synchronously.RequestDocument(documentId);
			Assert.Equal(lastKnownRevision, document.Revision);
			CheckAttachment(document.Attachments[attachmentId]);
		}

		void LoadAttachmentAsAInlinePartOfTheDocument() 
		{
			var document = dbApi.Synchronously.RequestDocument(
				documentId, additionalProperties: AdditionalDocumentProperty.Attachments);
			Assert.Equal(lastKnownRevision, document.Revision);
			CheckAttachment(document.Attachments[attachmentId]);
		}

		void UpdateAttachment()
		{
			GenerateNewAttachmentDataBuffer();
			var attachment = new Attachment(attachmentId);
			attachment.SetData(attachmentDataBuffer);
			attachment.ContentType = "application/x-file";
			var docInfo = dbApi.Synchronously.SaveAttachment(attachment, documentId, lastKnownRevision);
			lastKnownRevision = docInfo.Revision;
		}

		void CheckAttachment(Attachment attachment)
		{
			Assert.Equal("application/x-file", attachment.ContentType);
			Assert.Equal(attachmentId, attachment.Id);
			Assert.Equal(attachmentDataBuffer.Length, attachment.Length);
			Assert.Equal(attachmentDataBuffer, ReadStreamToBuffer(attachment.Syncronously.OpenRead()));
		}

		static byte[] ReadStreamToBuffer(Stream stream)
		{
			using (stream)
			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				memoryStream.Flush();
				return memoryStream.ToArray();
			}
		}

		void GenerateNewAttachmentDataBuffer()
		{
			attachmentDataBuffer = new byte[1024];
			var randomGenerator = System.Security.Cryptography.RandomNumberGenerator.Create();
			randomGenerator.GetBytes(attachmentDataBuffer);
		}
		
		void DeleteAttachment()
		{
			dbApi.Synchronously.DeleteAttachment(attachmentId, documentId, lastKnownRevision);
		}

		void CheckIfAttachmentHaveBeenDeleted()
		{
			var attachment = dbApi.Synchronously.RequestAttachment(attachmentId, documentId);
			Assert.Null(attachment);
		}
	}
}