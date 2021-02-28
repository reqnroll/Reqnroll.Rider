using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	/// <summary>
	/// Indicates a charset expression
	/// </summary>
	/// <remarks>Represented by [] in regular expression syntax</remarks>
	public class RegexCharsetExpression : RegexExpression, IEquatable<RegexCharsetExpression>
	{
		/// <summary>
		/// Indicates the <see cref="RegexCharsetEntry"/> entries in the character set
		/// </summary>
		public IList<RegexCharsetEntry> Entries { get; } = new List<RegexCharsetEntry>();
		/// <summary>
		/// Creates a new charset expression with the specified entries and optionally negated
		/// </summary>
		/// <param name="entries">The entries to initialize the charset with</param>
		/// <param name="hasNegatedRanges">True if the range is a "not range" like [^], otherwise false</param>
		public RegexCharsetExpression(IEnumerable<RegexCharsetEntry> entries,bool hasNegatedRanges=false)
		{
			foreach (var entry in entries)
				Entries.Add(entry);
			HasNegatedRanges = hasNegatedRanges;
		}
		/// <summary>
		/// Creates a default instance of the expression
		/// </summary>
		public RegexCharsetExpression() { }
		/// <summary>
		/// Creates a state machine representing this expression
		/// </summary>
		/// <typeparam name="TAccept">The type of accept symbol to use for this expression</typeparam>
		/// <param name="accept">The accept symbol to use for this expression</param>
		/// <returns>A new <see cref="CharFA{TAccept}"/> finite state machine representing this expression</returns>
		public override CharFA<TAccept> ToFA<TAccept>(TAccept accept)
		{
			var ranges = new List<CharRange>();
			for(int ic=Entries.Count,i=0;i<ic;++i)
			{
				var entry = Entries[i];
				var crc = entry as RegexCharsetCharEntry;
				if(null!=crc)
					ranges.Add(new CharRange(crc.Value,crc.Value));
				var crr = entry as RegexCharsetRangeEntry;
				if (null != crr)
					ranges.Add(new CharRange(crr.First, crr.Last));
				var crcl = entry as RegexCharsetClassEntry;
				if (null != crcl)
					ranges.AddRange(CharFA<TAccept>.CharacterClasses[crcl.Name]);
			}
			if (HasNegatedRanges)
				return CharFA<TAccept>.Set(CharRange.NotRanges(ranges), accept);
			return CharFA<TAccept>.Set(ranges,accept);
		}
		/// <summary>
		/// Indicates whether the range is a "not range"
		/// </summary>
		/// <remarks>This is represented by the [^] regular expression syntax</remarks>
		public bool HasNegatedRanges { get; set; } = false;
		/// <summary>
		/// Indicates whether or not this statement is a single element or not
		/// </summary>
		/// <remarks>If false, this statement will be wrapped in parentheses if necessary</remarks>
		public override bool IsSingleElement => true;
		/// <summary>
		/// Appends the textual representation to a <see cref="StringBuilder"/>
		/// </summary>
		/// <param name="sb">The string builder to use</param>
		/// <remarks>Used by ToString()</remarks>
		protected internal override void AppendTo(StringBuilder sb)
		{
			// special case for "."
			if(1==Entries.Count)
			{
				var dotE = Entries[0] as RegexCharsetRangeEntry;
				if(!HasNegatedRanges && null !=dotE && dotE.First==char.MinValue && dotE.Last==char.MaxValue)
				{
					sb.Append(".");
					return;
				}
				var cls = Entries[0] as RegexCharsetClassEntry;
				if (null != cls)
				{
					switch (cls.Name)
					{
						case "blank":
							if (!HasNegatedRanges)
								sb.Append(@"\h");
							return;
						case "digit":
							if (!HasNegatedRanges)
								sb.Append(@"\d");
							else
								sb.Append(@"\D");
							return;
						case "lower":
							if (!HasNegatedRanges)
								sb.Append(@"\l");
							return;
						case "space":
							if (!HasNegatedRanges)
								sb.Append(@"\s");
							else
								sb.Append(@"\S");
							return;
						case "upper":
							if (!HasNegatedRanges)
								sb.Append(@"\u");
							return;
						case "word":
							if (!HasNegatedRanges)
								sb.Append(@"\w");
							else
								sb.Append(@"\W");
							return;

					}
				}
			}
			
			sb.Append('[');
			if (HasNegatedRanges)
				sb.Append('^');
			for (int ic = Entries.Count, i = 0; i < ic; ++i)
				sb.Append(Entries[i]);
			sb.Append(']');
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
		public RegexCharsetExpression Clone()
		{
			return new RegexCharsetExpression(Entries, HasNegatedRanges);
		}
		#region Value semantics
		/// <summary>
		/// Indicates whether this expression is the same as the right hand expression
		/// </summary>
		/// <param name="rhs">The expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public bool Equals(RegexCharsetExpression rhs)
		{
			if (ReferenceEquals(rhs, this)) return true;
			if (ReferenceEquals(rhs, null)) return false;
			if(HasNegatedRanges==rhs.HasNegatedRanges && rhs.Entries.Count==Entries.Count)
			{
				for (int ic = Entries.Count, i = 0; i < ic; ++i)
					if (Entries[i] != rhs.Entries[i])
						return false;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Indicates whether this expression is the same as the right hand expression
		/// </summary>
		/// <param name="rhs">The expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public override bool Equals(object rhs)
			=> Equals(rhs as RegexCharsetExpression);
		/// <summary>
		/// Computes a hash code for this expression
		/// </summary>
		/// <returns>A hash code for this expression</returns>
		public override int GetHashCode()
		{
			var result = HasNegatedRanges.GetHashCode();
			for (int ic = Entries.Count, i = 0; i < ic; ++i)
				result ^= Entries[i].GetHashCode();
			return result;	
		}
		/// <summary>
		/// Indicates whether or not two expression are the same
		/// </summary>
		/// <param name="lhs">The left hand expression to compare</param>
		/// <param name="rhs">The right hand expression to compare</param>
		/// <returns>True if the expressions are the same, otherwise false</returns>
		public static bool operator ==(RegexCharsetExpression lhs, RegexCharsetExpression rhs)
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
		public static bool operator !=(RegexCharsetExpression lhs, RegexCharsetExpression rhs)
		{
			if (ReferenceEquals(lhs, rhs)) return false;
			if (ReferenceEquals(lhs, null)) return true;
			return !lhs.Equals(rhs);
		}
		#endregion
	}
}
