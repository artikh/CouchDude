using System.IO;

namespace CouchDude.Core.DesignDocumentManagment
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