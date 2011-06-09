using System;
using System.Text;

namespace CouchDude.Core.Configuration
{
	/// <summary>Generates random IDs using <see cref="Guid.NewGuid"/> method.</summary>
	public class GuidIdGenerator: IIdGenerator
	{
		/// <inheritdoc/>
		public string GenerateId()
		{
			var bytes = Guid.NewGuid().ToByteArray();
			var id = new StringBuilder(Convert.ToBase64String(bytes));
			for (var i = 0; i < id.Length; i++)
				switch (id[i])
				{
					case '+':
						id[i] = '_';
						break;
					case '/':
						id[i] = '-';
						break;
				}
			return id.ToString();
		}
	}
}