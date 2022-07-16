using System;
using System.Text;

namespace RE
{
	/// <summary>
	/// Represents an optional expression, as indicated by ?
	/// </summary>
	public class RegexOptionalExpression : RegexUnaryExpression, IEquatable<RegexOptionalExpression>
	{
		/// <summary>
		/// Indicates whether or not this statement is a single element or not
		/// </summary>
		/// <remarks>If false, this statement will be wrapped in parentheses if necessary</remarks>
		public override bool IsSingleElement => true;
		/// <summary>
		/// Creates an optional expression using the specified target expression
		/// </summary>
		/// <param name="expression">The target expression to make optional</param>
		public RegexOptionalExpression(RegexExpression expression) { Expression = expression; }
		/// <summary>
		/// Creates a default instance of the expression
		/// </summary>
		public RegexOptionalExpression() { }
		/// <summary>
		/// Creates a state machine representing this expression
		/// </summary>
		/// <typeparam name="TAccept">The type of accept symbol to use for this expression</typeparam>
		/// <param name="accept">The accept symbol to use for this expression</param>
		/// <returns>A new <see cref="CharFA{TAccept}"/> finite state machine representing this expression</returns>
		public override CharFA<TAccept> ToFA<TAccept>(TAccept accept)
			=>null != Expression ? CharFA<TAccept>.Optional(Expression.ToFA(accept),accept) : null;
		/// <summary>
		/// Appends the textual representation to a <see cref="StringBuilder"/>
		/// </summary>
		/// <param name="sb">The string builder to use</param>
		/// <remarks>Used by ToString()</remarks>
		protected internal override void AppendTo(StringBuilder sb)
		{
			if (null == Expression)
				sb.Append("()?");
			else
			{
				var ise = Expression.IsSingleElement;
				if (!ise)
					sb.Append('(');
				Expression.AppendTo(sb);
				if (!ise)
					sb.Append(")?");
				else
					sb.Append('?');
			}
		}
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
		public RegexOptionalExpression Clone()
		{
			return new RegexOptionalExpression(Expression);
		}
		#region Value semantics
		/// <summary>
		/// Indicates whether this expression is the same as the right hand expression
		/// </summary>
		/// <param name="rhs">The expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public bool Equals(RegexOptionalExpression rhs)
		{
			if (ReferenceEquals(rhs, this)) return true;
			if (ReferenceEquals(rhs, null)) return false;
			return Equals(Expression, rhs.Expression);
		}
		/// <summary>
		/// Indicates whether this expression is the same as the right hand expression
		/// </summary>
		/// <param name="rhs">The expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public override bool Equals(object rhs)
			=> Equals(rhs as RegexOptionalExpression);
		/// <summary>
		/// Computes a hash code for this expression
		/// </summary>
		/// <returns>A hash code for this expression</returns>
		public override int GetHashCode()
		{
			if (null != Expression)
				return Expression.GetHashCode();
			return 0;
		}
		/// <summary>
		/// Indicates whether or not two expression are the same
		/// </summary>
		/// <param name="lhs">The left hand expression to compare</param>
		/// <param name="rhs">The right hand expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public static bool operator ==(RegexOptionalExpression lhs, RegexOptionalExpression rhs)
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
		public static bool operator !=(RegexOptionalExpression lhs, RegexOptionalExpression rhs)
		{
			if (ReferenceEquals(lhs, rhs)) return false;
			if (ReferenceEquals(lhs, null)) return true;
			return !lhs.Equals(rhs);
		}
		#endregion
	}
}
