using System;
using System.IO;
using CommandLine;
using CommandLine.Text;
using CouchDude.Core.DesignDocumentManagment;

namespace CouchDude
{
	class Program
	{
		private static readonly HeadingInfo HeadingInfo = new HeadingInfo("CouchDude.Console", "1.0");
		private const int OkReturnCode = 0;
		private const int IncorrectOptionsReturnCode = 1;
		private const int UnknownErrorReturnCode = 2;
		private const string PasswordEnvVar = "COUCH_DUDE_PASSWORD";

		public enum CommandType
		{
			Help = 0,
			Generate,
			Check,
			Push
		}

		public sealed class Options
		{
			[Option("c", "command", Required = true, HelpText = "help|generate|check|push")]
			public CommandType Command = CommandType.Help;

			[Option("a", "address",
							HelpText = "Database URL")]
			public string DatabaseUrl = null;

			[Option("d", "directory",
							HelpText = "Base directory for document generation")]
			public string BaseDirectory = string.Empty;
			
			[HelpOption(HelpText = "Dispaly this help screen")]
			public string GetUsage()
			{
				var help = new HelpText(HeadingInfo) {
					AdditionalNewLineAfterOption = true
				};
				help.AddPreOptionsLine(
					"Usage: couchdude check -a http://admin:passw0rd@example.com:5984/yourdb [-d .\\designDocuments]");
				help.AddPreOptionsLine(PasswordEnvVar + " enviroment option used as database password if set.");
				help.AddOptions(this);
				return help;
			}
		}

		static int Main(string[] args)
		{
			var options = new Options();
			ICommandLineParser parser = 
				new CommandLineParser(new CommandLineParserSettings(Console.Error));
			if (!parser.ParseArguments(args, options))
			{
				WriteError("Some of argumens are incorrect.\n");
				return IncorrectOptionsReturnCode;
			}

			var directoryPath =
				!string.IsNullOrWhiteSpace(options.BaseDirectory)? options.BaseDirectory: Environment.CurrentDirectory;

			var baseDirectory = new DirectoryInfo(directoryPath);
			if(!baseDirectory.Exists)
			{
				WriteError("Provided directory {0} does not exist.", options.BaseDirectory);
				return IncorrectOptionsReturnCode;
			}

			var password = Environment.GetEnvironmentVariable(PasswordEnvVar);
			var url = new Lazy<Uri>(() => ParseDatabaseUrl(options));

			if (options.Command == CommandType.Help)
				Console.WriteLine(options.GetUsage());
			else
				try 
				{
					ExecuteCommand(options.Command, baseDirectory, url, password);
				}
				catch (Exception e)
				{
#if DEBUG
					WriteError(e.ToString());
#else
					WriteError(e.Message);
#endif
					return UnknownErrorReturnCode;
				}

			return OkReturnCode;
		}

		private static void ExecuteCommand(CommandType command, DirectoryInfo baseDirectory, Lazy<Uri> url, string password) 
		{
			var engine = Engine.CreateStandard(baseDirectory);
			switch (command)
			{
				case CommandType.Help:
					break;
				case CommandType.Generate:
					var generatedDocuments = engine.Generate();
					foreach (var generatedDocument in generatedDocuments)
					{
						Console.WriteLine(generatedDocument);
						Console.WriteLine();
					}
					break;
				case CommandType.Check:
					var haveChanged =
						engine.CheckIfChanged(url.Value, password);
					Console.WriteLine(haveChanged? "Changed": "Have not changed");
					break;
				case CommandType.Push:
					engine.PushIfChanged(url.Value, password);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		static Uri ParseDatabaseUrl(Options options)
		{
			Uri uri;
			if(!Uri.TryCreate(options.DatabaseUrl, UriKind.RelativeOrAbsolute, out uri))
			{
				WriteError("Provided URI is malformed: {0}", options.DatabaseUrl);
				Environment.Exit(IncorrectOptionsReturnCode);
			}
			if (uri.Scheme != "http" && uri.Scheme != "https")
			{
				WriteError("Provided URI is not of HTTP(S) scheme: {0}", options.DatabaseUrl);
				Environment.Exit(IncorrectOptionsReturnCode);
			}
			if (!uri.IsAbsoluteUri)
			{
				WriteError("Provided URI is not absolute: {0}", options.DatabaseUrl);
				Environment.Exit(IncorrectOptionsReturnCode);
			}
			return uri;
		}

		private static void WriteError(string message, params object[] messageParams)
		{
			var oldColor = Console.ForegroundColor;
			try
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine(message, messageParams);
			}
			finally
			{
				Console.ForegroundColor = oldColor;
			}
		}
	}
}
