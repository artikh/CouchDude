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
using CouchDude.Api;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class DocumentAttachmentTests
	{
		[Fact]
		public void ShouldReadContentType() 
		{
			IDocumentAttachment attachment = new DocumentAttachment("attachment1", new { content_type = "application/json" }.ToJObject());
			Assert.Equal("application/json", attachment.ContentType);
		}

		[Fact]
		public void ShouldWriteContentType() 
		{
			var jObject = new { content_type = "application/json" }.ToJObject();
			IDocumentAttachment attachment = new DocumentAttachment("attachment1", jObject);
			attachment.ContentType = "text/plain";

			Assert.Equal("text/plain", jObject.Value<string>("content_type"));
		}

		[Fact]
		public void ShouldReadNegativeStubFlag() 
		{
			IDocumentAttachment attachment = new DocumentAttachment("attachment1", new { stub = false }.ToJObject());
			Assert.True(attachment.Inline);
		}

		[Fact]
		public void ShouldReadAbsentStubFlag() 
		{
			IDocumentAttachment attachment = new DocumentAttachment("attachment1", new {  }.ToJObject());
			Assert.True(attachment.Inline);
		}

		[Fact]
		public void ShouldReadPositiveStubFlag() 
		{
			IDocumentAttachment attachment = new DocumentAttachment("attachment1", new { stub = true }.ToJObject());
			Assert.False(attachment.Inline);
		}

		[Fact]
		public void ShouldWriteStubFlag() 
		{
			var jObject = new JObject();
			IDocumentAttachment attachment = new DocumentAttachment("attachment1", jObject);
			attachment.Inline = true;

			Assert.False(jObject.Value<bool>("stub"));
		}

		[Fact]
		public void ShouldWriteData() 
		{
			var jObject = new JObject();
			IDocumentAttachment attachment = new DocumentAttachment("attachment1", jObject);

			attachment.InlineData = Encoding.UTF8.GetBytes("test test!");

			Assert.Equal("dGVzdCB0ZXN0IQ==", jObject.Value<string>("data"));
		}

		[Fact]
		public void ShouldMakeAttachmentInlineOnFirstWrite() 
		{
			IDocumentAttachment attachment = new DocumentAttachment("attachment1");
			attachment.InlineData = new byte[] { 42 };
			Assert.True(attachment.Inline);
		}

		[Fact]
		public void ShouldThrowOnIncorrectConstructorParameters() 
		{
			Assert.Throws<ArgumentNullException>(() => new DocumentAttachment(null));
			Assert.Throws<ArgumentNullException>(() => new DocumentAttachment(string.Empty));
			Assert.Throws<ParseException>(() => new DocumentAttachment("attachment1", "{{{ some incorrect JSON"));
			Assert.Throws<ParseException>(() => new DocumentAttachment("attachment1", "[\"correct JSON, but array\"]"));
			Assert.Throws<ParseException>(() => new DocumentAttachment("attachment1", "\"correct JSON, but string\""));
			Assert.Throws<ParseException>(() => new DocumentAttachment("attachment1", "42"));
		}

		[Fact]
		public void ShouldReadData()
		{
			IDocumentAttachment attachment = new DocumentAttachment("attachment1", new { data = "dGVzdCB0ZXN0IQ==" }.ToJObject());
			var stringData = Encoding.UTF8.GetString(attachment.InlineData);
			Assert.Equal("test test!", stringData);
		}
	}
}