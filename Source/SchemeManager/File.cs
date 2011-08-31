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

using System.IO;

namespace CouchDude.SchemeManager
{
	/// <summary>Wraps <see cref="FileInfo"/> with <see cref="IFile"/> 
	/// interface.</summary>
	public class File: IFile
	{
		private readonly FileInfo fileInfo;

		/// <constructor />
		public File(FileInfo fileInfo)
		{
			this.fileInfo = fileInfo;
		}

		/// <inheritdoc/>
		public Stream OpenRead() { return fileInfo.OpenRead(); }

		/// <inheritdoc/>
		public string Name { get { return fileInfo.Name; } }
	}
}