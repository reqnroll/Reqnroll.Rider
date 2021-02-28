using System;
using System.Collections.Generic;

namespace RE
{
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Returns a DFA table that can be used to lex or match
		/// </summary>
		/// <param name="symbolTable">The symbol table to use, or null to just implicitly tag symbols with integer ids</param>
		/// <param name="progress">The progress object used to report the progress of the task</param>
		/// <returns>A DFA table that can be used to efficiently match or lex input</returns>
		public CharDfaEntry[] ToDfaStateTable(IList<TAccept> symbolTable = null, IProgress<CharFAProgress> progress=null)
		{
			// only convert to a DFA if we haven't already
			// ToDfa() already checks but it always copies
			// the state information so this performs better
			var dfa = IsDfa?this: ToDfa(progress);
			var closure = dfa.FillClosure();
			var symbolLookup = new ListDictionary<TAccept, int>();
			// if we don't have a symbol table, build 
			// the symbol lookup from the states.
			if (null == symbolTable)
			{
				// go through each state, looking for accept symbols
				// and then add them to the new symbol table is we
				// haven't already
				var i = 0;
				for (int jc = closure.Count, j = 0; j < jc; ++j)
				{
					var fa = closure[j];
					if (fa.IsAccepting && !symbolLookup.ContainsKey(fa.AcceptSymbol))
					{
						symbolLookup.Add(fa.AcceptSymbol, i);
						++i;
					}
				}
			}
			else // build the symbol lookup from the symbol table
				for (int ic = symbolTable.Count, i = 0; i < ic; ++i)
					if (null != symbolTable[i])
						symbolLookup.Add(symbolTable[i], i);

			// build the root array
			var result = new CharDfaEntry[closure.Count];
			for (var i = 0; i < result.Length; i++)
			{
				var fa = closure[i];
				// get all the transition ranges for each destination state
				var trgs = fa.FillInputTransitionRangesGroupedByState();
				// make a new transition entry array for our DFA state table
				var trns = new CharDfaTransitionEntry[trgs.Count];
				var j = 0;
				// for each transition range
				foreach (var trg in trgs)
				{
					// add the transition entry using
					// the packed ranges from CharRange
					trns[j] = new CharDfaTransitionEntry(
						CharRange.ToPackedChars(trg.Value),
						closure.IndexOf(trg.Key));

					++j;
				}
				// now add the state entry for the state above
				result[i] = new CharDfaEntry(
					fa.IsAccepting ? symbolLookup[fa.AcceptSymbol] : -1,
					trns);

			}
			return result;
		}
	}
}
