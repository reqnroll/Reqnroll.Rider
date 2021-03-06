using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RE
{
	/// <summary>
	/// Represents the common functionality of all regular expression elements
	/// </summary>
	public abstract class RegexExpression : ICloneable {
		/// <summary>
		/// Indicates the 1 based line on which the regular expression was found
		/// </summary>
		public int Line { get; set; } = 1;
		/// <summary>
		/// Indicates the 1 based column on which the regular expression was found
		/// </summary>
		public int Column { get; set; } = 1;
		/// <summary>
		/// Indicates the 0 based position on which the regular expression was found
		/// </summary>
		public long Position { get; set; } = 0L;
		/// <summary>
		/// Indicates whether or not this statement is a single element or not
		/// </summary>
		/// <remarks>If false, this statement will be wrapped in parentheses if necessary</remarks>
		public abstract bool IsSingleElement { get; }
		/// <summary>
		/// Sets the location information for the expression
		/// </summary>
		/// <param name="line">The 1 based line where the expression appears</param>
		/// <param name="column">The 1 based column where the expression appears</param>
		/// <param name="position">The 0 based position where the expression appears</param>
		public void SetLocation(int line, int column, long position)
		{
			Line = line;
			Column = column;
			Position = position;
		}
		/// <summary>
		/// Creates a copy of the expression
		/// </summary>
		/// <returns>A copy of the expression</returns>
		protected abstract RegexExpression CloneImpl();
		object ICloneable.Clone() => CloneImpl();
		/// <summary>
		/// Creates a state machine representing this expression
		/// </summary>
		/// <typeparam name="TAccept">The type of accept symbol to use for this expression</typeparam>
		/// <returns>A new <see cref="CharFA{TAccept}"/> finite state machine representing this expression</returns>
		public CharFA<TAccept> ToFA<TAccept>()
			=> ToFA(default(TAccept));
		/// <summary>
		/// Creates a state machine representing this expression
		/// </summary>
		/// <typeparam name="TAccept">The type of accept symbol to use for this expression</typeparam>
		/// <param name="accept">The accept symbol to use for this expression</param>
		/// <returns>A new <see cref="CharFA{TAccept}"/> finite state machine representing this expression</returns>
		public abstract CharFA<TAccept> ToFA<TAccept>(TAccept accept);

		static IList<KeyValuePair<KeyValuePair<CharFA<TAccept>,CharFA<TAccept>>,RegexExpression>> _GetTransOfState<TAccept>(IDictionary<KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>, RegexExpression> tt,CharFA<TAccept> fa,bool includeSelf=false)
		{
			var result = new List<KeyValuePair<KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>, RegexExpression>>();
			foreach (var trn in tt)
				if(trn.Key.Key==fa && (includeSelf || fa !=trn.Key.Value))
					result.Add(trn);
			return result;
		}
		static IList<KeyValuePair<KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>, RegexExpression>> _GetDstsToState<TAccept>(IDictionary<KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>, RegexExpression> tt, CharFA<TAccept> fa)
		{
			var result = new List<KeyValuePair<KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>, RegexExpression>>();
			foreach (var t in tt)
				if (t.Key.Value == fa)
					result.Add(t);
			return result;
		}
		static void _EliminateDeadStates<TAccept>(IDictionary<KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>, RegexExpression> tt,IList<CharFA<TAccept>> closure)
		{
		
			//var ttc = new List<KeyValuePair<KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>, RegexExpression>>(tt);
			var dead = new List<CharFA<TAccept>>();
			foreach (var t in tt)
			{
				var srci = closure.IndexOf(t.Key.Key);
				//var dsti = closure.IndexOf(t.Key.Value);
				// we never eliminate the start state
				if(0!=srci)
				{
					// nothing points to this state
					var dsts = _GetDstsToState(tt, t.Key.Key);
					if (0==dsts.Count)
						if(!dead.Contains(t.Key.Key))
							dead.Add(t.Key.Key);
				}
			}
			foreach(var fa in dead)
				foreach(var t in _GetTransOfState(tt,fa,true))
					tt.Remove(t.Key);
		}
		static RegexExpression _FromFA<TAccept>(CharFA<TAccept> fa, HashSet<CharFA<TAccept>> visited)
		{
			if (!visited.Add(fa))
				return null;
			var trgs = fa.FillInputTransitionRangesGroupedByState();
			bool isAccepting = fa.IsAccepting;
			RegexExpression expr = null;
			foreach (var trg in trgs)
			{
				if (1 == trg.Value.Count && 1 == trg.Value[0].Length)
				{
					RegexExpression le = new RegexLiteralExpression(trg.Value[0][0]);
					var next = _FromFA(trg.Key, visited);
					if (null != next)
						le = new RegexConcatExpression(le, next);
					if (null == expr)
						expr = le;
					else
						expr = new RegexOrExpression(expr, le);
				}
				else
				{
					var csel = new List<RegexCharsetEntry>();
					foreach (var rng in trg.Value)
					{
						if (rng.First == rng.Last)
							csel.Add(new RegexCharsetCharEntry(rng.First));
						else
							csel.Add(new RegexCharsetRangeEntry(rng.First, rng.Last));
					}
					RegexExpression cse = new RegexCharsetExpression(csel);
					var next = _FromFA(trg.Key, visited);
					if (null != next)
						cse = new RegexConcatExpression(cse, next);
					if (null == expr)
						expr = cse;
					else
						expr = new RegexOrExpression(expr, cse);
				}
				
				
			}
			var isLoop = false;
			foreach (var val in fa.Descendants)
			{
				if (val == fa)
				{
					isLoop = true;
					break;
				}
			}

			if (isAccepting && !fa.IsFinal && !isLoop)
				expr = new RegexOptionalExpression(expr);
			
			return expr;
		}
		/// <summary>
		/// Appends the textual representation to a <see cref="StringBuilder"/>
		/// </summary>
		/// <param name="sb">The string builder to use</param>
		/// <remarks>Used by ToString()</remarks>
		protected internal abstract void AppendTo(StringBuilder sb);
		/// <summary>
		/// Gets a textual representation of the expression
		/// </summary>
		/// <returns>A string representing the expression</returns>
		public override string ToString()
		{
			var result = new StringBuilder();
			AppendTo(result);
			return result.ToString();
		}
		/// <summary>
		/// Parses a regular expresion from the specified string
		/// </summary>
		/// <param name="string">The string</param>
		/// <returns>A new abstract syntax tree representing the expression</returns>
		public static RegexExpression Parse(IEnumerable<char> @string) 
			=> Parse(ParseContext.Create(@string));
		/// <summary>
		/// Parses a regular expresion from the specified <see cref="TextReader"/>
		/// </summary>
		/// <param name="reader">The text reader</param>
		/// <returns>A new abstract syntax tree representing the expression</returns>
		public static RegexExpression ReadFrom(TextReader reader)
			=> Parse(ParseContext.CreateFrom(reader));
		/// <summary>
		/// Parses a regular expression from the specified <see cref="ParseContext"/>
		/// </summary>
		/// <param name="pc">The parse context to use</param>
		/// <returns>A new abstract syntax tree representing the expression</returns>
		public static RegexExpression Parse(ParseContext pc)
		{
			RegexExpression result=null,next=null;
			int ich;
			pc.EnsureStarted();
			var line = pc.Line;
			var column = pc.Column;
			var position = pc.Position;
			while (true)
			{
				switch (pc.Current)
				{
					case -1:
						return result;
					case '.':
						var nset = new RegexCharsetExpression(new RegexCharsetEntry[] { new RegexCharsetRangeEntry(char.MinValue, char.MaxValue) }, false);
						nset.SetLocation(line, column, position);
						if (null == result)
							result = nset;
						else
						{
							result = new RegexConcatExpression(result, nset);
							result.SetLocation(line, column, position);
						}
						pc.Advance();
						result = _ParseModifier(result, pc);
						line = pc.Line;
						column = pc.Column;
						position = pc.Position;
						break;
					case '\\':
						
						pc.Advance();
						pc.Expecting();
						switch (pc.Current)
						{
							case 'd':
								next = new RegexCharsetExpression(new RegexCharsetEntry[] { new RegexCharsetClassEntry("digit") });
								pc.Advance();
								break;
							case 'D':
								next = new RegexCharsetExpression(new RegexCharsetEntry[] { new RegexCharsetClassEntry("digit") },true);
								pc.Advance();
								break;
							case 'h':
								next = new RegexCharsetExpression(new RegexCharsetEntry[] { new RegexCharsetClassEntry("blank") });
								pc.Advance();
								break;
							case 'l':
								next = new RegexCharsetExpression(new RegexCharsetEntry[] { new RegexCharsetClassEntry("lower") });
								pc.Advance();
								break;
							case 's':
								next = new RegexCharsetExpression(new RegexCharsetEntry[] { new RegexCharsetClassEntry("space") });
								pc.Advance();
								break;
							case 'S':
								next = new RegexCharsetExpression(new RegexCharsetEntry[] { new RegexCharsetClassEntry("space") },true);
								pc.Advance();
								break;
							case 'u':
								next = new RegexCharsetExpression(new RegexCharsetEntry[] { new RegexCharsetClassEntry("upper") });
								pc.Advance();
								break;
							case 'w':
								next = new RegexCharsetExpression(new RegexCharsetEntry[] { new RegexCharsetClassEntry("word") });
								pc.Advance();
								break;
							case 'W':
								next = new RegexCharsetExpression(new RegexCharsetEntry[] { new RegexCharsetClassEntry("word") },true);
								pc.Advance();
								break;
							default:
								if (-1 != (ich = _ParseEscapePart(pc)))
								{
									next = new RegexLiteralExpression((char)ich);
								}
								else
								{
									pc.Expecting(); // throw an error
									return null; // doesn't execute
								}
								break;
						}
						next.SetLocation(line, column, position);
						next = _ParseModifier(next, pc);
						if (null != result)
						{
							result = new RegexConcatExpression(result, next);
							result.SetLocation(line, column, position);
						}
						else
							result = next;
						line = pc.Line;
						column = pc.Column;
						position = pc.Position;
						break;
					case ')':
						return result;
					case '(':
						pc.Advance();
						pc.Expecting();
						next = Parse(pc);
						pc.Expecting(')');
						pc.Advance();
						next = _ParseModifier(next, pc);
						if (null == result)
							result = next;
						else
						{
							result = new RegexConcatExpression(result, next);
							result.SetLocation(line, column, position);
						}
						line = pc.Line;
						column = pc.Column;
						position = pc.Position;
						break;
					case '|':
						if (-1 != pc.Advance())
						{
							next = Parse(pc);
							result = new RegexOrExpression(result, next);
							result.SetLocation(line, column, position);
						}
						else
						{
							result = new RegexOrExpression(result, null);
							result.SetLocation(line, column, position);
						}
						line = pc.Line;
						column = pc.Column;
						position = pc.Position;
						break;
					case '[':
						pc.ClearCapture();
						pc.Advance();
						pc.Expecting();
						bool not = false;
					
						
						if ('^' == pc.Current)
						{
							not = true;
							pc.Advance();
							pc.Expecting();
						}
						var ranges = _ParseRanges(pc);
						if (ranges.Count==0)
							System.Diagnostics.Debugger.Break();
						pc.Expecting(']');
						pc.Advance();
						next = new RegexCharsetExpression(ranges, not);
						next.SetLocation(line, column, position);
						next = _ParseModifier(next, pc);
						
						if (null == result)
							result = next;
						else
						{
							result = new RegexConcatExpression(result, next);
							result.SetLocation(pc.Line, pc.Column, pc.Position);
						}
						line = pc.Line;
						column = pc.Column;
						position = pc.Position;
						break;
					default:
						ich = pc.Current;
						next = new RegexLiteralExpression((char)ich);
						next.SetLocation(line, column, position);
						pc.Advance();
						next = _ParseModifier(next, pc);
						if (null == result)
							result = next;
						else
						{
							result = new RegexConcatExpression(result, next);
							result.SetLocation(line, column, position);
						}
						line = pc.Line;
						column = pc.Column;
						position = pc.Position;
						break;
				}
			}
		}
		static IList<RegexCharsetEntry> _ParseRanges(ParseContext pc)
		{
			pc.EnsureStarted();
			var result = new List<RegexCharsetEntry>();
			RegexCharsetEntry next = null;
			bool readDash = false;
			while (-1 != pc.Current && ']' != pc.Current)
			{
				switch (pc.Current)
				{
					case '[': // char class 
						if (null != next)
						{
							result.Add(next);
							if (readDash)
								result.Add(new RegexCharsetCharEntry('-'));
							result.Add(new RegexCharsetCharEntry('-'));
						}
						pc.Advance();
						pc.Expecting(':');
						pc.Advance();
						var l = pc.CaptureBuffer.Length;
						pc.TryReadUntil(':', false);
						var n = pc.GetCapture(l);
						pc.Advance();
						pc.Expecting(']');
						pc.Advance();
						result.Add(new RegexCharsetClassEntry(n));
						readDash = false;
						next = null;
						break;
					case '\\':
						pc.Advance();
						pc.Expecting();
						switch(pc.Current)
						{
							case 'h':
								_ParseCharClassEscape(pc, "space", result, ref next, ref readDash);
								break;
							case 'd':
								_ParseCharClassEscape(pc, "digit", result, ref next, ref readDash);
								break;
							case 'D':
								_ParseCharClassEscape(pc, "^digit", result, ref next, ref readDash);
								break;
							case 'l':
								_ParseCharClassEscape(pc,"lower", result, ref next, ref readDash);
								break;
							case 's':
								_ParseCharClassEscape(pc, "space", result, ref next, ref readDash);
								break;
							case 'S':
								_ParseCharClassEscape(pc, "^space", result, ref next, ref readDash);
								break;
							case 'u':
								_ParseCharClassEscape(pc, "upper", result, ref next, ref readDash);
								break;
							case 'w':
								_ParseCharClassEscape(pc, "word", result, ref next, ref readDash);
								break;
							case 'W':
								_ParseCharClassEscape(pc, "^word", result, ref next, ref readDash);
								break;
							default:
								var ch = (char)_ParseRangeEscapePart(pc);
								if (null == next)
									next = new RegexCharsetCharEntry(ch);
								else if (readDash)
								{
									result.Add(new RegexCharsetRangeEntry(((RegexCharsetCharEntry)next).Value, ch));
									next = null;
									readDash = false;
								}
								else
								{
									result.Add(next);
									next = new RegexCharsetCharEntry(ch);
								}
								
								break;
						}
						
						break;
					case '-':
						pc.Advance();
						if (null == next)
						{
							next = new RegexCharsetCharEntry('-');
							readDash = false;
						}
						else
						{
							if (readDash)
								result.Add(next);
							readDash = true;
						}
						break;
					default:
						if (null == next)
						{
							next = new RegexCharsetCharEntry((char)pc.Current);
						}
						else
						{
							if (readDash)
							{
								result.Add(new RegexCharsetRangeEntry(((RegexCharsetCharEntry)next).Value, (char)pc.Current));
								next = null;
								readDash = false;
							}
							else
							{
								result.Add(next);
								next = new RegexCharsetCharEntry((char)pc.Current);
							}
						}
						pc.Advance();
						break;
				}
			}
			if(null!=next)
			{
				result.Add(next);
				if(readDash)
				{
					next = new RegexCharsetCharEntry('-');
					result.Add(next);
				}
			}
			return result;
		}

		static void _ParseCharClassEscape(ParseContext pc, string cls, List<RegexCharsetEntry> result, ref RegexCharsetEntry next, ref bool readDash)
		{
			if (null != next)
			{
				result.Add(next);
				if (readDash)
					result.Add(new RegexCharsetCharEntry('-'));
				result.Add(new RegexCharsetCharEntry('-'));
			}
			pc.Advance();
			result.Add(new RegexCharsetClassEntry(cls));
			next = null;
			readDash = false;
		}

		static RegexExpression _ParseModifier(RegexExpression expr,ParseContext pc)
		{
			var line = pc.Line;
			var column = pc.Column;
			var position = pc.Position;
			switch (pc.Current)
			{
				case '*':
					expr = new RegexRepeatExpression(expr);
					expr.SetLocation(line, column, position);
					pc.Advance();
					break;
				case '+':
					expr = new RegexRepeatExpression(expr, 1);
					expr.SetLocation(line, column, position);
					pc.Advance();
					break;
				case '?':
					expr = new RegexOptionalExpression(expr);
					expr.SetLocation(line, column, position);
					pc.Advance();
					break;
				case '{':
					pc.Advance();
					pc.TrySkipWhiteSpace();
					pc.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',','}');
					var min = -1;
					var max = -1;
					if(','!=pc.Current && '}'!=pc.Current)
					{
						var l = pc.CaptureBuffer.Length;
						pc.TryReadDigits();
						min = int.Parse(pc.GetCapture(l));
						pc.TrySkipWhiteSpace();
					}
					if (','==pc.Current)
					{
						pc.Advance();
						pc.TrySkipWhiteSpace();
						pc.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '}');
						if('}'!=pc.Current)
						{
							var l = pc.CaptureBuffer.Length;
							pc.TryReadDigits();
							max = int.Parse(pc.GetCapture(l));
							pc.TrySkipWhiteSpace();
						}
					} else { max = min; }
					pc.Expecting('}');
					pc.Advance();
					expr = new RegexRepeatExpression(expr, min, max);
					expr.SetLocation(line, column, position);
					break;
			}
			return expr;
		}
		/// <summary>
		/// Appends a character escape to the specified <see cref="StringBuilder"/>
		/// </summary>
		/// <param name="ch">The character to escape</param>
		/// <param name="builder">The string builder to append to</param>
		internal static void AppendEscapedChar(char ch,StringBuilder builder)
		{
			switch (ch)
			{
				case '.':
				case '/': // js expects this
				case '(':
				case ')':
				case '[':
				case ']':
				case '<': // flex expects this
				case '>':
				case '|':
				case ';': // flex expects this
				case '\'': // pck expects this
				case '\"':
				case '{':
				case '}':
				case '?':
				case '*':
				case '+':
				case '$':
				case '^':
				case '\\':
					builder.Append('\\');
					builder.Append(ch);
					return;
				case '\t':
					builder.Append("\\t");
					return;
				case '\n':
					builder.Append("\\n");
					return;
				case '\r':
					builder.Append("\\r");
					return;
				case '\0':
					builder.Append("\\0");
					return;
				case '\f':
					builder.Append("\\f");
					return;
				case '\v':
					builder.Append("\\v");
					return;
				case '\b':
					builder.Append("\\b");
					return;
				
				default:
					if (!char.IsLetterOrDigit(ch) && !char.IsSeparator(ch) && !char.IsPunctuation(ch) && !char.IsSymbol(ch))
					{

						builder.Append("\\u");
						builder.Append(unchecked((ushort)ch).ToString("x4"));

					}
					else
						builder.Append(ch);
					break;
			}

		}
		/// <summary>
		/// Escapes the specified character
		/// </summary>
		/// <param name="ch">The character to escape</param>
		/// <returns>A string representing the escaped character</returns>
		internal static string EscapeChar(char ch)
		{
			switch (ch)
			{
				case '.':
				case '/': // js expects this
				case '(':
				case ')':
				case '[':
				case ']':
				case '<': // flex expects this
				case '>':
				case '|':
				case ';': // flex expects this
				case '\'': // pck expects this
				case '\"':
				case '{':
				case '}':
				case '?':
				case '*':
				case '+':
				case '$':
				case '^':
				case '\\':
					return string.Concat("\\", ch.ToString());
				case '\t':
					return "\\t";
				case '\n':
					return "\\n";
				case '\r':
					return "\\r";
				case '\0':
					return "\\0";
				case '\f':
					return "\\f";
				case '\v':
					return "\\v";
				case '\b':
					return "\\b";
				default:
					if (!char.IsLetterOrDigit(ch) && !char.IsSeparator(ch) && !char.IsPunctuation(ch) && !char.IsSymbol(ch))
					{

						return string.Concat("\\x",unchecked((ushort)ch).ToString("x4"));

					}
					else
						return string.Concat(ch);
			}
		}
		/// <summary>
		/// Appends an escaped range character to the specified <see cref="StringBuilder"/>
		/// </summary>
		/// <param name="rangeChar">The range character to escape</param>
		/// <param name="builder">The string builder to append to</param>
		internal static void AppendEscapedRangeChar(char rangeChar,StringBuilder builder)
		{
			switch (rangeChar)
			{
				case '.':
				case '/': // js expects this
				case '(':
				case ')':
				case '[':
				case ']':
				case '<': // flex expects this
				case '>':
				case '|':
				case ':': // expected by posix (sort of, Posix doesn't allow escapes in ranges, but standard extensions do)
				case ';': // flex expects this
				case '\'': // pck expects this
				case '\"':
				case '{':
				case '}':
				case '?':
				case '*':
				case '+':
				case '$':
				case '^':
				case '-':
				case '\\':
					builder.Append('\\');
					builder.Append(rangeChar);
					return;
				case '\t':
					builder.Append("\\t");
					return;
				case '\n':
					builder.Append("\\n");
					return;
				case '\r':
					builder.Append("\\r");
					return;
				case '\0':
					builder.Append("\\0");
					return;
				case '\f':
					builder.Append("\\f");
					return;
				case '\v':
					builder.Append("\\v");
					return;
				case '\b':
					builder.Append("\\b");
					return;
				default:
					if (!char.IsLetterOrDigit(rangeChar) && !char.IsSeparator(rangeChar) && !char.IsPunctuation(rangeChar) && !char.IsSymbol(rangeChar))
					{

						builder.Append("\\u");
						builder.Append(unchecked((ushort)rangeChar).ToString("x4"));

					}
					else
						builder.Append(rangeChar);
					break;
			}
		}
		/// <summary>
		/// Escapes a range character
		/// </summary>
		/// <param name="ch">The character to escape</param>
		/// <returns>A string containing the escaped character</returns>
		internal static string EscapeRangeChar(char ch)
		{
			switch (ch)
			{
				case '.':
				case '/': // js expects this
				case '(':
				case ')':
				case '[':
				case ']':
				case '<': // flex expects this
				case '>':
				case '|':
				case ':': // expected by posix (sort of, Posix doesn't allow escapes in ranges, but standard extensions do)
				case ';': // flex expects this
				case '\'': // pck expects this
				case '\"':
				case '{':
				case '}':
				case '?':
				case '*':
				case '+':
				case '$':
				case '^':
				case '-':
				case '\\':
					return string.Concat("\\", ch.ToString());
				case '\t':
					return "\\t";
				case '\n':
					return "\\n";
				case '\r':
					return "\\r";
				case '\0':
					return "\\0";
				case '\f':
					return "\\f";
				case '\v':
					return "\\v";
				case '\b':
					return "\\b";
				default:
					if (!char.IsLetterOrDigit(ch) && !char.IsSeparator(ch) && !char.IsPunctuation(ch) && !char.IsSymbol(ch))
					{

						return string.Concat("\\x", unchecked((ushort)ch).ToString("x4"));

					}
					else
						return string.Concat(ch);
			}
		}
		static byte _FromHexChar(char hex)
		{
			if (':' > hex && '/' < hex)
				return (byte)(hex - '0');
			if ('G' > hex && '@' < hex)
				return (byte)(hex - '7'); // 'A'-10
			if ('g' > hex && '`' < hex)
				return (byte)(hex - 'W'); // 'a'-10
			throw new ArgumentException("The value was not hex.", "hex");
		}
		static bool _IsHexChar(char hex)
		{
			if (':' > hex && '/' < hex)
				return true;
			if ('G' > hex && '@' < hex)
				return true;
			if ('g' > hex && '`' < hex)
				return true;
			return false;
		}
		// return type is either char or ranges. this is kind of a union return type.
		static int _ParseEscapePart(ParseContext pc)
		{
			if (-1 == pc.Current) return -1;
			switch (pc.Current)
			{
				case 'f':
					pc.Advance();
					return '\f';
				case 'v':
					pc.Advance();
					return '\v';
				case 't':
					pc.Advance();
					return '\t';
				case 'n':
					pc.Advance();
					return '\n';
				case 'r':
					pc.Advance();
					return '\r';
				case 'x':
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return 'x';
					byte b = _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					return unchecked((char)b);
				case 'u':
					if (-1 == pc.Advance())
						return 'u';
					ushort u = _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					return unchecked((char)u);
				default:
					int i = pc.Current;
					pc.Advance();
					return (char)i;
			}
		}
		static int _ParseRangeEscapePart(ParseContext pc)
		{
			if (-1== pc.Current)
				return -1;
			switch (pc.Current)
			{
				case 'f':
					pc.Advance();
					return '\f';
				case 'v':
					pc.Advance();
					return '\v';
				case 't':
					pc.Advance();
					return '\t';
				case 'n':
					pc.Advance();
					return '\n';
				case 'r':
					pc.Advance();
					return '\r';
				case 'x':
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return 'x';
					byte b = _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					return unchecked((char)b);
				case 'u':
					if (-1 == pc.Advance())
						return 'u';
					ushort u = _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					return unchecked((char)u);
				default:
					int i = pc.Current;
					pc.Advance();
					return (char)i;
			}
		}
		static char _ReadRangeChar(IEnumerator<char> e)
		{
			char ch;
			if ('\\' != e.Current || !e.MoveNext())
			{
				return e.Current;
			}
			ch = e.Current;
			switch (ch)
			{
				case 't':
					ch = '\t';
					break;
				case 'n':
					ch = '\n';
					break;
				case 'r':
					ch = '\r';
					break;
				case '0':
					ch = '\0';
					break;
				case 'v':
					ch = '\v';
					break;
				case 'f':
					ch = '\f';
					break;
				case 'b':
					ch = '\b';
					break;
				case 'x':
					if (!e.MoveNext())
						throw new ExpectingException("Expecting input for escape \\x");
					ch = e.Current;
					byte x = _FromHexChar(ch);
					if (!e.MoveNext())
					{
						ch = unchecked((char)x);
						return ch;
					}
					x *= 0x10;
					x += _FromHexChar(e.Current);
					ch = unchecked((char)x);
					break;
				case 'u':
					if (!e.MoveNext())
						throw new ExpectingException("Expecting input for escape \\u");
					ch = e.Current;
					ushort u = _FromHexChar(ch);
					if (!e.MoveNext())
					{
						ch = unchecked((char)u);
						return ch;
					}
					u *= 0x10;
					u += _FromHexChar(e.Current);
					if (!e.MoveNext())
					{
						ch = unchecked((char)u);
						return ch;
					}
					u *= 0x10;
					u += _FromHexChar(e.Current);
					if (!e.MoveNext())
					{
						ch = unchecked((char)u);
						return ch;
					}
					u *= 0x10;
					u += _FromHexChar(e.Current);
					ch = unchecked((char)u);
					break;
				default: // return itself
					break;
			}
			return ch;
		}
		static IEnumerable<CharRange> _ParseRanges(IEnumerable<char> charRanges)
		{
			using (var e = charRanges.GetEnumerator())
			{
				var skipRead = false;

				while (skipRead || e.MoveNext())
				{
					skipRead = false;
					char first = _ReadRangeChar(e);
					if (e.MoveNext())
					{
						if ('-' == e.Current)
						{
							if (e.MoveNext())
								yield return new CharRange(first, _ReadRangeChar(e));
							else
								yield return new CharRange('-', '-');
						}
						else
						{
							yield return new CharRange(first, first);
							skipRead = true;
							continue;

						}
					}
					else
					{
						yield return new CharRange(first, first);
						yield break;
					}
				}
			}
			yield break;
		}
		static IEnumerable<CharRange> _ParseRanges(IEnumerable<char> charRanges, bool normalize)
		{
			if (!normalize)
				return _ParseRanges(charRanges);
			else
			{
				var result = new List<CharRange>(_ParseRanges(charRanges));
				CharRange.NormalizeRangeList(result);
				return result;
			}
		}
	}
}
