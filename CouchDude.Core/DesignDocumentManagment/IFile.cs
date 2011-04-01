using System.Diagnostics.Contracts;
using System.IO;

namespace CouchDude.Core.DesignDocumentManagment
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