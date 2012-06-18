using System;
using CouchDude.Serialization;
using JetBrains.Annotations;

namespace CouchDude
{
	/// <summary>Represents API-layer settings</summary>
	public class CouchApiSettings 
	{
		readonly Uri serverUri;
		ISerializer serializer;
		string replicatorDatabase;

		/// <constructor />
		public CouchApiSettings(string serverUriString): this(ParseUri(serverUriString)) { }

		static Uri ParseUri([NotNull] string serverUriString)
		{
			if (string.IsNullOrEmpty(serverUriString)) throw new ArgumentNullException("serverUriString");
			return new Uri(serverUriString, UriKind.RelativeOrAbsolute);
		}

		/// <constructor />
		public CouchApiSettings([NotNull] Uri serverUri)
		{
			if (serverUri == null) throw new ArgumentNullException("serverUri");
			if (!serverUri.IsAbsoluteUri) throw new ArgumentOutOfRangeException("serverUri", serverUri, "CouchDB URI should be absolute");
			this.serverUri = serverUri;
		}

		/// <summary>CouchDB server URL.</summary>
		public Uri ServerUri { get { return serverUri; } }

		/// <summary>CouchDB access credentials.</summary>
		public Credentials Credentials { get; set; }

		/// <summary>CouchDude serializer instance.</summary>
		public ISerializer Serializer
		{
			get { return serializer ?? (serializer = new NewtonsoftSerializer()); } 
			set { serializer = value; }
		}

		/// <summary>Replicator database name.</summary>
		public string ReplicatorDatabase
		{
			get { return replicatorDatabase ?? "_replicator"; } 
			set { replicatorDatabase = value; }
		}
	}
}