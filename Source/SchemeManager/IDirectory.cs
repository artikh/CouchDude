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
using System.Diagnostics.Contracts;


namespace CouchDude.SchemeManager
{
	/// <summary>Abstraction over simple directory operations.</summary>
	[ContractClass(typeof(DirectoryContracts))]
	public interface IDirectory
	{
		/// <summary>Return all file paths in directory.</summary>
		IEnumerable<IFile> EnumerateFiles();

		/// <summary>Return all subdirectories in directory.</summary>
		IEnumerable<IDirectory> EnumerateDirectories();

		/// <summary>Name of the directory.</summary>
		string Name { get; }
	}

	/// <summary>Contract for <see cref="IDirectory"/>.</summary>
	/// <summary>Contract for <see cref="IDirectory"/>.</summary>
	[ContractClassFor(typeof(IDirectory))]
	public abstract class DirectoryContracts: IDirectory 
	{

		IEnumerable<IFile> IDirectory.EnumerateFiles()
		{
			Contract.Ensures(Contract.Result<IEnumerable<IFile>>() != null);
			return null;
		}

		IEnumerable<IDirectory> IDirectory.EnumerateDirectories()
		{
			Contract.Ensures(Contract.Result<IEnumerable<IDirectory>>() != null);
			return null;
		}

		string IDirectory.Name
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);
				return null;
			}
		}
	}
}
