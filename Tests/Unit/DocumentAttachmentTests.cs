using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CouchDude.Api;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit
{
	public class DocumentAttachmentTests
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
					_attachments = new { attachment1 = new { stub = true, length = 42 }}
				}.ToJsonObject());

			Assert.Equal(42, document.Attachments["attachment1"].Length);
		}

		[Theory]
		[InlineData("aGVsbG8=", 5)]
		[InlineData("aGVsbG8s", 6)]
		[InlineData("aGVsbG8sdw==", 7)]
		[InlineData("aGVsbG8sd28=", 8)]
		public void ShouldReadAttachmentLengthFromDataIfInline(string base64String, int expectedLength)
		{
			var document = new Document(
				new {
					_id = "doc1",
					_attachments = new { attachment1 = new { data = base64String }}
				}.ToJsonObject());

			Assert.Equal(expectedLength, document.Attachments["attachment1"].Length);
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
					TaskUtils.FromResult(
						new HttpResponseMessageAttachment("attachment1", new HttpResponseMessage(HttpStatusCode.OK) {
							Content = new StringContent("test", Encoding.UTF8, "text/plain")
						}) as Attachment
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
	}
}