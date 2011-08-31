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
using System.IO;
using CommandLine;
using CommandLine.Text;
using Common.Logging;
using CouchDude.SchemeManager;

#pragma warning disable 0649

namespace CouchDude
{
	class Program
	{
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();

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

			[Option("a", "address", HelpText = "Database URL")]
			public string DatabaseUrl;

			[Option("v", "verbose", HelpText = "Log diagnostics to console window")]
			public bool Verbose;

			[Option("d", "directory", HelpText = "Base directory for document generation")]
			public string BaseDirectory = string.Empty;
			
			[HelpOption(HelpText = "Dispaly this help screen")]
			public string GetUsage()
			{
				var help = new HelpText(HeadingInfo) {
					AdditionalNewLineAfterOption = true
				};
				help.AddPreOptionsLine("Usage: couchdude check -a http://admin:passw0rd@example.com:5984/yourdb [-d .\\designDocuments]");
				help.AddPreOptionsLine(PasswordEnvVar + " enviroment option used as database password if set.");
				help.AddOptions(this);
				return help;
			}
		}

		static int Main(string[] args)
		{
			var options = new Options();
			ICommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
			if (!parser.ParseArguments(args, options))
			{
				Console.Error.WriteLine("Some of argumens are incorrect");
				return IncorrectOptionsReturnCode;
			}

			ChangeLoggingLevel(options.Verbose ? "INFO" : "WARN");

			var directoryPath = !string.IsNullOrWhiteSpace(options.BaseDirectory)? options.BaseDirectory: Environment.CurrentDirectory;

			var baseDirectory = new DirectoryInfo(directoryPath);
			if(!baseDirectory.Exists)
			{
				Log.ErrorFormat("Provided directory {0} does not exist.", options.BaseDirectory);
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
					Log.ErrorFormat(e.ToString());
					return UnknownErrorReturnCode;
				}

			return OkReturnCode;
		}

		private static void ChangeLoggingLevel(string level)
		{
			var repositories= log4net.LogManager.GetAllRepositories();

			//Configure all loggers to be at the debug level.
			foreach (var repository in repositories)
			{
				repository.Threshold = repository.LevelMap[level];
					var hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
					var loggers=hier.GetCurrentLoggers();
				foreach (var logger in loggers)
					((log4net.Repository.Hierarchy.Logger) logger).Level = hier.LevelMap[level];
			}

			//Configure the root logger.
			var h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
			var rootLogger = h.Root;
			rootLogger.Level = h.LevelMap[level];
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
			if (!options.DatabaseUrl.EndsWith("/"))
				options.DatabaseUrl = options.DatabaseUrl + "/";

			if(!Uri.TryCreate(options.DatabaseUrl, UriKind.RelativeOrAbsolute, out uri))
			{
				Log.ErrorFormat("Provided URI is malformed: {0}", options.DatabaseUrl);
				Environment.Exit(IncorrectOptionsReturnCode);
			}
			if (uri.Scheme != "http" && uri.Scheme != "https")
			{
				Log.ErrorFormat("Provided URI is not of HTTP(S) scheme: {0}", options.DatabaseUrl);
				Environment.Exit(IncorrectOptionsReturnCode);
			}
			if (!uri.IsAbsoluteUri)
			{
				Log.ErrorFormat("Provided URI is not absolute: {0}", options.DatabaseUrl);
				Environment.Exit(IncorrectOptionsReturnCode);
			}
			return uri;
		}
	}
}
