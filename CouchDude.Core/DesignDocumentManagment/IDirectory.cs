using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace CouchDude.Core.DesignDocumentManagment
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
