using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Common.Logging;
using CouchDude.Core.HttpClient;
using Newtonsoft.Json;

namespace CouchDude.Core.DesignDocumentManagment
{
	/// <summary>Orchestrates Couch Dude's actions.</summary>
	public class Engine
	{
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();

		private readonly IHttpClient httpClient;
		private readonly IDesignDocumentAssembler designDocumentAssembler;
		private readonly IDesignDocumentExtractor designDocumentExtractor;
		
		/// <constructor />
		internal Engine(IHttpClient httpClient, IDesignDocumentExtractor designDocumentExtractor, IDesignDocumentAssembler designDocumentAssembler)
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
			Log.InfoFormat("{0} design documents will be pushed to database.", changedDocs.Count);

			foreach (var changedDoc in changedDocs) 
			{
				Log.InfoFormat("Pushing document {0} to the database.", changedDoc.Id);

				//пропускает папки, создаваемые при билде солюшена
				if (changedDoc.Id == "_design/obj" || changedDoc.Id == "_design/bin" || changedDoc.Id == "_design/Properties") continue;

				var documentUri = new Uri(databaseUri, changedDoc.Id);
				changedDoc.Definition.ToString(Formatting.None);
				httpClient.MakeRequest(
					new HttpRequest(
						documentUri, 
						"PUT", 
						body: new StringReader(changedDoc.Definition.ToString(Formatting.None))));
			}
		}

		/// <summary>Удаляет базу данных и создает снова.</summary>
		public void Truncate(Uri databaseUri, string password = null)
		{
			if (databaseUri == null) throw new ArgumentNullException("databaseUri");
			if (!databaseUri.IsAbsoluteUri)
				throw new ArgumentException("databaseUri should be absolute", "databaseUri");
			if (!databaseUri.Scheme.StartsWith("http"))
				throw new ArgumentException("databaseUri should be http or https", "databaseUri");
			Contract.EndContractBlock();

			FillPasswordIn(ref databaseUri, password);

			//var docsFromDatabase = GetDesignDocumentsFromDatabase(databaseUri);
			//var docsFromFileSystem = GetDesignDocumentsFromFileSystem();
			//var changedDocs = GetChangedDocuments(docsFromFileSystem, docsFromDatabase);
			//Log.InfoFormat("{0} design documents will be pushed to database.", changedDocs.Count);

			httpClient.MakeRequest(
					new HttpRequest(
						databaseUri,
						"DELETE"));

			httpClient.MakeRequest(
					new HttpRequest(
						databaseUri,
						"PUT"));
		}

		/// <summary>Generates design documents from directory content.</summary>
		public IEnumerable<string> Generate()
		{
			return GetDesignDocumentsFromFileSystem().Values
				.Select(dd => dd.Definition.ToString(Formatting.Indented));
		}

		private static void FillPasswordIn(ref Uri databaseUri, string password) 
		{
			if (password != null)
				databaseUri = new UriBuilder(databaseUri) { Password = password }.Uri;
		}

		private static IList<DesignDocument> GetChangedDocuments(
			IDictionary<string, DesignDocument> docsFromFs, 
			IDictionary<string, DesignDocument> docsFromDb)
		{
			Log.Info("Figuring out if any document have changed.");
			var changedDocuments = new List<DesignDocument>();
			foreach (var docFromFs in docsFromFs.Values)
			{
				DesignDocument docFromDb;
				if (!docsFromDb.TryGetValue(docFromFs.Id, out docFromDb))
				{
					Log.InfoFormat("Design document {0} is new:\n{1}", docFromFs.Id, docFromFs.Definition);
					changedDocuments.Add(docFromFs);
				}
				else if (docFromDb != docFromFs)
				{
					Log.InfoFormat("Design document {0} have changed:\n{1}", docFromFs.Id, docFromFs.Definition);
					changedDocuments.Add(docFromFs.CopyWithRevision(docFromDb.Revision));
				}
			}

			if(Log.IsInfoEnabled)
			{
				if(changedDocuments.Count == 0)
					Log.Info("No design documents have changed.");
				else
					Log.InfoFormat("{0} design documents will be pushed to database.", changedDocuments.Count);
			}

			return changedDocuments;
		}

		private IDictionary<string, DesignDocument> GetDesignDocumentsFromDatabase(Uri databaseUri) 
		{
			Log.Info("Downloading design documents from database...");
			var response = httpClient.MakeRequest(
				new HttpRequest(
					databaseUri + @"_all_docs?startkey=""_design/""&endkey=""_design0""&include_docs=true", 
					HttpMethod.Get)
				);
			var designDocumentsFromDatabase = designDocumentExtractor.Extract(response.Body);
			Log.InfoFormat(
				"{0} design documens downloaded from database.", designDocumentsFromDatabase.Count);
			return designDocumentsFromDatabase;
		}

		private IDictionary<string, DesignDocument> GetDesignDocumentsFromFileSystem()
		{
			Log.Info("Creating design documents from file system...");
			var designDocumentsFromFileSystem = designDocumentAssembler.Assemble();
			Log.InfoFormat(
				"{0} design documens created from file system.", designDocumentsFromFileSystem.Count);
			return designDocumentsFromFileSystem;
		}
	}
}