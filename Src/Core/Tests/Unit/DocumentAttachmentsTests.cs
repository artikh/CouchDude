using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CouchDude.Api;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit
{
	public class DocumentAttachmentsTests
	{
		[Fact]
		public void ShouldProjectAttachmentId()
		{
			var document = new Document(
				new {
					_id = "doc1",
					_attachments = new { attachment1 = new { }}
				}.ToJsonObject());

			Assert.Equal("attachment1", document.Attachments["attachment1"].Id);
		}

		[Fact]
		public void ShouldProjectAttachmentLength()
		{
			var document = new Document(
				new {
					_id = "doc1",
					_attachments = new { attachment1 = new { length = 42 }}
				}.ToJsonObject());

			Assert.Equal(42, document.Attachments["attachment1"].Length);
		}

		[Fact]
		public void ShouldProjectAttachmentContentType()
		{
			var document = new Document(
				new {
					_id = "doc1",
					_attachments = new { attachment1 = new { content_type = "text/plain" }}
				}.ToJsonObject());

			Assert.Equal("text/plain", document.Attachments["attachment1"].ContentType);
		}

		[Fact]
		public void ShouldProjectAttachmentStubIndicator()
		{
			var document = new Document(
				new {
					_id = "doc1",
					_attachments = new { attachment1 = new { stub = true }}
				}.ToJsonObject());

			Assert.False(document.Attachments["attachment1"].Inline);
		}

		[Fact]
		public void ShouldTreatAttachmentAsInlineIfThereAreNoStub()
		{
			var document = new Document(
				new {
					_id = "doc1",
					_attachments = new { attachment1 = new { }}
				}.ToJsonObject());

			Assert.True(document.Attachments["attachment1"].Inline);
		}

		[Fact]
		public void ShouldRetriveDataFromInlineAttachment()
		{
			var document = new Document(
				new {
					_id = "doc1",
					_attachments = new {
						attachment1 = new { data = "VGhlcmUgaXMgYSB0aGVvcnkgd2hpY2ggc3RhdGVz", content_type = "text/plain" }
					}
				}.ToJsonObject());

			Assert.Equal(
				"There is a theory which states", 
				document.Attachments["attachment1"].Syncronously.OpenRead().ReadAsUtf8String());
		}

		[Fact]
		public void ShouldRequestDataFromDocumentApiForStubAttachment()
		{
			var document = new Document(
				new {
					_id = "doc1",
					_rev = "rev1",
					_attachments = new {
						attachment1 = new { stub = true, content_type = "text/plain" }
					}
				}.ToJsonObject());


			var databaseApi = Mock.Of<IDatabaseApi>(
				a => a.RequestAttachment("attachment1", "doc1", "rev1") == 
					TaskEx.FromResult(
						new HttpResponseMessageDocumentAttachment("attachment1", new HttpResponseMessage(HttpStatusCode.OK) {
							Content = new StringContent("test", Encoding.UTF8, "text/plain")
						}) as DocumentAttachment
					)
				);

			document.DatabaseApiReference = new DatabaseApiReference(databaseApi);

			Assert.Equal("test", document.Attachments["attachment1"].Syncronously.OpenRead().ReadAsUtf8String());
		}
		
		[Fact]
		public void ShouldThrowLazyLoadingExceptionOnDetouchedNoneInlineDocument()
		{
			var document = new Document(
				new { 
					_id = "doc1", 
					_attachments = new { attachment1 = new { stub = true, content_type = "text/plain" } }
				}.ToJsonObject());

			Assert.Throws<LazyLoadingException>(() => document.Attachments["attachment1"].Syncronously.OpenRead());
		}

		[Fact]
		public void ShouldSetStubPropertyUsingInline()
		{
			var docJson = new { _id = "doc1", _attachments = new { a1 = new { } } }.ToJsonObject();
			var document = new Document(docJson);

			document.Attachments["a1"].Inline = false;
			Assert.Equal(true, (bool)docJson.AsDynamic()._attachments.a1.stub);

			document.Attachments["a1"].Inline = true;
			Assert.False(docJson["_attachments"]["a1"].ContainsKey("stub"));
		}
	}
}