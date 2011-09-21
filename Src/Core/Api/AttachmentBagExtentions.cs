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

using System.Text;
using CouchDude.Utils;

namespace CouchDude.Api
{
	/// <summary>Convinience methods for <see cref="IAttachmentBag"/>.</summary>
	public static class AttachmentBagExtentions
	{
		/// <summary>Adds inline attachment to the document's attachments collection.</summary>
		public static void AddInline(this IAttachmentBag self, string name, byte[] data, string contentType = null)
		{
			var attachment = new DocumentAttachment(name) {InlineData = data};
			if (contentType.HasValue())
				attachment.ContentType = contentType;
			attachment.Length = data.Length;
			self.Add(attachment);
		}

		/// <summary>Adds inline attachment to the document's attachments collection.</summary>
		public static void AddInline(
			this IAttachmentBag self, string name, string dataString, Encoding encoding = null, string contentType = null)
		{
			encoding = encoding ?? Encoding.UTF8;
			self.AddInline(name, encoding.GetBytes(dataString), contentType ?? "text/plain");
		}
	}
}