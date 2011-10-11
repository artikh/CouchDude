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

using System.IO;
using System.Linq;
using System.Text;
using CouchDude.SchemeManager;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.SchemeManager
{
	public class DesignDocumentAssemblerTests : TestBase
	{
		private static readonly IFile[] EmptyFileArray = new IFile[0];
		private static readonly IDirectory[] EmptyDirectoryArray = new IDirectory[0];

		[Fact]
		public void ShouldProduceDesignDocumentsWithIdFromEmptyDirectories()
		{
			var directory = Mock.Of<IDirectory>(
				d => d.EnumerateDirectories() == new[] { 
					Mock.Of<IDirectory>(
						d1 =>
							d1.EnumerateFiles() == EmptyFileArray &&
							d1.EnumerateDirectories() == EmptyDirectoryArray &&
							d1.Name == "first_doc"
					), 
					Mock.Of<IDirectory>(
						d1 =>
							d1.EnumerateFiles() == EmptyFileArray &&
							d1.EnumerateDirectories() == EmptyDirectoryArray &&
							d1.Name == "second_doc"
					)
				}
			);

			var generatedDocs = new DesignDocumentAssembler(directory).Assemble();

			Assert.Equal(2, generatedDocs.Count);
			var keys = generatedDocs.Keys.ToList();
			var designDocumentA = generatedDocs[keys[0]];
			var designDocumentB = generatedDocs[keys[1]];

			Assert.Equal("_design/first_doc", designDocumentA.Id);
			Assert.Null(designDocumentA.Revision);
			Assert.Equal(
				JObject.Parse(@"{ ""_id"": ""_design/first_doc"" }"),
				designDocumentA.Definition,
				new JTokenEqualityComparer());

			Assert.Equal("_design/second_doc", designDocumentB.Id); 
			Assert.Null(designDocumentB.Revision);
			Assert.Equal(
				JObject.Parse(@"{ ""_id"": ""_design/second_doc"" }"),
				designDocumentB.Definition,
				new JTokenEqualityComparer());
		}

		[Fact]
		public void ShouldProduceDesignDocumentsWithIdFromSpecialFile()
		{
			var directory = Mock.Of<IDirectory>(
				d => d.EnumerateDirectories() == new[] {
					Mock.Of<IDirectory>(
						d1 =>
							d1.EnumerateFiles() == 
								new[] { File("_id.with-some-strange-extention", "some special ID\n\r" )} &&
							d1.EnumerateDirectories() == EmptyDirectoryArray &&
							d1.Name == "strange_name"
						)
				}
			);

			var generatedDocs = new DesignDocumentAssembler(directory).Assemble();
			
			Assert.Equal(1, generatedDocs.Count);
			var generatedDoc = generatedDocs.Values.First();

			Assert.Equal("_design/some special ID", generatedDoc.Id);
			Assert.Null(generatedDoc.Revision);
			Assert.Equal(
				JObject.Parse(@"{ ""_id"": ""_design/some special ID"" }"),
				generatedDoc.Definition,
				new JTokenEqualityComparer());
		}

		[Fact]
		public void ShouldMapTextFilesInDirectoryToDocumentProperties()
		{
			var directory = Mock.Of<IDirectory>(
				d => d.EnumerateDirectories() == new[] {
					Mock.Of<IDirectory>(
						d1 =>
							d1.EnumerateFiles() == new[] {
								File("prop1.txt", "prop1 value"),
								File("prop2.js", "prop2 value")
							} &&
							d1.EnumerateDirectories() == EmptyDirectoryArray &&
							d1.Name == "doc1"
						)
				}
			);

			var generatedDocs = new DesignDocumentAssembler(directory).Assemble();

			Assert.Equal(1, generatedDocs.Count);
			var generatedDoc = generatedDocs.Values.First();

			Assert.Equal("_design/doc1", generatedDoc.Id);
			Assert.Null(generatedDoc.Revision);
			Assert.Equal(
				JObject.Parse(@"{ ""_id"": ""_design/doc1"", ""prop1"": ""prop1 value"", ""prop2"": ""prop2 value"" }"),
				generatedDoc.Definition,
				new JTokenEqualityComparer());
		}

		[Fact]
		public void ShouldDetectClashingFilesWithDifferentExtensionsAndThrow()
		{
			var directory = Mock.Of<IDirectory>(
				d => d.EnumerateDirectories() == new[] {
					Mock.Of<IDirectory>(
						d1 =>
							d1.EnumerateFiles() == new[] { File("prop.txt"), File("prop.js") } &&
							d1.EnumerateDirectories() == EmptyDirectoryArray &&
							d1.Name == "doc1"
						)
				}
			);

			Assert.Throws<DesignDocumentAssemblerException>(
				() =>new DesignDocumentAssembler(directory).Assemble());
		}

		[Fact]
		public void ShouldDetectClashingFileAndDirectoryAndThrow()
		{
			var directory = Mock.Of<IDirectory>(
				d => d.EnumerateDirectories() == new[] {
					Mock.Of<IDirectory>(
						d1 =>
							d1.EnumerateFiles() == new[] { File("prop.txt") } &&
							d1.Name == "doc1" &&
							d1.EnumerateDirectories() == new [] {
								Mock.Of<IDirectory>(d2 =>	d2.Name == "prop")
							}
						)
				}
			);

			Assert.Throws<DesignDocumentAssemblerException>(
				() =>new DesignDocumentAssembler(directory).Assemble());
		}

		[Theory]
		[InlineData("_rev")]
		[InlineData("_id")]
		public void ShouldDetectReservedDirectoriesAndThrow(string directoryName)
		{
			var directory = Mock.Of<IDirectory>(
				d => d.EnumerateDirectories() == new[] {
					Mock.Of<IDirectory>(
						d1 =>
							d1.EnumerateFiles() == EmptyFileArray &&
							d1.Name == "doc1" &&
							d1.EnumerateDirectories() == new [] {
								Mock.Of<IDirectory>(d2 =>	d2.Name == directoryName)
							}
						)
				}
			);

			Assert.Throws<DesignDocumentAssemblerException>(
				() =>new DesignDocumentAssembler(directory).Assemble());
		}

		[Fact]
		public void ShouldDetectReservedFilesAndThrow()
		{
			var directory = Mock.Of<IDirectory>(
				d => d.EnumerateDirectories() == new[] {
					Mock.Of<IDirectory>(
						d1 =>
							d1.EnumerateFiles() == new[] { File("_rev") }  &&
							d1.Name == "doc1" &&
							d1.EnumerateDirectories() == EmptyDirectoryArray
						)
				}
			);

			Assert.Throws<DesignDocumentAssemblerException>(
				() =>new DesignDocumentAssembler(directory).Assemble());
		}

		[Fact]
		public void ShouldMapTextFilesInSubDirectoriesToSubobjectProperties()
		{
			var directory = Mock.Of<IDirectory>(
				d => d.EnumerateDirectories() == new[] {
					Mock.Of<IDirectory>(
						d1 =>
							d1.EnumerateFiles() == EmptyFileArray &&
							d1.Name == "doc1" &&
							d1.EnumerateDirectories() == new [] {
								Mock.Of<IDirectory>(
									d2 => 
										d2.EnumerateFiles() == EmptyFileArray &&
										d2.Name == "interObj1" &&
										d2.EnumerateDirectories() == new [] {
											Mock.Of<IDirectory>(
												d3 => 
													d3.EnumerateFiles() == EmptyFileArray &&
													d3.Name == "interObj2" &&
													d3.EnumerateDirectories() == new [] {
															Mock.Of<IDirectory>(
																d4 => 
																	d4.Name == "interObj3" &&
																	d4.EnumerateFiles() == 
																		new[] { 
																			File("prop1.txt", "prop1 value"), 
																			File("prop2.js", "prop2 value")
																		}
															)
													}
											)
										}
								)
							} 
						)
				}
			);

			var generatedDocs = new DesignDocumentAssembler(directory).Assemble();
			var generatedDoc = generatedDocs.Values.First();


			Assert.Equal(1, generatedDocs.Count);
			Assert.Equal("_design/doc1", generatedDoc.Id);
			Assert.Null(generatedDoc.Revision);
			
			Assert.Equal(
				JObject.Parse(@"{ 
					""_id"": ""_design/doc1"", 
					""interObj1"": {
						""interObj2"": {
							""interObj3"": {
								""prop1"": ""prop1 value"", 
								""prop2"": ""prop2 value"" 
							}
						}
					}
				}"),
				generatedDoc.Definition,
				new JTokenEqualityComparer());
		}

		private static IFile File(string name)
		{
			return File(name, string.Empty);
		}

		private static IFile File(string name, string content)
		{
			return Mock.Of<IFile>(f =>
				f.Name == name && f.OpenRead() == new MemoryStream(Encoding.UTF8.GetBytes(content)));
		}
	}
}