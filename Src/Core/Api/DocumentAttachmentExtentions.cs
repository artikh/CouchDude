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

namespace CouchDude.Api
{
	/// <summary>Convinience methods for <see cref="WrappingDocumentAttachment"/>.</summary>
	public static class DocumentAttachmentExtentions
	{
		/// <summary>Returns attachment's data as string </summary>
		public static string ReadAsString(this IDocumentAttachment self, Encoding encoding = null)
		{
			if (self == null) throw new ArgumentNullException("self");
			if (!self.Inline)
				throw new ArgumentOutOfRangeException(
					"self", self, "Attachment should be inline to use this attachment");

			return (encoding ?? Encoding.UTF8).GetString(self.InlineData);
		}
	}
}