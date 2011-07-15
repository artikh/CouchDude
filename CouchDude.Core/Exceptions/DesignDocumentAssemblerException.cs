﻿// <autogenerated>
// This code was generated by a tool. Any changes made manually will be lost
// the next time this code is regenerated.
// </autogenerated>

using System;
using System.Runtime.Serialization;

namespace CouchDude.Core
{
	/// <summary> DesignDocumentAssembler exception.</summary>
	[Serializable]
	public partial class DesignDocumentAssemblerException: CouchDudeException
	{
		/// <summary>Initializes a new instance of the 
		/// <see cref="DesignDocumentAssemblerException" /> class.</summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the 
		/// serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains 
		/// contextual information about the source or destination.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="info"/> 
		/// parameter is null. </exception>
		/// <exception cref="SerializationException">The class name is null or 
		/// <see cref="Exception.HResult"/> is zero (0). </exception>
		public DesignDocumentAssemblerException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <summary>Initializes a new instance of the <see cref="DesignDocumentAssemblerException" /> class.</summary>
		/// <param name="message">The message.</param>
		/// <param name="messageParams">The message params.</param>
		[JetBrains.Annotations.StringFormatMethod("message")]
		public DesignDocumentAssemblerException(string message, params object[] messageParams)
			: this(null, message, messageParams) { }

		/// <summary>Initializes a new instance of the 
		/// <see cref="DesignDocumentAssemblerException" /> class.</summary>
		/// <param name="innerException">The inner exception.</param>
		/// <param name="message">The message.</param>
		/// <param name="messageParams">The message params.</param>
		[JetBrains.Annotations.StringFormatMethod("message")]
		public DesignDocumentAssemblerException(
			Exception innerException,
			string message,
			params object[] messageParams)
			: base(messageParams.Length > 0? String.Format(message, messageParams): message, innerException) { }
	}
}


