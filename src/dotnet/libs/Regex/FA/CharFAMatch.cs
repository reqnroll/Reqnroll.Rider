namespace RE
{
	/// <summary>
	/// Represents a regular expression match
	/// </summary>
	/// <remarks>Returned from the Match() and MatchDfa() methods</remarks>
	public sealed class CharFAMatch
	{
		/// <summary>
		/// Indicates the 1 based line where the match was found
		/// </summary>
		public int Line { get; }
		/// <summary>
		/// Indicates the 1 based column where the match was found
		/// </summary>
		public int Column { get; }
		/// <summary>
		/// Indicates the 0 based position where the match was found
		/// </summary>
		public long Position { get; }
		/// <summary>
		/// Indicates the value of the match
		/// </summary>
		public string Value { get; }
		/// <summary>
		/// Creates a new instance with the specified values
		/// </summary>
		/// <param name="line">The 1 based line where the match occured</param>
		/// <param name="column">The 1 based columns where the match occured</param>
		/// <param name="position">The 0 based position where the match occured</param>
		/// <param name="value">The value of the match</param>
		public CharFAMatch(int line,int column,long position,string value)
		{
			Line = line;
			Column = column;
			Position = position;
			Value = value;
		}
	}
}
