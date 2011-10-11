#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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
using System.Linq;

using CouchDude.SchemeManager;
using Xunit;
using Directory = CouchDude.SchemeManager.Directory;

namespace CouchDude.Tests.Unit.SchemeManager
{
	public class DirectoryTests: IDisposable
	{
		private readonly DirectoryInfo directoryInfo;
		private readonly IDirectory directory;

		public DirectoryTests()
		{
			var tempPath = Path.Combine(
				Path.GetTempPath(), Guid.NewGuid().ToString());
			directoryInfo = new DirectoryInfo(tempPath);
			directoryInfo.Create();
			directory = new Directory(directoryInfo);
		}
		
		public void Dispose()
		{
			lock (directoryInfo)
				if(directoryInfo != null && directoryInfo.Exists)
					directoryInfo.Delete(recursive: true);
		}

		private void WriteFile(string path, string content = "")
		{
			var fileInfo = new FileInfo(Path.Combine(directoryInfo.FullName, path));
			using (var writer = fileInfo.CreateText())
				writer.Write(content);
		}

		private static string ReadStream(Stream stream)
		{
			using (var reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}

		[Fact]
		public void ShouldOpenSimpleFileToRead()
		{
			const string content = "test\ntest\ntest";
			WriteFile("test.txt", content);

			string actual;
			using (var stream = 
				directory.EnumerateFiles().First(f => f.Name == "test.txt").OpenRead())
				actual = ReadStream(stream);

			Assert.Equal(content, actual);
		}

		[Fact]
		public void ShoudEnumerateFiles()
		{
			WriteFile("file1.test");
			WriteFile("file2.test");
			WriteFile("file3.test");
			directoryInfo.CreateSubdirectory("sub");

			var files = directory.EnumerateFiles().OrderBy(f => f.Name).ToArray();

			Assert.Equal(3, files.Length);
			Assert.Equal("file1.test", files[0].Name);
			Assert.Equal("file2.test", files[1].Name);
			Assert.Equal("file3.test", files[2].Name);
		}

		[Fact]
		public void ShoudEnumerateDirectories()
		{
			WriteFile("file.test");
			directoryInfo.CreateSubdirectory("sub1");
			WriteFile("sub1\\test.txt");
			directoryInfo.CreateSubdirectory("sub2");
			WriteFile("sub2\\test.txt");
			directoryInfo.CreateSubdirectory("sub3");
			WriteFile("sub3\\test.txt");

			var directories = directory.EnumerateDirectories().ToArray();
			Assert.Equal(3, directories.Count());
			AssertDirectoryWithFile(directories[0]);
			AssertDirectoryWithFile(directories[1]);
			AssertDirectoryWithFile(directories[2]);
		}

		private static void AssertDirectoryWithFile(IDirectory directory)
		{
			Assert.NotNull(directory);
			var files = directory.EnumerateFiles().ToArray();
			Assert.Equal(1, files.Length);
			Assert.Equal("test.txt", files[0].Name);
		}
	}
}
