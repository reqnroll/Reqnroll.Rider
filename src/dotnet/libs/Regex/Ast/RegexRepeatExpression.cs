using System;
using System.Text;

namespace RE
{
	/// <summary>
	/// Represents a repeat regular expression as indicated by *, +, or {min,max}
	/// </summary>
	public class RegexRepeatExpression : RegexUnaryExpression, IEquatable<RegexRepeatExpression>
	{
		/// <summary>
		/// Indicates whether or not this statement is a single element or not
		/// </summary>
		/// <remarks>If false, this statement will be wrapped in parentheses if necessary</remarks>
		public override bool IsSingleElement => true;
		/// <summary>
		/// Creates a repeat expression with the specifed target expression, and minimum and maximum occurances
		/// </summary>
		/// <param name="expression">The target expression</param>
		/// <param name="minOccurs">The minimum number of times the target expression can occur or -1</param>
		/// <param name="maxOccurs">The maximum number of times the target expression can occur or -1</param>
		public RegexRepeatExpression(RegexExpression expression,int minOccurs=-1,int maxOccurs=-1)
		{
			Expression = expression;
			MinOccurs = minOccurs;
			MaxOccurs = maxOccurs;
		}
		/// <summary>
		/// Creates a default instance of the expression
		/// </summary>
		public RegexRepeatExpression() { }
		/// <summary>
		/// Indicates the minimum number of times the target expression can occur, or 0 or -1 for no minimum
		/// </summary>
		public int MinOccurs { get; set; } = -1;
		/// <summary>
		/// Indicates the maximum number of times the target expression can occur, or 0 or -1 for no maximum
		/// </summary>
		public int MaxOccurs { get; set; } = -1; // kleene by default
		/// <summary>
		/// Creates a state machine representing this expression
		/// </summary>
		/// <typeparam name="TAccept">The type of accept symbol to use for this expression</typeparam>
		/// <param name="accept">The accept symbol to use for this expression</param>
		/// <returns>A new <see cref="CharFA{TAccept}"/> finite state machine representing this expression</returns>		
		public override CharFA<TAccept> ToFA<TAccept>(TAccept accept)
			=> null != Expression ? CharFA<TAccept>.Repeat(Expression.ToFA(accept),MinOccurs,MaxOccurs, accept) : null;
		/// <summary>
		/// Appends the textual representation to a <see cref="StringBuilder"/>
		/// </summary>
		/// <param name="sb">The string builder to use</param>
		/// <remarks>Used by ToString()</remarks>
		protected internal override void AppendTo(StringBuilder sb)
		{
			var ise = null!=Expression && Expression.IsSingleElement;
			if (!ise)
				sb.Append('(');
			if(null!=Expression)
				Expression.AppendTo(sb);
			if (!ise)
				sb.Append(')');

			switch (MinOccurs)
			{
				case -1:
				case 0:
					switch(MaxOccurs)
					{
						case -1:
						case 0:
							sb.Append('*');
							break;
						default:
							sb.Append('{');
							if (-1 != MinOccurs)
								sb.Append(MinOccurs);
							sb.Append(',');
							sb.Append(MaxOccurs);
							sb.Append('}');
							break;
					}
					break;
				case 1:
					switch (MaxOccurs)
					{
						case -1:
						case 0:
							sb.Append('+');
							break;
						default:
							sb.Append("{1,");
							sb.Append(MaxOccurs);
							sb.Append('}');
							break;
					}
					break;
				default:
					sb.Append('{');
					if (-1 != MinOccurs)
						sb.Append(MinOccurs);
					sb.Append(',');
					if (-1 != MaxOccurs)
						sb.Append(MaxOccurs);
					sb.Append('}');
					break;
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
		public RegexRepeatExpression Clone()
		{
			return new RegexRepeatExpression(Expression,MinOccurs,MaxOccurs);
		}
		#region Value semantics
		/// <summary>
		/// Indicates whether this expression is the same as the right hand expression
		/// </summary>
		/// <param name="rhs">The expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public bool Equals(RegexRepeatExpression rhs)
		{
			if (ReferenceEquals(rhs, this)) return true;
			if (ReferenceEquals(rhs, null)) return false;
			if(Equals(Expression, rhs.Expression))
			{
				var lmio = Math.Max(0, MinOccurs);
				var lmao = Math.Max(0, MaxOccurs);
				var rmio = Math.Max(0, rhs.MinOccurs);
				var rmao = Math.Max(0, rhs.MaxOccurs);
				return lmio == rmio && lmao == rmao;
			}
			return false;
		}
		/// <summary>
		/// Indicates whether this expression is the same as the right hand expression
		/// </summary>
		/// <param name="rhs">The expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public override bool Equals(object rhs)
			=> Equals(rhs as RegexRepeatExpression);
		/// <summary>
		/// Computes a hash code for this expression
		/// </summary>
		/// <returns>A hash code for this expression</returns>
		public override int GetHashCode()
		{
			var result = MinOccurs ^ MaxOccurs;
			if (null != Expression)
				return result ^ Expression.GetHashCode();
			return result;
		}
		/// <summary>
		/// Indicates whether or not two expression are the same
		/// </summary>
		/// <param name="lhs">The left hand expression to compare</param>
		/// <param name="rhs">The right hand expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public static bool operator ==(RegexRepeatExpression lhs, RegexRepeatExpression rhs)
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
		public static bool operator !=(RegexRepeatExpression lhs, RegexRepeatExpression rhs)
		{
			if (ReferenceEquals(lhs, rhs)) return false;
			if (ReferenceEquals(lhs, null)) return true;
			return !lhs.Equals(rhs);
		}
		#endregion
	}
}
