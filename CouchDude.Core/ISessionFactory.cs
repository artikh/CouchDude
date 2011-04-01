namespace CouchDude.Core
{
	/// <summary>Factory object for <see cref="ISession"/> object.</summary>
	public interface ISessionFactory
	{
		/// <summary>Creates session.</summary>
		ISession CreateSession();
	}
}