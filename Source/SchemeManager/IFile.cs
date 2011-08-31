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

using System.Diagnostics.Contracts;
using System.IO;

namespace CouchDude.SchemeManager
{
	/// <summary>Abstraction over simple file operations.</summary>
	[ContractClass(typeof(FileContracts))]
	public interface IFile
	{
		/// <summary>Opens file stream for read.</summary>
		Stream OpenRead();

		/// <summary>Name of the file.</summary>
		string Name { get; }
	}

	/// <summary>Contract for <see cref="IFile"/>.</summary>
	[ContractClassFor(typeof(IFile))]
	public abstract class FileContracts : IFile
	{
		Stream IFile.OpenRead()
		{
			Contract.Ensures(Contract.Result<Stream>() != null);
			return null;
		}


		string IFile.Name
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);
				return null;
			}
		}
	}

}