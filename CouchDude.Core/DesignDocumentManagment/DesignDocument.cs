using System;
using System.Diagnostics.Contracts;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.DesignDocumentManagment
{
	/// <summary>Design document descriptor.</summary>
	public class DesignDocument: IEquatable<DesignDocument>
	{
		/// <summary>Design ID prefix.</summary>
		public const string IdPrefix = "_design/";

		/// <summary>Document ID standard property name.</summary>
		public const string IdPropertyName = "_id";
		
		/// <summary>Document ID standard property name.</summary>
		public const string RevisionPropertyName = "_rev";

		/// <summary>Document reserved property names.</summary>
		public static readonly string[] ReservedPropertyNames = 
			new[] { IdPropertyName, RevisionPropertyName };

		/// <constructor />
		public DesignDocument(JObject defenition, string id, string revision = null)
		{
			if(string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException("id");
			if(!id.StartsWith(IdPrefix)) 
				throw new ArgumentException("Design document IDs should begin with '_design/'", "id");
			if(defenition == null) throw new ArgumentNullException("defenition");
			Contract.EndContractBlock();

			Id = id;
			Revision = revision;
			Definition = defenition;
		}

		/// <summary>Document Id (part after "_design/").</summary>
		public string Id { get; private set; }

		/// <summary>Design document revision.</summary>
		public string Revision { get; private set; }

		/// <summary>Document body.</summary>
		public JObject Definition { get; private set; }

		/// <summary>Determines if documents apears new.</summary>
		public bool IsNew { get { return Revision == null; } }

		/// <inheritdoc/>
		public bool Equals(DesignDocument other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return new JTokenEqualityComparer().Equals(RemoveRev(other.Definition), RemoveRev(Definition));
		}

		private static JToken RemoveRev(JObject defenition)
		{
			if (defenition.Property(RevisionPropertyName) == null)
				return defenition;
			var obj = (JObject)defenition.DeepClone();
			obj.Remove(RevisionPropertyName);
			return obj;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (DesignDocument)) return false;
			return Equals((DesignDocument) obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return RemoveRev(Definition).GetHashCode();
		}

		/// <inheritdoc/>
		public static bool operator ==(DesignDocument left, DesignDocument right)
		{
			return Equals(left, right);
		}

		/// <inheritdoc/>
		public static bool operator !=(DesignDocument left, DesignDocument right)
		{
			return !Equals(left, right);
		}

		/// <inheritdoc/>
		public DesignDocument CopyWithRevision(string revision)
		{
			var docWithRevision = (JObject)Definition.DeepClone();

			if (docWithRevision[RevisionPropertyName] != null)
				docWithRevision.Remove(RevisionPropertyName);

			docWithRevision.Property(IdPropertyName).AddAfterSelf(
				new JProperty(RevisionPropertyName, JToken.FromObject(revision))
			);
			return new DesignDocument(docWithRevision, Id, revision);
		}
	}
}