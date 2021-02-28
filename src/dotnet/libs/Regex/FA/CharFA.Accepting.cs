using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Retrieves all the states reachable from this state that are accepting.
		/// </summary>
		/// <param name="result">The list of accepting states. Will be filled after the call.</param>
		/// <returns>The resulting list of accepting states. This is the same value as the result parameter, if specified.</returns>
		public IList<CharFA<TAccept>> FillAcceptingStates(IList<CharFA<TAccept>> result = null)
			=> FillAcceptingStates(FillClosure(), result);
		/// <summary>
		/// Retrieves all the accept symbols from this state machine.
		/// </summary>
		/// <param name="result">The list of accept symbols. Will be filled after the call.</param>
		/// <returns>The resulting list of accept symbols. This is the same value as the result parameter, if specified.</returns>
		public IList<TAccept> FillAcceptSymbols(IList<TAccept> result = null)
			=> FillAcceptSymbols(FillClosure(), result);
		/// <summary>
		/// Retrieves all the states in this closure that are accepting
		/// </summary>
		/// <param name="closure">The closure to examine</param>
		/// <param name="result">The list of accepting states. Will be filled after the call.</param>
		/// <returns>The resulting list of accepting states. This is the same value as the result parameter, if specified.</returns>
		public static IList<CharFA<TAccept>> FillAcceptingStates(IList<CharFA<TAccept>> closure, IList<CharFA<TAccept>> result = null)
		{
			if (null == result)
				result = new List<CharFA<TAccept>>();
			for (int ic = closure.Count, i = 0; i < ic; ++i)
			{
				var fa = closure[i];
				if (fa.IsAccepting)
					if (!result.Contains(fa))
						result.Add(fa);
			}
			return result;
		}
		/// <summary>
		/// Retrieves all the accept symbols states in this closure
		/// </summary>
		/// <param name="closure">The closure to examine</param>
		/// <param name="result">The list of accept symbols. Will be filled after the call.</param>
		/// <returns>The resulting list of accept symbols. This is the same value as the result parameter, if specified.</returns>
		public static IList<TAccept> FillAcceptSymbols(IList<CharFA<TAccept>> closure, IList<TAccept> result = null)
		{
			if (null == result)
				result = new List<TAccept>();
			for (int ic = closure.Count, i = 0; i < ic; ++i)
			{
				var fa = closure[i];
				if (fa.IsAccepting) 
					if (!result.Contains(fa.AcceptSymbol))
						result.Add(fa.AcceptSymbol);
			}
			return result;
		}
		/// <summary>
		/// Returns the first state that accepts from a given FA, or null if none do.
		/// </summary>
		public CharFA<TAccept> FirstAcceptingState {
			get {
				foreach (var fa in Closure)
					if (fa.IsAccepting)
						return fa;
				return null;
			}
		}
		/// <summary>
		/// Returns the first accept symbol from a given FA, or the default value if none.
		/// </summary>
		public TAccept FirstAcceptSymbol {
			get {
				var fas = FirstAcceptingState;
				if (null != fas)
					return fas.AcceptSymbol;
				return default(TAccept);
			}
		}
		/// <summary>
		/// Indicates whether any of the states in the specified collection are accepting
		/// </summary>
		/// <param name="states">The state collection to examine</param>
		/// <returns>True if one or more of the states is accepting, otherwise false.</returns>
		public static bool IsAnyAccepting(IEnumerable<CharFA<TAccept>> states)
		{
			foreach (var fa in states)
				if (fa.IsAccepting)
					return true;
			return false;
		}
		/// <summary>
		/// Retrieves the first accept symbol from the collection of states
		/// </summary>
		/// <param name="states">The states to examine</param>
		/// <param name="result">The accept symbol, if the method returned true</param>
		/// <returns>True if an accept symbol was found, otherwise false</returns>
		public static bool TryGetAnyAcceptSymbol(IEnumerable<CharFA<TAccept>> states, out TAccept result)
		{
			foreach (var fa in states)
			{
				if (fa.IsAccepting)
				{
					result = fa.AcceptSymbol;
					return true;
				}
			}
			result = default(TAccept);
			return false;
		}
	}
}
