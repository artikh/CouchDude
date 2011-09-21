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
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class DocumentAttachmentExtentionsTests
	{
		[Fact]
		public void ShouldReadInlineAttachmentAsString()
		{
			var attachment = new DocumentAttachment(
				"attachment1", new {data = "dGVzdCDRgtC10YHRgiE=", length = 14}.ToJsonString());

			Assert.Equal("test тест!", attachment.ReadAsString());
		}

		[Fact]
		public void ShouldThrowOnAttemtToReadNoneInlineAttachment() 
		{
			var attachment = new DocumentAttachment("attachment1") {Inline = false};
			Assert.Throws<ArgumentOutOfRangeException>(() => attachment.ReadAsString());
		}

		[Fact]
		public void ShouldThrowOnNullAttachment()
		{
			DocumentAttachment attachment = null;
			// ReSharper disable ConditionIsAlwaysTrueOrFalse
			Assert.Throws<ArgumentNullException>(() => attachment.ReadAsString());
			// ReSharper restore ConditionIsAlwaysTrueOrFalse
		}
	}
}
