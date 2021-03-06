namespace RE
{
	/// <summary>
	/// Represents a binary expression
	/// </summary>
	public abstract class RegexBinaryExpression : RegexExpression
	{
		/// <summary>
		/// Indicates the left hand expression
		/// </summary>
		public RegexExpression Left { get; set; }
		/// <summary>
		/// Indicates the right hand expression
		/// </summary>
		public RegexExpression Right { get; set; }
	}
}
