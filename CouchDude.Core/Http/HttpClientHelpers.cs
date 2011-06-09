﻿using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.Text;

namespace CouchDude.Core.Http
{
	/// <summary>Extension methods for <see cref="System.Net.Http"/> API.</summary>
	internal static class HttpClientHelpers
	{
		private readonly static Encoding DefaultHttpEncoding = Encoding.GetEncoding(0x6faf);

		/// <summary>Constructs text reader over message content using response's encoding info.</summary>
		public static TextReader GetContentTextReader(this HttpResponseMessage self)
		{
			if (self == null) throw new ArgumentNullException("self");
			Contract.EndContractBlock();

			return self.Content.GetTextReader();
		}
		
		/// <summary>Populates request message with text data form provided string.</summary>
		public static void WriteContentText(this HttpRequestMessage self, string contentTextString)
		{
			if (self == null) throw new ArgumentNullException("self");
			Contract.EndContractBlock();

			self.Content = new StringContent(contentTextString);
		}
		
		/// <summary>Constructs text reader over HTTP content using response's encoding info.</summary>
		public static TextReader GetTextReader(this HttpContent self)
		{
			if (self == null)
				return null;

			var encoding = GetContentEncoding(self);
			return new StreamReader(self.ContentReadStream, encoding);
		}

		private static Encoding GetContentEncoding(HttpContent httpContent)
		{
			var encoding = DefaultHttpEncoding;
			var contentType = httpContent.Headers.ContentType;
			if (contentType != null && contentType.CharSet != null)
			{
				try
				{
					encoding = Encoding.GetEncoding(contentType.CharSet);
				}
				catch (ArgumentException exception)
				{
					throw new InvalidOperationException(
						"The character set provided in ContentType is invalid. Cannot read content as string using an invalid character set.",
						exception);
				}
			}
			return encoding;
		}
	}
}