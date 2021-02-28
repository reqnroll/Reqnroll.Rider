namespace RE
{
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Pattern matches through a string of text
		/// </summary>
		/// <param name="context">The parse context to search</param>
		/// <returns>A <see cref="CharFAMatch"/> that contains the match information, or null if the match is not found.</returns>
		public CharFAMatch Match(ParseContext context)
		{
			context.EnsureStarted();
			var line = context.Line;
			var column = context.Column;
			var position = context.Position;
			var l = context.CaptureBuffer.Length;
			var success = false;
			// keep going until we find something or reach the end
			while (-1 != context.Current && !(success = _DoMatch(context)))
			{
				line = context.Line;
				column = context.Column;
				position = context.Position;
				l = context.CaptureBuffer.Length;
			}
			if (success)
				return new CharFAMatch(
					line,
					column,
					position,
					context.GetCapture(l));
			return null;
		}
		/// <summary>
		/// Pattern matches through a string of text using a DFA
		/// </summary>
		/// <param name="context">The parse context to search</param>
		/// <returns>A <see cref="CharFAMatch"/> that contains the match information, or null if the match is not found.</returns>
		/// <remarks>An NFA will not work with this method, but for performance reasons we cannot verify that the state machine is a DFA before running. Be sure to only use DFAs with this method.</remarks>
		public CharFAMatch MatchDfa(ParseContext context)
		{
			context.EnsureStarted();
			var line = context.Line;
			var column = context.Column;
			var position = context.Position;
			var l = context.CaptureBuffer.Length;
			var success = false;
			// keep going until we find something or reach the end
			while (-1 != context.Current && !(success = _DoMatchDfa(context)))
			{
				line = context.Line;
				column = context.Column;
				position = context.Position;
				l = context.CaptureBuffer.Length;
			}
			if (success)
				return new CharFAMatch(
					line,
					column,
					position,
					context.GetCapture(l));
			return null;
		}
		/// <summary>
		/// Pattern matches through a string of text using a DFA
		/// </summary>
		/// <param name="dfaTable">The DFA state table to use</param>
		/// <param name="context">The parse context to search</param>
		/// <returns>A <see cref="CharFAMatch"/> that contains the match information, or null if the match is not found.</returns>
		public static CharFAMatch MatchDfa(CharDfaEntry[] dfaTable, ParseContext context)
		{
			context.EnsureStarted();
			var line = context.Line;
			var column = context.Column;
			var position = context.Position;
			var l = context.CaptureBuffer.Length;
			var success = false;
			// keep going until we find something or reach the end
			while (-1 != context.Current && !(success = _DoMatchDfa(dfaTable, context)))
			{
				line = context.Line;
				column = context.Column;
				position = context.Position;
				l = context.CaptureBuffer.Length;
			}
			if (success)
				return new CharFAMatch(
					line,
					column,
					position,
					context.GetCapture(l));
			return null;
		}
		// almost the same as our lex methods
		bool _DoMatch(ParseContext context)
		{
			// get the initial states
			var states = FillEpsilonClosure();
			while (true)
			{
				// if no more input
				if (-1 == context.Current)
				{
					// if we accept, return that
					return IsAnyAccepting(states);
				}
				// move by current character
				var newStates = FillMove(states, (char)context.Current);
				// we couldn't match anything
				if (0 == newStates.Count)
				{
					// if we accept, return that
					if (IsAnyAccepting(states))
						return true;
					// otherwise error
					// advance the input
					context.Advance();
					return false;
				}
				// store the current character
				context.CaptureCurrent();
				// advance the input
				context.Advance();
				// iterate to our next states
				states = newStates;
			}
		}
		bool _DoMatchDfa(ParseContext context)
		{
			// track the current state
			var state = this;
			while (true)
			{
				// if no more input
				if (-1 == context.Current)
				{
					// if we accept, return that
					return state.IsAccepting;
				}
				// move by current character
				var newState = state.MoveDfa((char)context.Current);
				// we couldn't match anything
				if (null == newState)
				{
					// if we accept, return that
					if (state.IsAccepting)
						return true;
					// otherwise error
					// advance the input
					context.Advance();
					return false;
				}
				// store the current character
				context.CaptureCurrent();
				// advance the input
				context.Advance();
				// iterate to our next state
				state = newState;
			}
		}
		static bool _DoMatchDfa(CharDfaEntry[] dfaTable, ParseContext context)
		{
			// track our current state
			var state = 0;
			// prepare the parse context
			context.EnsureStarted();
			while (true)
			{
				// if no more input
				if (-1 == context.Current)
					// if we accept, return that
					return -1 != dfaTable[state].AcceptSymbolId;
				
				// move by current character
				var newState = MoveDfa(dfaTable, state, (char)context.Current);
				// we couldn't match anything
				if (-1 == newState)
				{
					// if we accept, return that
					if (-1 != dfaTable[state].AcceptSymbolId)
						return true;
					// otherwise error
					// advance the input
					context.Advance();
					return false;
				}
				// store the current character
				context.CaptureCurrent();
				// advance the input
				context.Advance();
				// iterate to our next states
				state = newState;
			}
		}
	}
}
