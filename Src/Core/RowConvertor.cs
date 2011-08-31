namespace CouchDude
{
	/// <summary>Represents row conversion method.</summary>
	public delegate T RowConvertor<out T, in TRow>(TRow row) where TRow : IQueryResultRow;
}