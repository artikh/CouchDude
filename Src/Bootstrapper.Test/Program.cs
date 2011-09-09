using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Simple;
using CouchDude.Bootstrapper;

namespace Bootstrapper.Test
{
	class Program
	{
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();

		static void Main()
		{
			LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter();

			var replicationSettings = new ReplicationSettings {
				EndPointsToReplicateTo = new [] {
					new IPEndPoint(IPAddress.Loopback, 9998),
					new IPEndPoint(IPAddress.Loopback, 9997),
					new IPEndPoint(IPAddress.Loopback, 9996)
				},
				DatabasesToReplicate = new [] { "mecasto" }
			};

			var watchDogs = replicationSettings.EndPointsToReplicateTo
				.AsParallel()
				.Select(ep => StartCouchInstance(ep.Port - 10, ep.Port, replicationSettings))
				.ToArray();

			Console.Write("Press q to close CouchDB: ");
			while (Console.ReadKey().KeyChar != 'q') {}
			foreach (var watchDog in watchDogs)
				watchDog.TerminateIfUp();
			Log.Info("CouchDB have been terminated");
			
			Console.Write("Press ENTER to exit...");
			Console.ReadLine();
		}

		private static CouchDBWatchdog StartCouchInstance(int lucenePort, int port, ReplicationSettings replicationSettings)
		{
			var storageDir =
				new DirectoryInfo(Path.Combine(Path.GetTempPath(), "couch-bootstrapper-" + DateTime.Now.Ticks + port));
			storageDir.Create();

			var dataDir = new DirectoryInfo(Path.Combine(storageDir.FullName, "data"));
			dataDir.Create();
			var logDir = new DirectoryInfo(Path.Combine(storageDir.FullName, "logs"));
			logDir.Create();
			var binDir = new DirectoryInfo(Path.Combine(storageDir.FullName, "bin"));
			binDir.Create();

			var bootstrapSettings = new BootstrapSettings {
				CouchDBDistributive =
					new FileInfo(
						Path.Combine(Environment.CurrentDirectory, "couchdb-1.1.0+COUCHDB-1152_otp_R14B03_lean.zip")),
				CouchDBLuceneDistributive =
					new FileInfo(Path.Combine(Environment.CurrentDirectory, "couchdb-lucene-0.7.0.zip")),
				JavaDistributive = new FileInfo(Path.Combine(Environment.CurrentDirectory, "jre6.zip")),
				CouchDBLucenePort = lucenePort,
				BinDirectory = binDir,
				DataDirectory = dataDir,
				LogDirectory = logDir,
				EndpointToListenOn = new IPEndPoint(IPAddress.Loopback, port),
				SetupCouchDBLucene = true,
			};

			bootstrapSettings.ReplicationSettings.DatabasesToReplicate = replicationSettings.DatabasesToReplicate;
			bootstrapSettings.ReplicationSettings.EndPointsToReplicateTo = replicationSettings.EndPointsToReplicateTo;

			var watchDog = CouchDBBootstraper.Bootstrap(bootstrapSettings);

			if (watchDog == null)
				throw new Exception("Unable to start");
			return watchDog;
		}
	}
}
