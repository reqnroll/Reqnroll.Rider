using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	partial class CharFA<TAccept>
	{
		// this routine uses an algorithm called powerset construction
		// see https://en.wikipedia.org/wiki/Powerset_construction
		// what it does essentially, is take every possible combination of states
		// the machine can be in, and for each possibility it creates a new state.
		// this technique effectively "splits" states as necessary in order to
		// eliminate epsilon transitions and "combines" states as it is possible.
		/// <summary>
		/// Transforms an NFA to a DFA
		/// </summary>
		/// <param name="progress">The optional progress object used to report the progress of the operation</param>
		/// <returns>A new finite state machine equivilent to this state machine but with no epsilon transitions</returns>
		public CharFA<TAccept> ToDfa(IProgress<CharFAProgress> progress = null)
		{
			// if it's already a DFA we don't need to do this transformation.
			// however, we still need to clone the state machine it because
			// the consumer expects a copy, not the original state.
			if (IsDfa)
				return Clone();
			// The DFA states are keyed by the set of NFA states they represent.
			var dfaMap = new Dictionary<List<CharFA<TAccept>>, CharFA<TAccept>>(_SetComparer.Default);

			var unmarked = new HashSet<CharFA<TAccept>>();

			// compute the epsilon closure of the initial state in the NFA
			var states = new List<CharFA<TAccept>>();
			FillEpsilonClosure(states);

			// create a new state to represent the current set of states. If one 
			// of those states is accepting, set this whole state to be accepting.
			CharFA<TAccept> dfa = new CharFA<TAccept>();
			var al = new List<TAccept>();
			// find the accepting symbols for the current states
			foreach (var fa in states)
				if (fa.IsAccepting)
					if (!al.Contains(fa.AcceptSymbol))
						al.Add(fa.AcceptSymbol);
			// here we assign the appropriate accepting symbol
			int ac = al.Count;
			if (1 == ac)
				dfa.AcceptSymbol = al[0];
			else if (1 < ac)
				dfa.AcceptSymbol = al[0]; // could throw, just choose the first one
			dfa.IsAccepting = 0 < ac;

			CharFA<TAccept> result = dfa; // store the initial state for later, so we can return it.

			// add it to the dfa map
			dfaMap.Add(states, dfa);
			dfa.Tag = new List<CharFA<TAccept>>(states);
			// add it to the unmarked states, signalling that we still have work to do.
			unmarked.Add(dfa);
			bool done = false;
			var j = 0;
			while (!done)
			{
				// report our progress
				if (null != progress)
					progress.Report(new CharFAProgress(CharFAStatus.DfaTransform, j));
				done = true;
				// a new hashset used to hold our current key states
				var mapKeys = new HashSet<List<CharFA<TAccept>>>(dfaMap.Keys, _SetComparer.Default);
				foreach (var mapKey in mapKeys)
				{
					dfa = dfaMap[mapKey];
					if (unmarked.Contains(dfa))
					{
						// when we get here, mapKey represents the epsilon closure of our 
						// current dfa state, which is indicated by kvp.Value

						// build the transition list for the new state by combining the transitions
						// from each of the old states

						// retrieve every possible input for these states
						var inputs = new HashSet<char>();
						foreach (var state in mapKey)
						{
							var dtrns = state.InputTransitions as IDictionary<CharFA<TAccept>, ICollection<char>>;
							foreach (var trns in dtrns)
								foreach (var inp in trns.Value)
									inputs.Add(inp);
						}
						// for each input, create a new transition
						foreach (var input in inputs)
						{
							var acc = new List<TAccept>();
							var ns = new List<CharFA<TAccept>>();
							foreach (var state in mapKey)
							{
								CharFA<TAccept> dst = null;
								if (state.InputTransitions.TryGetValue(input, out dst))
								{
									foreach (var d in dst.FillEpsilonClosure())
									{
										//  add the accepting symbols
										if (d.IsAccepting)
											if (!acc.Contains(d.AcceptSymbol))
												acc.Add(d.AcceptSymbol);
										if (!ns.Contains(d))
											ns.Add(d);
									}
								}
							}

							CharFA<TAccept> ndfa;
							if (!dfaMap.TryGetValue(ns, out ndfa))
							{
								ac = acc.Count;
								ndfa = new CharFA<TAccept>(0 < ac);
								// assign the appropriate accepting symbol
								if (1 == ac)
									ndfa.AcceptSymbol = acc[0];
								else if (1 < ac)
									ndfa.AcceptSymbol = acc[0]; // could throw, instead just set it to the first state's accept
								dfaMap.Add(ns, ndfa);
								// work on this new state
								unmarked.Add(ndfa);
								ndfa.Tag = new List<CharFA<TAccept>>(ns);
								done = false;
							}
							dfa.InputTransitions.Add(input, ndfa);
						}
						// we're done with this state
						unmarked.Remove(dfa);
					}
				}
				++j;
			}
			return result;
		}
		/// <summary>
		/// Indicates whether or not this state machine is a DFA
		/// </summary>
		public bool IsDfa {
			get {
				// just check if any of our states have
				// epsilon transitions
				foreach(var fa in FillClosure())
					if (0 != fa.EpsilonTransitions.Count)
						return false;
				return true;
			}
		}
	}
}
