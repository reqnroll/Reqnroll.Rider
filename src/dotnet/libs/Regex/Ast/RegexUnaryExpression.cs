using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	/// <summary>
	/// Represents an expression with a single target expression
	/// </summary>
	public abstract class RegexUnaryExpression : RegexExpression
	{
		/// <summary>
		/// Indicates the target expression
		/// </summary>
		public RegexExpression Expression { get; set; }

	}
}
