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

using CouchDude.Api;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class AttachmentBagExtentionsTests
	{
		[Fact]
		public void ShouldAddInlineByteArrayAttachment()
		{
			var jObject = new JObject();
			var document = new Document(jObject);
			document.DocumentAttachments.AddInline("attachment1", new byte[] { 42, 42 }, contentType: "application/binary");
			
			var attachment = jObject["_attachments"]["attachment1"];
			
			Assert.NotNull(attachment);
			Assert.Equal("application/binary", attachment.Value<string>("content_type"));
			Assert.Null(attachment["stub"]);
			Assert.Equal(2, attachment.Value<int>("length"));
			Assert.Equal("Kio=", attachment.Value<string>("data"));
		}

		[Fact]
		public void ShouldSetApplicationOctetStreamByDefaultWhenCreatingByteArrayInlineAttachment() 
		{
			var document = new Document();
			document.DocumentAttachments.AddInline("attachment1", new byte[] { 42, 42 });

			Assert.Equal("application/octet-stream", document.DocumentAttachments["attachment1"].ContentType);
		}

		[Fact]
		public void ShouldAddInlineStringAttachment()
		{
			var jObject = new JObject();
			var document = new Document(jObject);
			document.DocumentAttachments.AddInline("attachment1", "test тест!", contentType: "text/test");
			
			var attachment = jObject["_attachments"]["attachment1"];
			
			Assert.NotNull(attachment);
			Assert.Equal("text/test", attachment.Value<string>("content_type"));
			Assert.Null(attachment["stub"]);
			Assert.Equal(14, attachment.Value<int>("length"));
			Assert.Equal("dGVzdCDRgtC10YHRgiE=", attachment.Value<string>("data"));
		}

		[Fact]
		public void ShouldSetTextPlainByDefaultWhenCreatingStringInlineAttachment() 
		{
			var document = new Document();
			document.DocumentAttachments.AddInline("attachment1", "test");

			Assert.Equal("text/plain", document.DocumentAttachments["attachment1"].ContentType);
		}
	}
}