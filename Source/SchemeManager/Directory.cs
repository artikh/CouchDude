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
using System.Collections.Generic;

using System.IO;
using System.Linq;

namespace CouchDude.SchemeManager
{
	/// <summary>Wraps <see cref="DirectoryInfo"/> with <see cref="IDirectory"/> 
	/// interface.</summary>
	public class Directory: IDirectory
	{
		private readonly DirectoryInfo directoryInfo;

		/// <constructor />
		public Directory(DirectoryInfo directoryInfo)
		{			
			if(directoryInfo == null) throw new ArgumentNullException("directoryInfo");
			if(!directoryInfo.Exists) 
				throw new ArgumentException("Directory existance required.", "directoryInfo");
			

			this.directoryInfo = directoryInfo;
		}

		/// <inheritdoc/>
		public string Name { get { return directoryInfo.Name; } }

		/// <inheritdoc/>
		public IEnumerable<IFile> EnumerateFiles()
		{
			return directoryInfo.EnumerateFiles().Select(fi => new File(fi));
		}

		/// <inheritdoc/>
		public IEnumerable<IDirectory> EnumerateDirectories()
		{
			return directoryInfo.EnumerateDirectories().Select(di => new Directory(di));
		}
	}
}