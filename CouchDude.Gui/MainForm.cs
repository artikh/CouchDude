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
using Common.Logging;
using System.Windows.Forms;
using CouchDude.Core.DesignDocumentManagment;
using File = System.IO.File;

namespace CouchDude.Gui
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			if (File.Exists(Environment.CurrentDirectory + "/ls.txt"))
				using (var sw = new StreamReader(Environment.CurrentDirectory + "/ls.txt"))
				{
					BaseDirectory.Text = sw.ReadLine();
					DatabaseUrl.Text = sw.ReadLine();
				}
			if (string.IsNullOrEmpty(DatabaseUrl.Text))
				DatabaseUrl.Text = @"http://localhost:5984/mecasto";
		}

		private static readonly ILog Log = LogManager.GetCurrentClassLogger();

		private const int OkReturnCode = 0;
		private const int IncorrectOptionsReturnCode = 1;
		private const int UnknownErrorReturnCode = 2;
		private const string PasswordEnvVar = "COUCH_DUDE_PASSWORD";

		public enum CommandType
		{
			Help = 0,
			Generate,
			Check,
			Push,
			Truncate
		}

		public sealed class Options
		{
			//[Option("c", "command", Required = true, HelpText = "help|generate|check|push")]
			public CommandType Command = CommandType.Help;

			//[Option("a", "address", HelpText = "Database URL")]
			public string DatabaseUrl;

			//[Option("d", "directory", HelpText = "Base directory for document generation")]
			public string BaseDirectory = string.Empty;
		}

		private int Process(CommandType command)
		{
			var options = new Options { DatabaseUrl = DatabaseUrl.Text, BaseDirectory = BaseDirectory.Text, Command = command };

			var directoryPath = !string.IsNullOrWhiteSpace(options.BaseDirectory) ? options.BaseDirectory : Environment.CurrentDirectory;

			var baseDirectory = new DirectoryInfo(directoryPath);
			if (!baseDirectory.Exists)
			{
				MessageBox.Show(@"Provided directory {0} does not exist.", options.BaseDirectory);
				return IncorrectOptionsReturnCode;
			}

			var password = Environment.GetEnvironmentVariable(PasswordEnvVar);
			var url = new Lazy<Uri>(() => ParseDatabaseUrl(options));

			try
			{
				ExecuteCommand(options.Command, baseDirectory, url, password);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
				return UnknownErrorReturnCode;
			}

			return OkReturnCode;
		}



		private void ExecuteCommand(CommandType command, DirectoryInfo baseDirectory, Lazy<Uri> url, string password)
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
						OutputBox.AppendText("\r\n");
						OutputBox.AppendText(generatedDocument.Replace("\\r\\n", "\r\n").Replace("\\t", "\t"));
						OutputBox.AppendText("\r\n");
					}
					break;
				case CommandType.Check:
					var haveChanged =
						engine.CheckIfChanged(url.Value, password);
					OutputBox.AppendText("\r\n");
					OutputBox.AppendText(haveChanged ? "Changed" : "Have not changed");
					OutputBox.AppendText("\r\n");
					break;
				case CommandType.Push:
					engine.PushIfChanged(url.Value, password);
					OutputBox.AppendText("\r\nPushed\r\n");
					break;
				case CommandType.Truncate:
					engine.Truncate(url.Value, password);
					OutputBox.AppendText("\r\nTruncated\r\n");
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

			if (!Uri.TryCreate(options.DatabaseUrl, UriKind.RelativeOrAbsolute, out uri))
			{
				MessageBox.Show(string.Format("Provided URI is malformed: {0}", options.DatabaseUrl));
				Environment.Exit(IncorrectOptionsReturnCode);
			}
			if (uri.Scheme != "http" && uri.Scheme != "https")
			{
				MessageBox.Show(string.Format("Provided URI is not of HTTP(S) scheme: {0}", options.DatabaseUrl));
				Environment.Exit(IncorrectOptionsReturnCode);
			}
			if (!uri.IsAbsoluteUri)
			{
				MessageBox.Show(string.Format("Provided URI is not absolute: {0}", options.DatabaseUrl));
				Environment.Exit(IncorrectOptionsReturnCode);
			}
			return uri;
		}

		private void CheckButton_Click(object sender, System.EventArgs e)
		{
			Process(CommandType.Check);
		}

		private void BrowseButton_Click(object sender, System.EventArgs e)
		{
			var dialog = new FolderBrowserDialog { ShowNewFolderButton = false };
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				BaseDirectory.Text = dialog.SelectedPath;
				using (var sw = new StreamWriter(Environment.CurrentDirectory + "/ls.txt"))
				{
					sw.WriteLine(BaseDirectory.Text);
					sw.WriteLine(DatabaseUrl.Text);
				}
			}

		}

		private void PushButton_Click(object sender, EventArgs e)
		{
			Process(CommandType.Push);
		}

		private void GenerateButton_Click(object sender, EventArgs e)
		{
			Process(CommandType.Generate);
		}

		private void TruncateButton_Click(object sender, EventArgs e)
		{
			Process(CommandType.Truncate);
			Process(CommandType.Push);
		}
	}
}