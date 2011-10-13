#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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

using CouchDude.Utils;

namespace CouchDude
{
	/// <summary>Document ID and revision. </summary>
	public struct DocumentInfo
	{
		/// <summary>Document ID.</summary>
		public readonly string Id;

		/// <summary>Document revision.</summary>
		public readonly string Revision;

		/// <constructor />
		public DocumentInfo(string id, string revision)
		{
			if (id.HasNoValue()) throw new ArgumentNullException("id");
			if (revision.HasNoValue()) throw new ArgumentNullException("id");
			
			Id = id;
			Revision = revision;
		}

		/// <inheritdoc/>
		public bool Equals(DocumentInfo other)
		{
			return Equals(other.Id, Id) && Equals(other.Revision, Revision);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof (DocumentInfo)) return false;
			return Equals((DocumentInfo) obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				return (Id.GetHashCode()*397) ^ Revision.GetHashCode();
			}
		}
	}
}