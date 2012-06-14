using System;
using System.Net.Http.Headers;
using System.Security;
using System.Text;

namespace CouchDude
{
	/// <summary>CouchDB access credentials.</summary>
	public class Credentials
	{
		/// <summary>Access user name.</summary>
		public readonly string UserName;

		/// <summary>Access password.</summary>
		public readonly SecureString Password;

		AuthenticationHeaderValue authenticationHeader;

		/// <constructor />
		public Credentials(string userName, SecureString password)
		{
			UserName = userName;
			Password = password;
		}

		internal AuthenticationHeaderValue ToAuthenticationHeader()
		{
			if (authenticationHeader == null)
			{
				var credentials = string.Concat(UserName, ":", Password);
				var credentialsBytes = Encoding.UTF8.GetBytes(credentials);
				var base64Credentials = Convert.ToBase64String(credentialsBytes);
				authenticationHeader = new AuthenticationHeaderValue("Basic", base64Credentials);
			}

			return authenticationHeader;
		}
	}
}