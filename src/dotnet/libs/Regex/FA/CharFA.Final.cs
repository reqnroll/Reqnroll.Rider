using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Indicates whether or not the state has any outgoing transitions
		/// </summary>
		public bool IsFinal {
			get { return 0 == InputTransitions.Count && 0 == EpsilonTransitions.Count; }
		}

		/// <summary>
		/// Retrieves all the states reachable from this state that are final.
		/// </summary>
		/// <param name="result">The list of final states. Will be filled after the call.</param>
		/// <returns>The resulting list of final states. This is the same value as the result parameter, if specified.</returns>
		public IList<CharFA<TAccept>> FillFinalStates(IList<CharFA<TAccept>> result = null)
			=> FillFinalStates(FillClosure(), result);
		/// <summary>
		/// Retrieves all the states in this closure that are final
		/// </summary>
		/// <param name="closure">The closure to examine</param>
		/// <param name="result">The list of final states. Will be filled after the call.</param>
		/// <returns>The resulting list of final states. This is the same value as the result parameter, if specified.</returns>
		public static IList<CharFA<TAccept>> FillFinalStates(IList<CharFA<TAccept>> closure, IList<CharFA<TAccept>> result = null)
		{
			if (null == result)
				result = new List<CharFA<TAccept>>();
			for (int ic = closure.Count, i = 0; i < ic; ++i)
			{
				var fa = closure[i];
				if (fa.IsFinal)
					if (!result.Contains(fa))
						result.Add(fa);
			}
			return result;
		}
		/// <summary>
		/// Makes all accepting states transition to a new accepting final state, and sets them as non-accepting
		/// </summary>
		/// <param name="accept">The symbol to accept</param>
		public void Finalize(TAccept accept = default(TAccept))
		{
			var asc = FillAcceptingStates();
			var ascc = asc.Count;
			if (1 == ascc) return; // don't need to do anything
			var final = new CharFA<TAccept>(true, accept);
			for (var i = 0; i < ascc; ++i)
			{
				var fa = asc[i];
				fa.IsAccepting = false;
				fa.EpsilonTransitions.Add(final);
			}
		}
	}
}
