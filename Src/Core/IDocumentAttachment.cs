#region Licence Info 
/*
	Copyright 2011 � Artem Tikhomirov																					
																																					
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

using System.IO;
using System.Threading.Tasks;

namespace CouchDude
{
	/// <summary>Describes document attachment</summary>
	public interface IDocumentAttachment: IJsonFragment
	{
		/// <summary>Name of the attachment. Could not be changed - create new attachment insted.</summary>
		string Id { get; }

		/// <summary>Attachment content type.</summary>
		string ContentType { get; set; }

		/// <summary>Indicates whether attachment data. is loaded as part of document (base64 encoded).</summary>
		bool Inline { get; }

		/// <summary>Length of the </summary>
		int Length { get; set; }

		/// <summary>Sets attachment's inline data.</summary>
		void SetData(Stream dataStream);

		/// <summary>Opens attachment for read.</summary>
		/// <remarks>This is remote call if <see cref="Inline"/> is false.</remarks>
		Task<Stream> OpenRead();

		/// <summary>Returns syncronous version of the async attachment method.</summary>
		ISyncronousDocumentAttachment Syncronously { get; }
	}
}