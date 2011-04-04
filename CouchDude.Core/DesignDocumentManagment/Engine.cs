using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using CouchDude.Core.HttpClient;
using Newtonsoft.Json;

namespace CouchDude.Core.DesignDocumentManagment
{
	/// <summary>Orchestrates Couch Dude's actions.</summary>
	public class Engine
	{
		private readonly IHttpClient httpClient;
		private readonly IDesignDocumentAssembler designDocumentAssembler;
		private readonly IDesignDocumentExtractor designDocumentExtractor;

		/// <constructor />
		public Engine(IHttpClient httpClient, IDesignDocumentExtractor designDocumentExtractor, IDesignDocumentAssembler designDocumentAssembler)
		{
			if(httpClient == null) throw new ArgumentNullException("httpClient");
			if(designDocumentAssembler == null) 
				throw new ArgumentNullException("designDocumentAssembler");
			Contract.EndContractBlock();

			this.httpClient = httpClient;
			this.designDocumentExtractor = designDocumentExtractor;
			this.designDocumentAssembler = designDocumentAssembler;
		}

		/// <summary>Creates engine based on provided <paramref name="directoryInfo"/>.</summary>
		public static Engine CreateStandard(DirectoryInfo directoryInfo)
		{
			var directory = new Directory(directoryInfo);
			var documentAssembler = new DesignDocumentAssembler(directory);
			var httpClient = new HttpClientImpl();
			var designDocumentExtractor = new DesignDocumentExtractor();
			return new Engine(httpClient, designDocumentExtractor, documentAssembler);
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
			httpClient.MakeRequest(new HttpRequest(
				documentUri, "PUT", body: new StringReader(doc.Definition.ToString(Formatting.None))));
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
			var response = httpClient.MakeRequest(
				new HttpRequest(
					new Uri(
						databaseUri,
						@"_all_docs?startkey=""_design/""&endkey=""_design0""&include_docs=true"), 
					HttpMethod.Get)
				);
			return designDocumentExtractor.Extract(response.Body);
		}

		private IDictionary<string, DesignDocument> GetDesignDocumentsFromFileSystem() 
		{
			return designDocumentAssembler.Assemble();
		}
	}
}