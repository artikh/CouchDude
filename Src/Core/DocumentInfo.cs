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