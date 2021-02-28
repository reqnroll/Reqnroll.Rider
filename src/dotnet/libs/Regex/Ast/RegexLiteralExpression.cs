using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	/// <summary>
	/// Represents a single character literal
	/// </summary>
	public class RegexLiteralExpression : RegexExpression, IEquatable<RegexLiteralExpression>
	{
		/// <summary>
		/// Indicates whether or not this statement is a single element or not
		/// </summary>
		/// <remarks>If false, this statement will be wrapped in parentheses if necessary</remarks>
		public override bool IsSingleElement => true;
		/// <summary>
		/// Indicates the character literal of this expression
		/// </summary>
		public char Value { get; set; } = default(char);
		/// <summary>
		/// Creates a series of concatenated literals representing the specified string
		/// </summary>
		/// <param name="value">The string to use</param>
		/// <returns>An expression representing <paramref name="value"/></returns>
		public static RegexExpression CreateString(string value)
		{
			if (string.IsNullOrEmpty(value))
				return null;
			RegexExpression result = new RegexLiteralExpression(value[0]);
			for (var i = 1; i < value.Length; i++)
				result = new RegexConcatExpression(result, new RegexLiteralExpression(value[i]));
			return result;
		}
		/// <summary>
		/// Creates a literal expression with the specified character
		/// </summary>
		/// <param name="value">The character to represent</param>
		public RegexLiteralExpression(char value) { Value = value; }
		/// <summary>
		/// Creates a default instance of the expression
		/// </summary>
		public RegexLiteralExpression() { }
		/// <summary>
		/// Creates a state machine representing this expression
		/// </summary>
		/// <typeparam name="TAccept">The type of accept symbol to use for this expression</typeparam>
		/// <param name="accept">The accept symbol to use for this expression</param>
		/// <returns>A new <see cref="CharFA{TAccept}"/> finite state machine representing this expression</returns>
		public override CharFA<TAccept> ToFA<TAccept>(TAccept accept)
			=>CharFA<TAccept>.Literal(new char[] { Value }, accept);
		/// <summary>
		/// Appends the textual representation to a <see cref="StringBuilder"/>
		/// </summary>
		/// <param name="sb">The string builder to use</param>
		/// <remarks>Used by ToString()</remarks>
		protected internal override void AppendTo(StringBuilder sb)
			=>AppendEscapedChar(Value,sb);
		/// <summary>
		/// Creates a new copy of this expression
		/// </summary>
		/// <returns>A new copy of this expression</returns>
		protected override RegexExpression CloneImpl()
			=> Clone();
		/// <summary>
		/// Creates a new copy of this expression
		/// </summary>
		/// <returns>A new copy of this expression</returns>
		public RegexLiteralExpression Clone()
		{
			return new RegexLiteralExpression(Value);
		}

		#region Value semantics
		/// <summary>
		/// Indicates whether this expression is the same as the right hand expression
		/// </summary>
		/// <param name="rhs">The expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public bool Equals(RegexLiteralExpression rhs)
		{
			if (ReferenceEquals(rhs, this)) return true;
			if (ReferenceEquals(rhs, null)) return false;
			return Value == rhs.Value;
		}
		/// <summary>
		/// Indicates whether this expression is the same as the right hand expression
		/// </summary>
		/// <param name="rhs">The expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public override bool Equals(object rhs)
			=> Equals(rhs as RegexLiteralExpression);
		/// <summary>
		/// Computes a hash code for this expression
		/// </summary>
		/// <returns>A hash code for this expression</returns>
		public override int GetHashCode()
			=> Value.GetHashCode();
		/// <summary>
		/// Indicates whether or not two expression are the same
		/// </summary>
		/// <param name="lhs">The left hand expression to compare</param>
		/// <param name="rhs">The right hand expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public static bool operator ==(RegexLiteralExpression lhs, RegexLiteralExpression rhs)
		{
			if (ReferenceEquals(lhs, rhs)) return true;
			if (ReferenceEquals(lhs, null)) return false;
			return lhs.Equals(rhs);
		}
		/// <summary>
		/// Indicates whether or not two expression are different
		/// </summary>
		/// <param name="lhs">The left hand expression to compare</param>
		/// <param name="rhs">The right hand expression to compare</param>
		/// <returns>True if the expressions are different, otherwise false</returns>
		public static bool operator !=(RegexLiteralExpression lhs, RegexLiteralExpression rhs)
		{
			if (ReferenceEquals(lhs, rhs)) return false;
			if (ReferenceEquals(lhs, null)) return true;
			return !lhs.Equals(rhs);
		}
		#endregion

	}
}
