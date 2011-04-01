using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using CouchDude.Core.Implementation;
using Newtonsoft.Json;

namespace CouchDude.Core.DesignDocumentManagment
{
	/// <summary>Orchestrates Couch Dude's actions.</summary>
	public class Engine
	{
		private readonly IHttp http;
		private readonly IDesignDocumentAssembler designDocumentAssembler;
		private readonly IDesignDocumentExtractor designDocumentExtractor;

		/// <constructor />
		public Engine(IHttp http, IDesignDocumentExtractor designDocumentExtractor, IDesignDocumentAssembler designDocumentAssembler)
		{
			if(http == null) throw new ArgumentNullException("http");
			if(designDocumentAssembler == null) 
				throw new ArgumentNullException("designDocumentAssembler");
			Contract.EndContractBlock();

			this.http = http;
			this.designDocumentExtractor = designDocumentExtractor;
			this.designDocumentAssembler = designDocumentAssembler;
		}

		/// <summary>Creates engine based on provided <paramref name="directoryInfo"/>.</summary>
		public static Engine CreateStandard(DirectoryInfo directoryInfo)
		{
			var directory = new Directory(directoryInfo);
			var documentAssembler = new DesignDocumentAssembler(directory);
			var couchProxy = new Http();
			var designDocumentExtractor = new DesignDocumentExtractor();
			return new Engine(couchProxy, designDocumentExtractor, documentAssembler);
		}

		/// <summary>Checks if database design document set have changed comparing with
		/// one generated from file system tree.</summary>
		public bool CheckIfChanged(Uri databaseUri, string password = null)
		{
			if (databaseUri == null) throw new ArgumentNullException("databaseUri");
			if (!databaseUri.IsAbsoluteUri)
				throw new ArgumentException("databaseUri should be absolute", "databaseUri");
			if (!databaseUri.Scheme.StartsWith("http"))
				throw new ArgumentException("databaseUri should be http or https", "databaseUri");
			Contract.EndContractBlock();

			FillPasswordIn(ref databaseUri, password);

			var docsFromDatabase = GetDesignDocumentsFromDatabase(databaseUri);
			var docsFromFileSystem = GetDesignDocumentsFromFileSystem();

			return GetChangedDocuments(docsFromFileSystem, docsFromDatabase).Count() > 0;
		}

		/// <summary>Updates database design document set with one generated from file system
		/// tree.</summary>
		public void PushIfChanged(Uri databaseUri, string password = null)
		{			
			if(databaseUri == null) throw new ArgumentNullException("databaseUri");
			if(!databaseUri.IsAbsoluteUri) 
				throw new ArgumentException("databaseUri should be absolute", "databaseUri");
			if(!databaseUri.Scheme.StartsWith("http")) 
				throw new ArgumentException("databaseUri should be http or https", "databaseUri");
			Contract.EndContractBlock();

			FillPasswordIn(ref databaseUri, password);

			var docsFromDatabase = GetDesignDocumentsFromDatabase(databaseUri);
			var docsFromFileSystem = GetDesignDocumentsFromFileSystem();
			var changedDocs = GetChangedDocuments(docsFromFileSystem, docsFromDatabase);

			foreach (var changedDoc in changedDocs)
				PostOrPutDocument(changedDoc, databaseUri);
		}

		/// <summary>Generates design documents from directory content.</summary>
		public IEnumerable<string> Generate()
		{
			return GetDesignDocumentsFromFileSystem().Values
				.Select(dd => dd.Definition.ToString(Formatting.Indented));
		}

		private void PostOrPutDocument(DesignDocument doc, Uri databaseUri)
		{
			var documentUri = new Uri(databaseUri, doc.Id);
			doc.Definition.ToString(Formatting.None);
			using (var documentStringReader = 
				new StringReader(doc.Definition.ToString(Formatting.None)))
				http.Request(documentUri, "PUT", documentStringReader);
		}

		private static void FillPasswordIn(ref Uri databaseUri, string password) 
		{
			if (password != null)
				databaseUri = new UriBuilder(databaseUri) { Password = password }.Uri;
		}

		private static IEnumerable<DesignDocument> GetChangedDocuments(
			IDictionary<string, DesignDocument> docsFromFs, 
			IDictionary<string, DesignDocument> docsFromDb)
		{
			var changedDocuments = new List<DesignDocument>();
			foreach (var docFromFs in docsFromFs.Values)
			{
				DesignDocument docFromDb;
				if (!docsFromDb.TryGetValue(docFromFs.Id, out docFromDb)) 
					changedDocuments.Add(docFromFs);
				else if (docFromDb != docFromFs)
					changedDocuments.Add(docFromFs.CopyWithRevision(docFromDb.Revision));
			}
			return changedDocuments;
		}

		private IDictionary<string, DesignDocument> GetDesignDocumentsFromDatabase(Uri databaseUri) 
		{
			var getAllDocumentsUri = 
				new Uri(
					databaseUri, 
					@"_all_docs?startkey=""_design/""&endkey=""_design0""&include_docs=true");

			using (var textReader = http.RequestAndOpenTextReader(getAllDocumentsUri, "GET", null))
				return designDocumentExtractor.Extract(textReader);
		}

		private IDictionary<string, DesignDocument> GetDesignDocumentsFromFileSystem() 
		{
			return designDocumentAssembler.Assemble();
		}
	}
}