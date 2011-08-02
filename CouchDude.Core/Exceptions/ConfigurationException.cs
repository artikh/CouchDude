﻿// <autogenerated>
// This code was generated by a tool. Any changes made manually will be lost
// the next time this code is regenerated.
// </autogenerated>

using System;
using System.Runtime.Serialization;

namespace CouchDude.Core
{
	/// <summary> Configuration exception.</summary>
	[Serializable]
	public partial class ConfigurationException: CouchDudeException
	{
		/// <summary>Initializes a new instance of the 
		/// <see cref="ConfigurationException" /> class.</summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the 
		/// serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains 
		/// contextual information about the source or destination.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="info"/> 
		/// parameter is null. </exception>
		/// <exception cref="SerializationException">The class name is null or 
		/// <see cref="Exception.HResult"/> is zero (0). </exception>
		public ConfigurationException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <summary>Initializes a new instance of the <see cref="ConfigurationException" /> class.</summary>
		/// <param name="message">The message.</param>
		public ConfigurationException(string message): base(message) { }

		/// <summary>Initializes a new instance of the <see cref="ConfigurationException" /> class.</summary>
		/// <param name="messageTemplate">The message template.</param>
		/// <param name="messageParams">The message params.</param>
		[JetBrains.Annotations.StringFormatMethod("messageTemplate")]
		public ConfigurationException(string messageTemplate, params object[] messageParams)
			: base(String.Format(messageTemplate, messageParams)) { }

		/// <summary>Initializes a new instance of the 
		/// <see cref="ConfigurationException" /> class.</summary>
		/// <param name="innerException">The inner exception.</param>
		/// <param name="messageTemplate">The message template.</param>
		/// <param name="messageParams">The message params.</param>
		[JetBrains.Annotations.StringFormatMethod("messageTemplate")]
		public ConfigurationException(
			Exception innerException,
			string messageTemplate,
			params object[] messageParams)
			: base(AddInnerExceptionMessage(String.Format(messageTemplate, messageParams), innerException), innerException) { }

		/// <summary>Initializes a new instance of the 
		/// <see cref="ConfigurationException" /> class.</summary>
		/// <param name="innerException">The inner exception.</param>
		/// <param name="message">The message.</param>
		[JetBrains.Annotations.StringFormatMethod("message")]
		public ConfigurationException(Exception innerException, string message)
			: base(AddInnerExceptionMessage(message, innerException), innerException) { }
			
		private static string AddInnerExceptionMessage(string message, Exception innerException)
		{
			return string.Concat(message, ": ", innerException.Message);
		}
	}
}


