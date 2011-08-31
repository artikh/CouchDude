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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Logging;
using Newtonsoft.Json.Linq;

namespace CouchDude.SchemeManager
{
	/// <summary>Assembles design documents from file system structure</summary>
	public class DesignDocumentAssembler: IDesignDocumentAssembler
	{
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();

		private readonly IDirectory directory;

		/// <constructor />
		public DesignDocumentAssembler(IDirectory directory)
		{
			this.directory = directory;
		}

		/// <summary>Geterates design documents form file system structures.</summary>
		public IDictionary<string, DesignDocument> Assemble()
		{
			return GenerateInternal(directory).ToDictionary(doc => doc.Id, doc => doc);
		}

		private static IEnumerable<DesignDocument> GenerateInternal(IDirectory directory)
		{
			foreach (var subDirectory in directory.EnumerateDirectories())
			{
				var id = GetId(subDirectory);
				var designDocument = new JObject(new JProperty(DesignDocument.IdPropertyName, id));
				MapPropertiesRecursive(designDocument, subDirectory);
				yield return new DesignDocument(designDocument, id);
			}
		}

		private static void MapPropertiesRecursive(JObject jObject, IDirectory directory)
		{
			foreach (var file in directory.EnumerateFiles())
			{
				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
				if (fileNameWithoutExtension == null || fileNameWithoutExtension == DesignDocument.IdPropertyName)
					continue;
				if (DesignDocument.ReservedPropertyNames.Contains(fileNameWithoutExtension))
					throw new DesignDocumentAssemblerException("File name {0} is reserved.", file);

				if (jObject.Property(fileNameWithoutExtension) != null)
					throw CreateClashingFilesException(directory, fileNameWithoutExtension);
				jObject[fileNameWithoutExtension] = ReadFile(file);
			}

			foreach (var subDirectory in directory.EnumerateDirectories())
			{
				var subDirectoryName = subDirectory.Name;

				if (DesignDocument.ReservedPropertyNames.Contains(subDirectoryName))
					throw new DesignDocumentAssemblerException(
						"Directory name {0} is reserved.", subDirectoryName);
				if (jObject.Property(subDirectoryName) != null)
					throw CreateClashingFilesException(directory, subDirectoryName);

				var subObject = new JObject();
				jObject[subDirectoryName] = subObject;
				MapPropertiesRecursive(subObject, subDirectory);
			}
		}

		private static string ReadFile(IFile file) 
		{
			using (var stream = file.OpenRead())
			using (var reader = new StreamReader(stream))
				return reader.ReadToEnd().Trim(' ', '\t', '\n', '\r');
		}

		private static DesignDocumentAssemblerException 
			CreateClashingFilesException(IDirectory directory, string clashingName)
		{
			var clushingFileNames = directory
				.EnumerateFiles()
				.Where(f => Path.GetFileNameWithoutExtension(f.Name) == clashingName)
				.Select(f => f.Name);
			var clushingSubDirectoryNames = directory
				.EnumerateDirectories()
				.Where(d => d.Name == clashingName)
				.Select(d => d.Name);

			var clashingDirectoriesAndFiles = 
				clushingFileNames.Union(clushingSubDirectoryNames).ToArray();
			return new DesignDocumentAssemblerException(
				"Files and subdirectories with same name and different extensions detected: {0}",
				string.Join(", ", clashingDirectoriesAndFiles));
		}

		private static string GetId(IDirectory subDirectory)
		{
			return DesignDocument.IdPrefix + (TryReadProvidedId(subDirectory) ?? subDirectory.Name);
		}

		private static string TryReadProvidedId(IDirectory subDirectory) 
		{
			var idFile = GetIdFile(subDirectory);
			if (idFile == null)
				return null;

			var providedId = ReadFile(idFile);

			if(string.IsNullOrWhiteSpace(providedId))
				throw new DesignDocumentAssemblerException(
					"There is {0} file exists in {1} directory, but it's empty.", idFile, subDirectory.Name);

			if(Log.IsWarnEnabled)
			{
				if (providedId.Length > 256)
					Log.WarnFormat("{0}\\{1} is too long.", subDirectory.Name, idFile);

				if (providedId.Contains('/'))
					Log.WarnFormat(
						"{0}\\{1} contains slash, it should be prohibited.",
						subDirectory.Name, 
						idFile);
			}
			return providedId;
		}

		private static IFile GetIdFile(IDirectory subDirectory)
		{
			return subDirectory
				.EnumerateFiles()
				.FirstOrDefault(n => Path.GetFileNameWithoutExtension(n.Name) == DesignDocument.IdPropertyName);
		}
	}
}