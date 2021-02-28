using System;
using System.Collections.Generic;

namespace RE
{
	// Provides static builder methods for creating common regular expression patterns
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Creates an FA that matches a literal string
		/// </summary>
		/// <param name="string">The string to match</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA machine that will match this literal</returns>
		public static CharFA<TAccept> Literal(IEnumerable<char> @string, TAccept accept = default(TAccept))
		{
			var result = new CharFA<TAccept>();
			var current = result;
			foreach (var ch in @string)
			{
				current.IsAccepting = false;
				var fa = new CharFA<TAccept>(true, accept);
				current.InputTransitions.Add(ch, fa);
				current = fa;
			}
			return result;
		}
		/// <summary>
		/// Creates an FA that will match any one of a set of a characters
		/// </summary>
		/// <param name="set">The set of characters that will be matched</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>An FA that will match the specified set</returns>
		public static CharFA<TAccept> Set(IEnumerable<char> set, TAccept accept = default(TAccept))
		{
			var result = new CharFA<TAccept>();
			var final = new CharFA<TAccept>(true, accept);
			foreach (var ch in set)
				result.InputTransitions[ch]= final;
			return result;
		}
		/// <summary>
		/// Creates a new FA that is a concatenation of two other FA expressions
		/// </summary>
		/// <param name="exprs">The FAs to concatenate</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA that is the concatenation of the specified FAs</returns>
		public static CharFA<TAccept> Concat(IEnumerable<CharFA<TAccept>> exprs, TAccept accept = default(TAccept))
		{
			CharFA<TAccept> left = null;
			var right = left;
			foreach (var val in exprs)
			{
				if (null == val) continue;
				//Debug.Assert(null != val.FirstAcceptingState);
				var nval = val.Clone();
				//Debug.Assert(null != nval.FirstAcceptingState);
				if (null == left)
				{
					left = nval;
					//Debug.Assert(null != left.FirstAcceptingState);
					continue;
				}
				else if (null == right)
				{
					right = nval;
					//Debug.Assert(null != right.FirstAcceptingState);
				}
				else
				{
					//Debug.Assert(null != right.FirstAcceptingState);
					_Concat(right, nval);
					//Debug.Assert(null != right.FirstAcceptingState);

				}
				//Debug.Assert(null != left.FirstAcceptingState);
				_Concat(left, right.Clone());
				//Debug.Assert(null != left.FirstAcceptingState);

			}
			if (null != right)
			{
				right.FirstAcceptingState.AcceptSymbol = accept;
			}
			else
			{
				left.FirstAcceptingState.AcceptSymbol = accept;
			}
			return left;
		}
		static void _Concat(CharFA<TAccept> lhs, CharFA<TAccept> rhs)
		{
			//Debug.Assert(lhs != rhs);
			var f = lhs.FirstAcceptingState;
			//Debug.Assert(null != rhs.FirstAcceptingState);
			f.IsAccepting = false;
			f.EpsilonTransitions.Add(rhs);
			//Debug.Assert(null!= lhs.FirstAcceptingState);

		}
		/// <summary>
		/// Creates an FA that will match any one of a set of a characters
		/// </summary>
		/// <param name="ranges">The set ranges of characters that will be matched</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>An FA that will match the specified set</returns>
		public static CharFA<TAccept> Set(IEnumerable<CharRange> ranges, TAccept accept = default(TAccept))
		{
			var result = new CharFA<TAccept>();
			var final = new CharFA<TAccept>(true, accept);

			foreach (var ch in CharRange.ExpandRanges(ranges))
				result.InputTransitions[ch]= final;
			return result;
		}
		/// <summary>
		/// Creates a new FA that matches any one of the FA expressions passed
		/// </summary>
		/// <param name="exprs">The expressions to match</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA that will match the union of the FA expressions passed</returns>
		public static CharFA<TAccept> Or(IEnumerable<CharFA<TAccept>> exprs, TAccept accept = default(TAccept))
		{
			var result = new CharFA<TAccept>();
			var final = new CharFA<TAccept>(true, accept);
			foreach (var fa in exprs)
			{
				if (null != fa)
				{
					var nfa = fa.Clone();
					result.EpsilonTransitions.Add(nfa);
					var nffa = nfa.FirstAcceptingState;
					nffa.IsAccepting = false;
					nffa.EpsilonTransitions.Add(final);
				}
				else if (!result.EpsilonTransitions.Contains(final))
					result.EpsilonTransitions.Add(final);
			}
			return result;
		}
		/// <summary>
		/// Creates a new FA that will match a repetition of the specified FA expression
		/// </summary>
		/// <param name="expr">The expression to repeat</param>
		/// <param name="minOccurs">The minimum number of times to repeat or -1 for unspecified (0)</param>
		/// <param name="maxOccurs">The maximum number of times to repeat or -1 for unspecified (unbounded)</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA that matches the specified FA one or more times</returns>
		public static CharFA<TAccept> Repeat(CharFA<TAccept> expr, int minOccurs = -1, int maxOccurs = -1, TAccept accept = default(TAccept))
		{
			expr = expr.Clone();
			if (minOccurs > 0 && maxOccurs > 0 && minOccurs > maxOccurs)
				throw new ArgumentOutOfRangeException(nameof(maxOccurs));
			CharFA<TAccept> result;
			switch (minOccurs)
			{
				case -1:
				case 0:
					switch (maxOccurs)
					{
						case -1:
						case 0:
							result = new CharFA<TAccept>();
							var final = new CharFA<TAccept>(true, accept);
							final.EpsilonTransitions.Add(result);
							foreach (var afa in expr.FillAcceptingStates())
							{
								afa.IsAccepting = false;
								afa.EpsilonTransitions.Add(final);
							}
							result.EpsilonTransitions.Add(expr);
							result.EpsilonTransitions.Add(final);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
						case 1:
							result = Optional(expr, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
						default:
							var l = new List<CharFA<TAccept>>();
							expr = Optional(expr);
							l.Add(expr);
							for (int i = 1; i < maxOccurs; ++i)
							{
								l.Add(expr.Clone());
							}
							result = Concat(l, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
					}
				case 1:
					switch (maxOccurs)
					{
						case -1:
						case 0:
							result = new CharFA<TAccept>();
							var final = new CharFA<TAccept>(true, accept);
							final.EpsilonTransitions.Add(result);
							foreach (var afa in expr.FillAcceptingStates())
							{
								afa.IsAccepting = false;
								afa.EpsilonTransitions.Add(final);
							}
							result.EpsilonTransitions.Add(expr);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
						case 1:
							//Debug.Assert(null != expr.FirstAcceptingState);
							return expr;
						default:
							result = Concat(new CharFA<TAccept>[] { expr, Repeat(expr.Clone(), 0, maxOccurs - 1) }, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
					}
				default:
					switch (maxOccurs)
					{
						case -1:
						case 0:
							result = Concat(new CharFA<TAccept>[] { Repeat(expr, minOccurs, minOccurs, accept), Repeat(expr, 0, 0, accept) }, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
						case 1:
							throw new ArgumentOutOfRangeException(nameof(maxOccurs));
						default:
							if (minOccurs == maxOccurs)
							{
								var l = new List<CharFA<TAccept>>();
								l.Add(expr);
								//Debug.Assert(null != expr.FirstAcceptingState);
								for (int i = 1; i < minOccurs; ++i)
								{
									var e = expr.Clone();
									//Debug.Assert(null != e.FirstAcceptingState);
									l.Add(e);
								}
								result = Concat(l, accept);
								//Debug.Assert(null != result.FirstAcceptingState);
								return result;
							}
							result = Concat(new CharFA<TAccept>[] { Repeat(expr.Clone(), minOccurs, minOccurs, accept), Repeat(Optional(expr.Clone()), maxOccurs - minOccurs, maxOccurs - minOccurs, accept) }, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;


					}
			}
			// should never get here
			throw new NotImplementedException();
		}
		/// <summary>
		/// Creates a new FA that matches the specified FA expression or empty
		/// </summary>
		/// <param name="expr">The expression to make optional</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA that will match the specified expression or empty</returns>
		public static CharFA<TAccept> Optional(CharFA<TAccept> expr, TAccept accept = default(TAccept))
		{
			var result = expr.Clone();
			var f = result.FirstAcceptingState;
			f.AcceptSymbol = accept;
			result.EpsilonTransitions.Add(f);
			return result;
		}
		/// <summary>
		/// Makes the specified expression case insensitive
		/// </summary>
		/// <param name="expr">The target expression</param>
		/// <param name="accept">The accept symbol</param>
		/// <returns>A new expression that is the case insensitive equivelent of <paramref name="expr"/></returns>
		public static CharFA<TAccept> CaseInsensitive(CharFA<TAccept> expr,TAccept accept=default(TAccept))
		{
			var fa = expr.Clone();
			var closure = fa.FillClosure();
			for(int ic=closure.Count,i=0;i<ic;++i)
			{
				var ffa = closure[i];
				if (ffa.IsAccepting)
					ffa.AcceptSymbol = accept;
				foreach(var trns in ffa.InputTransitions as IDictionary<CharFA<TAccept>, ICollection<char>>)
				{
					foreach(var ch in new List<char>(trns.Value))
					{
						if(char.IsLower(ch))
						{
							var cch = char.ToUpperInvariant(ch);
							if(!trns.Value.Contains(cch))
								ffa.InputTransitions.Add(cch, trns.Key);
						} else if(char.IsUpper(ch))
						{
							var cch = char.ToLowerInvariant(ch);
							if (!trns.Value.Contains(cch))
								ffa.InputTransitions.Add(cch, trns.Key);
						}
					}
				}
			}
			return fa;
		}
	}
}
