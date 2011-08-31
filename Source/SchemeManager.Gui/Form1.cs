using System;
using System.IO;
using Common.Logging;
using System.Windows.Forms;
using CouchDude.Core.DesignDocumentManagment;
using File = System.IO.File;

namespace CouchDude.Gui
{
	public partial class Form1 : Form
	{
		public Form1()
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
			Push
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
						OutputBox.AppendText(generatedDocument.Replace("\\r\\n", "\r\n").Replace("\\t", "\t"));
					}
					break;
				case CommandType.Check:
					var haveChanged =
						engine.CheckIfChanged(url.Value, password);
					MessageBox.Show(haveChanged ? "Changed" : "Have not changed");
					break;
				case CommandType.Push:
					engine.PushIfChanged(url.Value, password);
					MessageBox.Show("Pushed");
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
	}
}