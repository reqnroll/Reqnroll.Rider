// a class that helps with writing hand-rolled parsers, which we use for regex parsing
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RE
{

	#region ExpectingException
	/// <summary>
	/// An exception encountered during parsing where the stream contains one thing, but another is expected
	/// </summary>
	[Serializable]
	public sealed class ExpectingException : Exception
	{
		/// <summary>
		/// Initialize the exception with the specified message.
		/// </summary>
		/// <param name="message">The message</param>
		public ExpectingException(string message) : base(message) { }
		/// <summary>
		/// The list of expected strings.
		/// </summary>
		public string[] Expecting { get; internal set; }
		/// <summary>
		/// The position when the error was realized.
		/// </summary>
		public long Position { get; internal set; }
		/// <summary>
		/// The line of the error
		/// </summary>
		public int Line { get; internal set; }
		/// <summary>
		/// The column of the error
		/// </summary>
		public int Column { get; internal set; }

	}
	#endregion ExpectingException
	/// <summary>
	/// see https://www.codeproject.com/Articles/5162847/ParseContext-2-0-Easier-Hand-Rolled-Parsers
	/// </summary>
	public class ParseContext : IDisposable
	{
		/// <summary>
		/// Attempts to read whitespace from the current input, capturing it
		/// </summary>
		/// <returns>True if whitespace was read, otherwise false</returns>
		public bool TryReadWhiteSpace()
		{
			EnsureStarted();
			if (-1 == Current || !char.IsWhiteSpace((char)Current))
				return false;
			CaptureCurrent();
			while (-1 != Advance() && char.IsWhiteSpace((char)Current))
				CaptureCurrent();
			return true;
		}
		/// <summary>
		/// Attempts to skip whitespace in the current input without capturing it
		/// </summary>
		/// <returns>True if whitespace was skipped, otherwise false</returns>
		public bool TrySkipWhiteSpace()
		{
			EnsureStarted();
			if (-1 == Current || !char.IsWhiteSpace((char)Current))
				return false;
			while (-1 != Advance() && char.IsWhiteSpace((char) Current))
			{
			}
			return true;
		}
		/// <summary>
		/// Attempts to read up until the specified character, optionally consuming it
		/// </summary>
		/// <param name="character">The character to halt at</param>
		/// <param name="readCharacter">True if the character should be consumed, otherwise false</param>
		/// <returns>True if the character was found, otherwise false</returns>
		public bool TryReadUntil(int character, bool readCharacter = true)
		{
			EnsureStarted();
			if (0 > character) character = -1;
			CaptureCurrent();
			if (Current == character)
			{
				return true;
			}
			while (-1 != Advance() && Current != character)
				CaptureCurrent();
			//
			if (Current == character)
			{
				if (readCharacter)
				{
					CaptureCurrent();
					Advance();
				}
				return true;
			}
			return false;
		}
		/// <summary>
		/// Attempts to skip up until the specified character, optionally consuming it
		/// </summary>
		/// <param name="character">The character to halt at</param>
		/// <param name="skipCharacter">True if the character should be consumed, otherwise false</param>
		/// <returns>True if the character was found, otherwise false</returns>
		public bool TrySkipUntil(int character, bool skipCharacter = true)
		{
			EnsureStarted();
			if (0 > character) character = -1;
			if (Current == character)
				return true;
			while (-1 != Advance() && Current != character)
			{
			}

			if (Current == character)
			{
				if (skipCharacter)
					Advance();
				return true;
			}
			return false;
		}
		/// <summary>
		/// Attempts to read up until the specified character, using the specified escape, optionally consuming it
		/// </summary>
		/// <param name="character">The character to halt at</param>
		/// <param name="escapeChar">The escape indicator character to use</param>
		/// <param name="readCharacter">True if the character should be consumed, otherwise false</param>
		/// <returns>True if the character was found, otherwise false</returns>
		public bool TryReadUntil(int character, int escapeChar, bool readCharacter = true)
		{
			EnsureStarted();
			if (0 > character) character = -1;
			if (-1 == Current) return false;
			if (Current == character)
			{
				if (readCharacter)
				{
					CaptureCurrent();
					Advance();
				}
				return true;
			}

			do
			{
				if (escapeChar == Current)
				{
					CaptureCurrent();
					if (-1 == Advance())
						return false;
					CaptureCurrent();
				}
				else
				{
					if (character == Current)
					{
						if (readCharacter)
						{
							CaptureCurrent();
							Advance();
						}
						return true;
					}
					else
						CaptureCurrent();
				}
			}
			while (-1 != Advance());

			return false;
		}
		/// <summary>
		/// Attempts to skip up until the specified character, using the specified escape, optionally consuming it
		/// </summary>
		/// <param name="character">The character to halt at</param>
		/// <param name="escapeChar">The escape indicator character to use</param>
		/// <param name="skipCharacter">True if the character should be consumed, otherwise false</param>
		/// <returns>True if the character was found, otherwise false</returns>
		public bool TrySkipUntil(int character, int escapeChar, bool skipCharacter = true)
		{
			EnsureStarted();
			if (0 > character) character = -1;
			if (Current == character)
				return true;
			while (-1 != Advance() && Current != character)
			{
				if (character == escapeChar)
					if (-1 == Advance())
						break;
			}
			if (Current == character)
			{
				if (skipCharacter)
					Advance();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Attempts to read a series of digits, consuming them
		/// </summary>
		/// <returns>True if digits were consumed, otherwise false</returns>
		public bool TryReadDigits()
		{
			EnsureStarted();
			if (-1 == Current || !char.IsDigit((char)Current))
				return false;
			CaptureCurrent();
			while (-1 != Advance() && char.IsDigit((char)Current))
				CaptureCurrent();
			return true;
		}

		ParseContext(IEnumerable<char> inner) { _inner = inner.GetEnumerator(); }
		ParseContext(TextReader inner) { _inner = new _TextReaderEnumerator(inner); }
		Queue<char> _input = new Queue<char>();
		IEnumerator<char> _inner = null;
		/// <summary>
		/// Indicates the capture buffer used to hold gathered input
		/// </summary>
		public StringBuilder CaptureBuffer { get; } = new StringBuilder();
		/// <summary>
		/// Indicates the 0 based position of the parse context
		/// </summary>
		public long Position { get; private set; } = -2;
		/// <summary>
		/// Indicates the 1 based column of the parse context
		/// </summary>
		public int Column { get; private set; } = 1;
		/// <summary>
		/// Indicates the 1 based line of the parse context
		/// </summary>
		public int Line { get; private set; } = 1;
		/// <summary>
		/// Indicates the current status, -1 if end of input (like <see cref="TextReader"/>) or -2 if before the beginning.
		/// </summary>
		public int Current { get; private set; } = -2;
		/// <summary>
		/// Indicates the width of tabs on the output device.
		/// </summary>
		/// <remarks>Used for tracking column position</remarks>
		public int TabWidth { get; set; } = 8;
		bool _EnsureInput()
		{
			if (0 == _input.Count)
			{
				if (!_inner.MoveNext())
					return false;
				_input.Enqueue(_inner.Current);
				return true;
			}
			return true;
		}
		/// <summary>
		/// Ensures that the parse context is started and the input cursor is valid
		/// </summary>
		public void EnsureStarted()
		{
			_CheckDisposed();
			if (-2 == Current)
				Advance();
		}
		/// <summary>
		/// Peeks the specified number of characters ahead in the input without advancing
		/// </summary>
		/// <param name="lookAhead">Indicates the number of characters to look ahead. Zero is the current position.</param>
		/// <returns>An integer representing the character at the position, or -1 if past the end of the input.</returns>
		public int Peek(int lookAhead = 1)
		{
			_CheckDisposed();
			if (-2 == Current) throw new InvalidOperationException("The parse context has not been started.");
			if (0 > lookAhead)
				lookAhead = 0;
			if (!EnsureLookAhead(0 != lookAhead ? lookAhead : 1))
				return -1;
			int i = 0;
			foreach (var result in _input)
			{
				if (i == lookAhead)
					return result;
				++i;
			}
			return -1;

		}
		/// <summary>
		/// Pre-reads the specified amount of lookahead characters
		/// </summary>
		/// <param name="lookAhead">The number of lookahead characters to read</param>
		/// <returns>True if the entire lookahead request could be satisfied, otherwise false</returns>
		public bool EnsureLookAhead(int lookAhead = 1)
		{
			_CheckDisposed();
			if (1 > lookAhead) lookAhead = 1;
			while (_input.Count < lookAhead && _inner.MoveNext())
				_input.Enqueue(_inner.Current);
			return _input.Count >= lookAhead;
		}
		/// <summary>
		/// Advances the input cursor by one
		/// </summary>
		/// <returns>An integer representing the next character</returns>
		public int Advance()
		{
			_CheckDisposed();
			if (0 != _input.Count)
				_input.Dequeue();
			if (_EnsureInput())
			{
				if (-2 == Current)
				{
					Position = -1;
					Column = 0;
				}
				Current = _input.Peek();
				++Column;
				++Position;
				if ('\n' == Current)
				{
					++Line;
					Column = 0;
				}
				else if ('\r' == Current)
				{
					Column = 0;
				}
				else if ('\t' == Current && 0 < TabWidth)
				{
					Column = ((Column / TabWidth) + 1) * TabWidth;
				}
				// handle other whitespace as necessary here...
				return Current;
			}
			if (-1 != Current)
			{ // last read moves us past the end. subsequent reads don't move anything
				++Position;
				++Column;
			}
			Current = -1;
			return -1;
		}
		/// <summary>
		/// Disposes of the parse context and closes any resources used
		/// </summary>
		public void Dispose()
		{
			if (null != _inner)
			{
				Current = -3;
				_inner.Dispose();
				_inner = null;
			}
		}
		/// <summary>
		/// Clears the capture buffer
		/// </summary>
		public void ClearCapture()
		{
			_CheckDisposed();
			CaptureBuffer.Clear();
		}
		/// <summary>
		/// Captures the current character if available
		/// </summary>
		public void CaptureCurrent()
		{
			_CheckDisposed();
			if (-2 == Current) throw new InvalidOperationException("The parse context has not been started.");
			if (-1 != Current)
				CaptureBuffer.Append((char)Current);
		}
		/// <summary>
		/// Gets the capture buffer at the specified start index
		/// </summary>
		/// <param name="startIndex">The index to begin copying</param>
		/// <param name="count">The number of characters to copy</param>
		/// <returns>A string representing the specified subset of the capture buffer</returns>
		public string GetCapture(int startIndex, int count = 0)
		{
			_CheckDisposed();
			if (0 == count)
				count = CaptureBuffer.Length - startIndex;
			return CaptureBuffer.ToString(startIndex, count);
		}
		/// <summary>
		/// Gets the capture buffer at the specified start index
		/// </summary>
		/// <param name="startIndex">The index to begin copying</param>
		/// <returns>A string representing the specified subset of the capture buffer</returns>
		public string GetCapture(int startIndex = 0)
		{
			_CheckDisposed();
			return CaptureBuffer.ToString(startIndex, CaptureBuffer.Length - startIndex);
		}
		/// <summary>
		/// Sets the location information for the parse context
		/// </summary>
		/// <remarks>This does not move the cursor. It simply updates the position information.</remarks>
		/// <param name="line">The 1 based current line</param>
		/// <param name="column">The 1 based current column</param>
		/// <param name="position">The zero based current position</param>
		public void SetLocation(int line, int column, long position)
		{
			switch (Current)
			{
				case -3:
					throw new ObjectDisposedException(GetType().Name);
				case -2:
					throw new InvalidOperationException("The cursor is before the start of the stream.");
				case -1:
					throw new InvalidOperationException("The cursor is after the end of the stream.");
			}
			Position = position;
			Line = line;
			Column = column;
		}
		/// <summary>
		/// Throws a <see cref="ExpectingException"/> with a set of packed int ranges where the ints are pairs indicating first and last
		/// </summary>
		/// <param name="expecting">The packed ranges</param>
		[DebuggerHidden()]
		public void ThrowExpectingRanges(int[] expecting)
		{
			ExpectingException ex = null;
			ex = new ExpectingException(_GetExpectingMessageRanges(expecting));
			ex.Position = Position;
			ex.Line = Line;
			ex.Column = Column;
			ex.Expecting = null;
			throw ex;

		}
		void _CheckDisposed()
		{
			if (-3 == Current) throw new ObjectDisposedException(GetType().Name);
		}
		string _GetExpectingMessageRanges(int[] expecting)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('[');
			for (var i = 0; i < expecting.Length; i++)
			{
				var first = expecting[i];
				++i;
				var last = expecting[i];
				if (first == last)
				{
					if (-1 == first)
						sb.Append("(end of stream)");
					else
						sb.Append((char)first);
				}
				else
				{
					sb.Append((char)first);
					sb.Append('-');
					sb.Append((char)last);
				}
			}
			sb.Append(']');
			string at = string.Concat(" at line ", Line, ", column ", Column, ", position ", Position);
			if (-1 == Current)
			{
				if (0 == expecting.Length)
					return string.Concat("Unexpected end of input", at, ".");
				return string.Concat("Unexpected end of input. Expecting ", sb.ToString(), at, ".");
			}
			if (0 == expecting.Length)
				return string.Concat("Unexpected character \"", (char)Current, "\" in input", at, ".");
			return string.Concat("Unexpected character \"", (char)Current, "\" in input. Expecting ", sb.ToString(), at, ".");

		}
		string _GetExpectingMessage(int[] expecting)
		{
			StringBuilder sb = null;
			switch (expecting.Length)
			{
				case 0:
					break;
				case 1:
					sb = new StringBuilder();
					if (-1 == expecting[0])
						sb.Append("end of input");
					else
					{
						sb.Append("\"");
						sb.Append((char)expecting[0]);
						sb.Append("\"");
					}
					break;
				case 2:
					sb = new StringBuilder();
					if (-1 == expecting[0])
						sb.Append("end of input");
					else
					{
						sb.Append("\"");
						sb.Append((char)expecting[0]);
						sb.Append("\"");
					}
					sb.Append(" or ");
					if (-1 == expecting[1])
						sb.Append("end of input");
					else
					{
						sb.Append("\"");
						sb.Append((char)expecting[1]);
						sb.Append("\"");
					}
					break;
				default: // length > 2
					sb = new StringBuilder();
					if (-1 == expecting[0])
						sb.Append("end of input");
					else
					{
						sb.Append("\"");
						sb.Append((char)expecting[0]);
						sb.Append("\"");
					}
					int l = expecting.Length - 1;
					int i = 1;
					for (; i < l; ++i)
					{
						sb.Append(", ");
						if (-1 == expecting[i])
							sb.Append("end of input");
						else
						{
							sb.Append("\"");
							sb.Append((char)expecting[i]);
							sb.Append("\"");
						}
					}
					sb.Append(", or ");
					if (-1 == expecting[i])
						sb.Append("end of input");
					else
					{
						sb.Append("\"");
						sb.Append((char)expecting[i]);
						sb.Append("\"");
					}
					break;
			}
			string at = string.Concat(" at line ", Line, ", column ", Column, ", position ", Position);
			if (-1 == Current)
			{
				if (0 == expecting.Length)
					return string.Concat("Unexpected end of input", at, ".");
				return string.Concat("Unexpected end of input. Expecting ", sb.ToString(), at, ".");
			}
			if (0 == expecting.Length)
				return string.Concat("Unexpected character \"", (char)Current, "\" in input", at, ".");
			return string.Concat("Unexpected character \"", (char)Current, "\" in input. Expecting ", sb.ToString(), at, ".");

		}
		/// <summary>
		/// Throws an exception indicating the expected characters if the current character is not one of the specified characters
		/// </summary>
		/// <param name="expecting">The characters to check for, or -1 for end of input. If the characters are empty, any character other than the end of input is accepted.</param>
		/// <exception cref="ExpectingException">Raised when the current character doesn't match any of the specified characters</exception>
		[DebuggerHidden()]
		public void Expecting(params int[] expecting)
		{
			ExpectingException ex = null;
			switch (expecting.Length)
			{
				case 0:
					if (-1 == Current)
						ex = new ExpectingException(_GetExpectingMessage(expecting));
					break;
				case 1:
					if (expecting[0] != Current)
						ex = new ExpectingException(_GetExpectingMessage(expecting));
					break;
				default:
					if (0 > Array.IndexOf(expecting, Current))
						ex = new ExpectingException(_GetExpectingMessage(expecting));
					break;
			}
			if (null != ex)
			{
				ex.Position = Position;
				ex.Line = Line;
				ex.Column = Column;
				ex.Expecting = new string[expecting.Length];
				for (int i = 0; i < ex.Expecting.Length; i++)
					ex.Expecting[i] = Convert.ToString(expecting[i]);
				throw ex;
			}
		}
		/// <summary>
		/// Creates a parse context over a string (<see cref="IEnumerable{Char}"/>)
		/// </summary>
		/// <param name="string">The input string to use</param>
		/// <returns>A parse context over the specified input</returns>
		public static ParseContext Create(IEnumerable<char> @string) { return new ParseContext(@string); }
		/// <summary>
		/// Creates a parse context over a <see cref="TextReader"/>
		/// </summary>
		/// <param name="reader">The text reader to use</param>
		/// <returns>A parse context over the specified input</returns>
		public static ParseContext CreateFrom(TextReader reader) { return new ParseContext(reader); }
		/// <summary>
		/// Creates a parse context over the specified file
		/// </summary>
		/// <param name="filename">The filename to use</param>
		/// <returns>A parse context over the specified file</returns>
		public static ParseContext CreateFrom(string filename) { return new ParseContext(File.OpenText(filename)); }

		class _TextReaderEnumerator : IEnumerator<char>
		{
			int _current = -2;
			TextReader _inner;
			internal _TextReaderEnumerator(TextReader inner) { _inner = inner; }
			public char Current {
				get {
					switch (_current)
					{
						case -1:
							throw new InvalidOperationException("The enumerator is past the end of the stream.");
						case -2:
							throw new InvalidOperationException("The enumerator has not been started.");
					}
					return unchecked((char)_current);
				}
			}
			object IEnumerator.Current => Current;

			public void Dispose()
			{
				_current = -3;
				if (null != _inner)
				{
					_inner.Dispose();
					_inner = null;
				}
			}
			public bool MoveNext()
			{
				switch (_current)
				{
					case -1:
						return false;
					case -3:
						throw new ObjectDisposedException(GetType().Name);
				}
				_current = _inner.Read();
				if (-1 == _current)
					return false;
				return true;
			}

			public void Reset()
			{
				throw new NotImplementedException();
			}
		}

	}

}
