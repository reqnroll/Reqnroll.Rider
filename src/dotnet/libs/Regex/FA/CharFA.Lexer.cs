using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Creates a lexer out of the specified FSM "expressions"
		/// </summary>
		/// <param name="exprs">The expressions to compose the lexer with</param>
		/// <returns>An FSM representing the lexer.</returns>
		public static CharFA<TAccept> ToLexer(params CharFA<TAccept>[] exprs)
		{
			var result = new CharFA<TAccept>();
			for (var i = 0; i < exprs.Length; i++)
				result.EpsilonTransitions.Add(exprs[i]);
			return result;
		}
		/// <summary>
		/// Lexes the next input from the parse context.
		/// </summary>
		/// <param name="context">The <see cref="ParseContext"/> to use.</param>
		/// <param name="errorSymbol">The symbol to report in the case of an error</param>
		/// <returns>The next symbol matched - <paramref name="context"/> contains the capture and line information</returns>
		public TAccept Lex(ParseContext context,TAccept errorSymbol = default(TAccept))
		{
			TAccept acc;
			// get the initial states
			var states = FillEpsilonClosure();
			// prepare the parse context
			context.EnsureStarted();
			while (true)
			{
				// if no more input
				if (-1 == context.Current)
				{
					// if we accept, return that
					if (TryGetAnyAcceptSymbol(states, out acc))
						return acc;
					// otherwise return error
					return errorSymbol;
				}
				// move by current character
				var newStates = FillMove(states, (char)context.Current);
				// we couldn't match anything
				if (0 == newStates.Count)
				{
					// if we accept, return that
					if (TryGetAnyAcceptSymbol(states, out acc))
						return acc;
					// otherwise error
					// store the current character
					context.CaptureCurrent();
					// advance the input
					context.Advance();
					return errorSymbol;
				}
				// store the current character
				context.CaptureCurrent();
				// advance the input
				context.Advance();
				// iterate to our next states
				states = newStates;
			}
		}
		/// <summary>
		/// Lexes the next input from the parse context.
		/// </summary>
		/// <param name="context">The <see cref="ParseContext"/> to use.</param>
		/// <param name="errorSymbol">The symbol to report in the case of an error</param>
		/// <returns>The next symbol matched - <paramref name="context"/> contains the capture and line information</returns>
		/// <remarks>This method will not work properly on an NFA but will not error in that case, so take care to only use this with a DFA</remarks>
		public TAccept LexDfa(ParseContext context, TAccept errorSymbol = default(TAccept))
		{
			// track our current state
			var state = this;
			// prepare the parse context
			context.EnsureStarted();
			while (true)
			{
				// if no more input
				if (-1 == context.Current)
				{
					// if we accept, return that
					if(state.IsAccepting)
						return state.AcceptSymbol;
					// otherwise return error
					return errorSymbol;
				}
				// move by current character
				var newState = state.MoveDfa((char)context.Current);
				// we couldn't match anything
				if (null == newState)
				{
					// if we accept, return that
					if (state.IsAccepting)
						return state.AcceptSymbol;
					// otherwise error
					// store the current character
					context.CaptureCurrent();
					// advance the input
					context.Advance();
					return errorSymbol;
				}
				// store the current character
				context.CaptureCurrent();
				// advance the input
				context.Advance();
				// iterate to our next states
				state = newState;
			}
		}
		/// <summary>
		/// Lexes the next input from the parse context.
		/// </summary>
		/// <param name="dfaTable">The DFA state table to use</param>
		/// <param name="context">The <see cref="ParseContext"/> to use.</param>
		/// <param name="errorSymbol">The symbol id to report in the case of an error</param>
		/// <returns>The next symbol id matched - <paramref name="context"/> contains the capture and line information</returns>
		public static int LexDfa(CharDfaEntry[] dfaTable, ParseContext context, int errorSymbol = -1)
		{
			// track our current state
			var state = 0;
			// prepare the parse context
			context.EnsureStarted();
			while (true)
			{
				// if no more input
				if (-1 == context.Current)
				{
					var sid = dfaTable[state].AcceptSymbolId;
					// if we accept, return that
					if (-1 != sid)
						return sid;
					// otherwise return error
					return errorSymbol;
				}
				// move by current character
				var newState = MoveDfa(dfaTable, state, (char)context.Current);
				// we couldn't match anything
				if (-1 == newState)
				{
					// if we accept, return that
					if (-1 != dfaTable[state].AcceptSymbolId)
						return dfaTable[state].AcceptSymbolId;
					// otherwise error
					// store the current character
					context.CaptureCurrent();
					// advance the input
					context.Advance();
					return errorSymbol;
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
