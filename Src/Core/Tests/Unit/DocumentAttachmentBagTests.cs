using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace CouchDude.Tests.Unit
{
	public class DocumentAttachmentBagTests
	{
		[Fact]
		public void ShouldCreateNewTextInlineAttachment()
		{
			TestCreation(b => b.Create("attachment1", "<h1>test</h1>", "text/html"));
		}

		[Fact]
		public void ShouldCreateNewByteInlineAttachment() 
		{
			TestCreation(b => b.Create("attachment1", Encoding.UTF8.GetBytes("<h1>test</h1>"), "text/html"));
		}

		[Fact]
		public void ShouldCreateNewStreamInlineAttachment()
		{
			TestCreation(b => b.Create("attachment1", new MemoryStream(Encoding.UTF8.GetBytes("<h1>test</h1>")), "text/html"));
		}

		[Fact]
		public void ShouldThrowInvalidOperationExceptionOnDuplicateAttachmentCreation() 
		{
			var doc = new Document();
			Assert.DoesNotThrow(() => doc.DocumentAttachments.Create("attachment1", "test"));
			Assert.Throws<InvalidOperationException>(() => doc.DocumentAttachments.Create("attachment1", "test"));
		}
		
		[Fact]
		public void ShouldEnumerateAttachments()
		{
			var doc = new Document(new {
				_id = "doc1", 
				_attachments = 
					new {
						attachment1 = new { content_type = "text/plain", stub = true },
						attachment2 = new { content_type = "text/html", stub = true },
						attachment3 = new { content_type = "application/xml", stub = true }
					}
			}.ToJsonObject());

			// ReSharper disable RedundantEnumerableCastCall
			var attachments = doc.DocumentAttachments.OfType<DocumentAttachment>().ToArray();
			// ReSharper restore RedundantEnumerableCastCall
			Assert.Equal(3, attachments.Length);
			Assert.Equal("text/plain", attachments[0].ContentType);
			Assert.False(attachments[0].Inline);
			Assert.Equal("text/html", attachments[1].ContentType);
			Assert.False(attachments[1].Inline);
			Assert.Equal("application/xml", attachments[2].ContentType);
			Assert.False(attachments[2].Inline);
		}

		[Fact]
		public void ShouldEnumerateEmptyAttachmentsCollection() 
		{
			Assert.Equal(0, new Document().DocumentAttachments.ToArray().Length);
		}

		[Fact]
		public void ShouldDeleteOriginalAttachmentDescription() 
		{
			var doc = new Document(new { _id = "doc1", _attachments = new { attachment1 = new { } } }.ToJsonObject());
			doc.DocumentAttachments.Delete("attachment1");
			Assert.False(doc.RawJsonObject["_attachments"].ContainsKey("attachment1"));
		}

		[Fact]
		public void ShouldThrowKeyNotFoundOnWhenDeletingNoneExistingAttachment() 
		{
			var doc = new Document(new { _id = "doc1", _attachments = new { attachment1 = new { } } }.ToJsonObject());
			Assert.Throws<KeyNotFoundException>(() => doc.DocumentAttachments.Delete("attachment2"));
		}

		[Fact]
		public void ShouldCreateSameObjectFromIndexProperty() 
		{
			var doc = new Document();
			Assert.Same(doc.DocumentAttachments["attachment1"], doc.DocumentAttachments["attachment1"]);
		}

		[Fact]
		public void ShouldThrowArgumentNullExceptionOnIndexProperty()
		{
			var doc = new Document();
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments[null]);
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments[""]);
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments["		"]);
		}

		[Fact]
		public void ShouldThrowArgumentNullExceptionOnDeleteMethod()
		{
			var doc = new Document();
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Delete(null));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Delete(""));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Delete("		"));
		}

		[Fact]
		public void ShouldThrowArgumentNullExceptionOnCreationMethods() 
		{
			var doc = new Document();
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create("", "test"));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create("		", "test"));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create(null, "test"));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create("attachment1", (string)null));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create("", new byte[0]));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create("		", new byte[0]));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create(null, new byte[0]));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create("attachment1", (byte[])null));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create("", new MemoryStream()));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create("		", new MemoryStream()));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create(null, new MemoryStream()));
			Assert.Throws<ArgumentNullException>(() => doc.DocumentAttachments.Create("attachment1", (Stream)null));

		}

		static void TestCreation(Func<DocumentAttachmentBag, DocumentAttachment> createAction)
		{
			var document = new Document(new {_id = "doc1"}.ToJsonObject());

			var createdAttachment = createAction(document.DocumentAttachments);

			var newAttachment = document.DocumentAttachments["attachment1"];

			Assert.Same(createdAttachment, newAttachment);
			Assert.Equal("attachment1", newAttachment.Id);
			Assert.Equal("<h1>test</h1>", newAttachment.Syncronously.OpenRead().ReadAsUtf8String());
			Assert.Equal("text/html", newAttachment.ContentType);
		}
	}
}