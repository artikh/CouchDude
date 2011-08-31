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

using Newtonsoft.Json.Linq;

namespace CouchDude.SchemeManager
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