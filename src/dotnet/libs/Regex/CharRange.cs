using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	/// <summary>
	/// Represents an ascending range of characters.
	/// </summary>
	public struct CharRange : IEquatable<CharRange>, IList<char>
	{
		/// <summary>
		/// Initializes the character range with the specified first and last characters
		/// </summary>
		/// <param name="first">The first character</param>
		/// <param name="last">The last character</param>
		public CharRange(char first, char last) { First = (first <= last) ? first : last; Last = (first <= last) ? last : first; }
		/// <summary>
		/// Gets a character at the specified index
		/// </summary>
		/// <param name="index">The index within the range</param>
		/// <returns>The character at the specified index</returns>
		char IList<char>.this[int index] { get => this[index]; set { _ThrowReadOnly(); } }
		/// <summary>
		/// Gets a character at the specified index
		/// </summary>
		/// <param name="index">The index within the range</param>
		/// <returns>The character at the specified index</returns>
		public char this[int index] { get { if (0 > index || Length <= index) throw new IndexOutOfRangeException(); return (char)(First + index); } }
		/// <summary>
		/// Gets the length of the range
		/// </summary>
		public int Length { get { return Last - First + 1; } }
		/// <summary>
		/// Gets the first character in the range
		/// </summary>
		public char First { get; }
		/// <summary>
		/// Gets the last character in the range
		/// </summary>
		public char Last { get; }
		/// <summary>
		/// Gets ranges for a series of characters.
		/// </summary>
		/// <param name="sortedString">The sorted characters</param>
		/// <returns>A series of ranges representing the specified characters</returns>
		public static IEnumerable<CharRange> GetRanges(IEnumerable<char> sortedString)
		{
			char first = '\0';
			char last = '\0';
			using (IEnumerator<char> e = sortedString.GetEnumerator())
			{
				bool moved = e.MoveNext();
				while (moved)
				{
					first = last = e.Current;
					while ((moved = e.MoveNext()) && (e.Current == last || e.Current == last + 1))
					{
						last = e.Current;
					}
					yield return new CharRange(first, last);

				}
			}
		}
		/// <summary>
		/// Returns an array of character pairs representing the ranges
		/// </summary>
		/// <param name="ranges">The ranges to pack</param>
		/// <returns>A packed array of ranges</returns>
		public static char[] ToPackedChars(IEnumerable<CharRange> ranges)
		{
			var rl = new List<CharRange>(ranges);
			NormalizeRangeList(rl);
			var result = new char[rl.Count * 2];
			int j = 0;
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = rl[j].First;
				++i;
				result[i] = rl[j].Last;
				++j;
			}
			return result;
		}
		/// <summary>
		/// Returns an array of int pairs representing the ranges
		/// </summary>
		/// <param name="ranges">The ranges to pack</param>
		/// <returns>A packed array of ranges</returns>
		public static int[] ToPackedInts(IEnumerable<CharRange> ranges)
		{
			var rl = new List<CharRange>(ranges);
			NormalizeRangeList(rl);
			var result = new int[rl.Count * 2];
			int j = 0;
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = rl[j].First;
				++i;
				result[i] = rl[j].Last;
				++j;
			}
			return result;
		}
		/// <summary>
		/// Returns a packed string of character pairs representing the ranges
		/// </summary>
		/// <param name="ranges">The ranges to pack</param>
		/// <returns>A string containing the packed ranges</returns>
		public static string ToPackedString(IEnumerable<CharRange> ranges)
		{
			var rl = new List<CharRange>(ranges);
			NormalizeRangeList(rl);
			int j = 0;
			var result = new StringBuilder();
			for (int ic = rl.Count * 2, i = 0; i < ic; ++i)
			{
				result.Append(rl[j].First);
				++i;
				result.Append(rl[j].Last);
				++j;
			}
			return result.ToString();
		}
		/// <summary>
		/// Expands the ranges into a collection of characters
		/// </summary>
		/// <param name="ranges">The ranges to expand</param>
		/// <returns>A collection of characters representing the ranges</returns>
		public static IEnumerable<char> ExpandRanges(IEnumerable<CharRange> ranges)
		{
			var seen = new HashSet<char>();
			foreach (var range in ranges)
				foreach (char ch in range)
					if (seen.Add(ch))
						yield return ch;
		}
		/// <summary>
		/// Negates the character ranges
		/// </summary>
		/// <param name="ranges">The ranges to negate</param>
		/// <returns>The inverse set of ranges. Every character not in <paramref name="ranges"/> becomes part of a range.</returns>
		public static IEnumerable<CharRange> NotRanges(IEnumerable<CharRange> ranges)
		{
			// expects ranges to be normalized
			var last = char.MaxValue;
			using (var e = ranges.GetEnumerator())
			{
				if (!e.MoveNext())
				{
					yield return new CharRange(char.MinValue, char.MaxValue);
					yield break;
				}
				if (e.Current.First > char.MinValue)
				{
					yield return new CharRange(char.MinValue, unchecked((char)(e.Current.First - 1)));
					last = e.Current.Last;
					if (char.MaxValue == last)
						yield break;
				}
				while (e.MoveNext())
				{
					if (char.MaxValue == last)
						yield break;
					if (unchecked((char)(last + 1)) < e.Current.First)
						yield return new CharRange(unchecked((char)(last + 1)), unchecked((char)(e.Current.First - 1)));
					last = e.Current.Last;
				}
				if (char.MaxValue > last)
					yield return new CharRange(unchecked((char)(last + 1)), char.MaxValue);

			}

		}
		/// <summary>
		/// Takes a list of ranges and ensures each range's First character is less than or equal to its Last character
		/// </summary>
		/// <param name="ranges">The ranges to normalize</param>
		public static void NormalizeRangeList(List<CharRange> ranges)
		{
			ranges.Sort(delegate (CharRange left, CharRange right)
			{
				return left.First.CompareTo(right.First);
			});
			var or = default(CharRange);
			for (int i = 1; i < ranges.Count; ++i)
			{
				if (ranges[i - 1].Last >= ranges[i].First)
				{
					var nr = new CharRange(ranges[i - 1].First, ranges[i].Last);
					ranges[i - 1] = or = nr;
					ranges.RemoveAt(i);
					--i; // compensated for by ++i in for loop
				}
			}
		}
		/// <summary>
		/// Returns the count of characters in the range
		/// </summary>
		int ICollection<char>.Count => Length;
		/// <summary>
		/// Indicates that the range is read only
		/// </summary>
		bool ICollection<char>.IsReadOnly => true;
		/// <summary>
		/// Indicates whether this range equals another range
		/// </summary>
		/// <param name="rhs">The range to compare</param>
		/// <returns>True if the ranges are equal, otherwise false</returns>
		public bool Equals(CharRange rhs)
			=> First == rhs.First && Last == rhs.Last;
		/// <summary>
		/// Indicates whether this range equals another range
		/// </summary>
		/// <param name="obj">The range to compare</param>
		/// <returns>True if the ranges are equal, otherwise false</returns>
		public override bool Equals(object obj)
			=> obj is CharRange && Equals((CharRange)obj);
		/// <summary>
		/// Gets the hash code for the range
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
			=> First ^ Last;
		/// <summary>
		/// Returns a string representation of a range
		/// </summary>
		/// <returns>A string representing the range</returns>
		public override string ToString()
		{
			if (First == Last)
				return _Escape(First);
			if (2==Length)
				return string.Concat(_Escape(First), _Escape(Last));
			if (3 == Length)
				return string.Concat(_Escape(First), _Escape((char)(First+1)), _Escape(Last));

			return string.Concat(_Escape(First), "-", _Escape(Last));
		}
		// throws on attempt to write
		void _ThrowReadOnly()
		{
			throw new NotSupportedException("The collection is read-only.");
		}
		// collection support (required)
		void ICollection<char>.Add(char item)
		{
			_ThrowReadOnly();
		}
		// collection support (required)
		void ICollection<char>.Clear()
		{
			_ThrowReadOnly();
		}
		// collection support (required)
		bool ICollection<char>.Contains(char item)
			=> item >= First && item <= Last;
		// collection support (required)
		void ICollection<char>.CopyTo(char[] array, int arrayIndex)
		{
			char ch = First;
			for (int ic = Length, i = arrayIndex; i < ic; ++i)
			{
				array[i] = ch;
				++ch;
			}
		}
		// collection support (required)
		IEnumerator<char> IEnumerable<char>.GetEnumerator()
		{
			if (First != Last)
			{
				for (char ch = First; ch < Last; ++ch)
					yield return ch;
				yield return Last;
			}
			else
				yield return First;
		}
		// legacy collection support (required)
		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable<char>)this).GetEnumerator();
		// list support (required)
		int IList<char>.IndexOf(char item)
		{
			if (First <= item && Last >= item)
				return item - First;
			return -1;
		}
		// legacy list support
		void IList<char>.Insert(int index, char item)
		{
			_ThrowReadOnly();
		}
		// legacy collection support
		bool ICollection<char>.Remove(char item)
		{
			_ThrowReadOnly();
			return false;
		}
		// legacy list support
		void IList<char>.RemoveAt(int index)
		{
			_ThrowReadOnly();
		}

		#region _Escape
		// escapes a character
		string _Escape(char ch)
		{
			switch (ch)
			{
				case '\n':
					return @"\n";
				case '\r':
					return @"\r";
				case '\t':
					return @"\t";
				case '\f':
					return @"\f";
				case '\b':
					return @"\b";
				case '-':
					return @"\-";
				case '[':
					return @"\[";
				case ']':
					return @"\]";
				case '(':
					return @"\(";
				case ')':
					return @"\)";
				case '?':
					return @"\?";
				case '+':
					return @"\+";
				case '*':
					return @"\*";
				case '.':
					return @"\.";
				case '^':
					return @"\^";
				case ' ':
					return " ";
				default:
					if (char.IsControl(ch) || char.IsWhiteSpace(ch))
						return @"\u" + ((int)ch).ToString("x4");
					break;
			}
			return ch.ToString();
		}
		#endregion
		// C# value semantics overloads
		/// <summary>
		/// Indicates whether or not two character ranges are the same
		/// </summary>
		/// <param name="lhs">The left hand range to compare</param>
		/// <param name="rhs">The right hand range to compare</param>
		/// <returns>True if the ranges are the same, otherwise false</returns>
		public static bool operator ==(CharRange lhs, CharRange rhs) => lhs.Equals(rhs);
		/// <summary>
		/// Indicates whether or not two character ranges are different
		/// </summary>
		/// <param name="lhs">The left hand range to compare</param>
		/// <param name="rhs">The right hand range to compare</param>
		/// <returns>True if the ranges are different, otherwise false</returns>
		public static bool operator !=(CharRange lhs, CharRange rhs) => !lhs.Equals(rhs);

	}
}
