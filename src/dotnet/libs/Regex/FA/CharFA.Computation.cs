using System.Collections.Generic;

namespace RE
{
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Retrieves all states reachable from this state
		/// </summary>
		/// <param name="result">The collection to hold the result, or null to create one.</param>
		/// <returns>A collection containing the closure of items</returns>
		public IList<CharFA<TAccept>> FillClosure(IList<CharFA<TAccept>> result = null)
		{
			if (null == result)
				result = new List<CharFA<TAccept>>();
			else if (result.Contains(this))
				return result;
			result.Add(this);
			// use a cast to get the optimized internal input mapping by FA state
			foreach (var trns in InputTransitions as IDictionary<CharFA<TAccept>,ICollection<char>>)
				trns.Key.FillClosure(result);
			foreach (var fa in EpsilonTransitions)
				fa.FillClosure(result);
			return result;
		}
		/// <summary>
		/// Retrieves an enumeration that indicates the closure of the state
		/// </summary>
		/// <remarks>This uses lazy evaluation.</remarks>
		public IEnumerable<CharFA<TAccept>> Closure
			=>_EnumClosure(new HashSet<CharFA<TAccept>>()); 
		
		// lazy closure implementation
		IEnumerable<CharFA<TAccept>> _EnumClosure(HashSet<CharFA<TAccept>> visited)
		{
			if (visited.Add(this))
			{
				yield return this;
				foreach (var trns in InputTransitions as IDictionary<CharFA<TAccept>, ICollection<char>>)
					foreach (var fa in trns.Key._EnumClosure(visited))
						yield return fa;
				foreach (var fa in EpsilonTransitions)
					foreach (var ffa in fa._EnumClosure(visited))
						yield return ffa;
			}
		}
		/// <summary>
		/// Retrieves all states reachable from this state on no input.
		/// </summary>
		/// <param name="result">A collection to hold the result or null to create one</param>
		/// <returns>A collection containing the epsilon closure of this state</returns>
		public IList<CharFA<TAccept>> FillEpsilonClosure(IList<CharFA<TAccept>> result = null)
		{
			if (null == result)
				result = new List<CharFA<TAccept>>();
			else if (result.Contains(this))
				return result;
			result.Add(this);
			foreach (var fa in EpsilonTransitions)
				fa.FillEpsilonClosure(result);
			return result;
		}
		/// <summary>
		/// Retrieves an enumeration that indicates the epsilon closure of this state
		/// </summary>
		/// <remarks>This uses lazy evaluation.</remarks>
		public IEnumerable<CharFA<TAccept>> EpsilonClosure
			=> _EnumEpsilonClosure(new HashSet<CharFA<TAccept>>());
		// lazy epsilon closure
		IEnumerable<CharFA<TAccept>> _EnumEpsilonClosure(HashSet<CharFA<TAccept>> visited) {
			if (visited.Add(this)) {
				yield return this;
				foreach (var fa in EpsilonTransitions)
					foreach (var ffa in fa._EnumEpsilonClosure(visited))
						yield return ffa;
			}
		}
		/// <summary>
		/// Takes a set of states and computes the total epsilon closure as a set of states
		/// </summary>
		/// <param name="states">The states to examine</param>
		/// <param name="result">The result to be filled</param>
		/// <returns>The epsilon closure of <paramref name="states"/></returns>
		public static IList<CharFA<TAccept>> FillEpsilonClosure(IEnumerable<CharFA<TAccept>> states, IList<CharFA<TAccept>> result = null)
		{
			if (null == result)
				result = new List<CharFA<TAccept>>();
			foreach (var fa in states)
				fa.FillEpsilonClosure(result);
			return result;
		}
		/// <summary>
		/// Retrieves all states that are descendants of this state
		/// </summary>
		/// <param name="result">A collection to hold the result or null to create one</param>
		/// <returns>A collection containing the descendants of this state</returns>
		public IList<CharFA<TAccept>> FillDescendants(IList<CharFA<TAccept>> result = null)
		{
			if(null==result)
				result = new List<CharFA<TAccept>>();
			foreach(var trns in InputTransitions as IDictionary<CharFA<TAccept>,ICollection<char>>)
				trns.Key.FillClosure(result);
			foreach (var fa in EpsilonTransitions)
				fa.FillClosure(result);
			return result;
		}
		/// <summary>
		/// Retrieves an enumeration that indicates the descendants of the state
		/// </summary>
		/// <remarks>This uses lazy evaluation.</remarks>
		public IEnumerable<CharFA<TAccept>> Descendants
			=> _EnumDescendants(new HashSet<CharFA<TAccept>>());

		// lazy closure implementation
		IEnumerable<CharFA<TAccept>> _EnumDescendants(HashSet<CharFA<TAccept>> visited)
		{
			foreach (var trns in InputTransitions as IDictionary<CharFA<TAccept>, ICollection<char>>)
				foreach (var fa in trns.Key._EnumClosure(visited))
					yield return fa;
			foreach (var fa in EpsilonTransitions)
				foreach (var ffa in fa._EnumClosure(visited))
					yield return ffa;
		}
		/// <summary>
		/// Fills a collection with the result of moving each of the specified <paramref name="states"/> by the specified input.
		/// </summary>
		/// <param name="states">The states to examine</param>
		/// <param name="input">The input to use</param>
		/// <param name="result">The states that are now entered as a result of the move</param>
		/// <returns><paramref name="result"/> or a new collection if it wasn't specified.</returns>
		public static IList<CharFA<TAccept>> FillMove(IEnumerable<CharFA<TAccept>> states, char input, IList<CharFA<TAccept>> result = null)
		{
			if (null == result) result = new List<CharFA<TAccept>>();
			var ec = FillEpsilonClosure(states);
			for (int ic = ec.Count,i=0;i<ic;++i)
			{
				var fa = ec[i];
				// examine each of the states reachable from this state on no input
				CharFA<TAccept> ofa;
				// see if this state has this input in its transitions
				if (fa.InputTransitions.TryGetValue(input, out ofa))
				{
					var ec2 = ofa.FillEpsilonClosure();
					for (int jc = ec2.Count, j = 0; j < jc; ++j)
					{
						var efa = ec2[j];
						if (!result.Contains(efa)) // if it does, add it if it's not already there
							result.Add(efa);
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Moves from the specified state to a destination state in a DFA by moving along the specified input.
		/// </summary>
		/// <param name="input">The input to move on</param>
		/// <returns>The state which the machine moved to or null if no state could be found.</returns>
		public CharFA<TAccept> MoveDfa(char input)
		{
			CharFA<TAccept> fa;
			if (InputTransitions.TryGetValue(input, out fa))
				return fa;
			return null;
		}
		/// <summary>
		/// Moves from the specified state to a destination state in a DFA by moving along the specified input.
		/// </summary>
		/// <param name="dfaTable">The DFA state table to use</param>
		/// <param name="state">The current state id</param>
		/// <param name="input">The input to move on</param>
		/// <returns>The state id which the machine moved to or -1 if no state could be found.</returns>
		public static int MoveDfa(CharDfaEntry[] dfaTable,int state, char input)
		{
			// go through all the transitions
			for (var i = 0; i < dfaTable[state].Transitions.Length; i++)
			{
				var entry = dfaTable[state].Transitions[i];
				var found = false;
				// go through all the ranges to see if we matched anything.
				for (var j = 0; j < entry.PackedRanges.Length; j++)
				{
					var first = entry.PackedRanges[j];
					++j;
					var last = entry.PackedRanges[j];
					if (input > last) continue;
					if (first > input) break;
					found = true;
					break;
				}
				if (found)
				{
					// set the transition destination
					return entry.Destination;
				}
			}
			return -1;
		}
		/// <summary>
		/// Returns a dictionary keyed by state, that contains all of the outgoing local input transitions, expressed as a series of ranges
		/// </summary>
		/// <param name="result">The dictionary to fill, or null to create one.</param>
		/// <returns>A dictionary containing the result of the query</returns>
		public IDictionary<CharFA<TAccept>, IList<CharRange>> FillInputTransitionRangesGroupedByState(IDictionary<CharFA<TAccept>, IList<CharRange>> result = null)
		{
			if (null == result)
				result = new Dictionary<CharFA<TAccept>, IList<CharRange>>();
			// using the optimized dictionary we have little to do here.
			foreach (var trns in (IDictionary<CharFA<TAccept>, ICollection<char>>)InputTransitions)
			{
				var sl = new List<char>(trns.Value);
				sl.Sort();
				result.Add(trns.Key, new List<CharRange>(CharRange.GetRanges(sl)));
			}
			return result;
		}
	}
}
