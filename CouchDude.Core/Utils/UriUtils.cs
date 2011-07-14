using System;
using System.Reflection;

namespace CouchDude.Core.Utils
{
	/// <summary><see cref="Uri"/>-related utility class.</summary>
	public static class UriUtils
	{
		// System.UriSyntaxFlags is internal, so let's duplicate the flag privately
		private const int UnEscapeDotsAndSlashes = 0x2000000;
		private const int SimpleUserSyntax = 0x20000;

		/// <summary>Reverts default <see cref="Uri"/> behaviour of unescaping slashes and dots in path.</summary>
		public static Uri LeaveDotsAndSlashesEscaped(this Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			FieldInfo fieldInfo = uri.GetType().GetField("m_Syntax", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
				throw new MissingFieldException("'m_Syntax' field not found");

			object uriParser = fieldInfo.GetValue(uri);
			fieldInfo = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
				throw new MissingFieldException("'m_Flags' field not found");

			object uriSyntaxFlags = fieldInfo.GetValue(uriParser);

			// Clear the flag that we don't want
			uriSyntaxFlags = (int)uriSyntaxFlags & ~UnEscapeDotsAndSlashes;
			uriSyntaxFlags = (int)uriSyntaxFlags & ~SimpleUserSyntax;
			fieldInfo.SetValue(uriParser, uriSyntaxFlags);

			return uri;
		}
	}
}