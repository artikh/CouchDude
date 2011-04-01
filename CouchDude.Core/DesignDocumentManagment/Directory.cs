using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace CouchDude.Core.DesignDocumentManagment
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
			Contract.EndContractBlock();

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