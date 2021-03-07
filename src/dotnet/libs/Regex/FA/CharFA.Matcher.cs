namespace RE
{
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Pattern matches through a string of text
		/// </summary>
		/// <param name="successOnAnyState">When true, it will return success if the input string can be fully match but is incompleted. Eg: Regex('Abc') will return true for 'Ab'</param>
		/// <param name="context">The parse context to search</param>
		/// <returns>A <see cref="CharFAMatch"/> that contains the match information, or null if the match is not found.</returns>
		public CharFAMatch Match(ParseContext context, bool successOnAnyState = false)
		{
			context.EnsureStarted();
			var line = context.Line;
			var column = context.Column;
			var position = context.Position;
			var l = context.CaptureBuffer.Length;
			var success = false;
			// keep going until we find something or reach the end
			while (-1 != context.Current && !(success = _DoMatch(context, successOnAnyState)))
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
		bool _DoMatch(ParseContext context, bool successOnAnyState = false)
		{
			// get the initial states
			var states = FillEpsilonClosure();
			while (true)
			{
				// if no more input
				if (-1 == context.Current)
				{
					// if we accept, return that
					return successOnAnyState || IsAnyAccepting(states);
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
	}
}
