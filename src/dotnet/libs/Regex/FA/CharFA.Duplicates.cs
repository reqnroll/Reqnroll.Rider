using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	// a series of methods for querying and removing duplicate states. This isn't necessary for a working regex engine, but it makes the result more efficient.
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Indicates whether this state is a duplicate of another state.
		/// </summary>
		/// <param name="rhs">The state to compare with</param>
		/// <returns>True if the states are duplicates (one can be removed without changing the language of the machine)</returns>
		public bool IsDuplicate(CharFA<TAccept> rhs)
		{
			return null != rhs && IsAccepting == rhs.IsAccepting &&
				_SetComparer.Default.Equals(EpsilonTransitions, rhs.EpsilonTransitions) &&
				_SetComparer.Default.Equals((IDictionary<CharFA<TAccept>, ICollection<char>>)InputTransitions, (IDictionary<CharFA<TAccept>, ICollection<char>>)rhs.InputTransitions);
		}
		/// <summary>
		/// Fills a dictionary of duplicates by state for any duplicates found in the state graph
		/// </summary>
		/// <param name="result">The resulting dictionary to be filled.</param>
		/// <returns>The resulting dictionary of duplicates</returns>
		public IDictionary<CharFA<TAccept>, ICollection<CharFA<TAccept>>> FillDuplicatesGroupedByState(IDictionary<CharFA<TAccept>, ICollection<CharFA<TAccept>>> result = null)
			=> FillDuplicatesGroupedByState(FillClosure());

		/// <summary>
		/// Fills a dictionary of duplicates by state for any duplicates found in the state graph
		/// </summary>
		/// <param name="closure">The closure to examine</param>
		/// <param name="result">The resulting dictionary to be filled.</param>
		/// <returns>The resulting dictionary of duplicates</returns>
		public static IDictionary<CharFA<TAccept>, ICollection<CharFA<TAccept>>> FillDuplicatesGroupedByState(IList<CharFA<TAccept>> closure, IDictionary<CharFA<TAccept>, ICollection<CharFA<TAccept>>> result = null)
		{
			if (null == result)
				result = new Dictionary<CharFA<TAccept>, ICollection<CharFA<TAccept>>>();
			var cl = closure;
			int c = cl.Count;
			for (int i = 0; i < c; i++)
			{
				var s = cl[i];
				for (int j = i + 1; j < c; j++)
				{
					var cmp = cl[j];
					if (s.IsDuplicate(cmp))
					{
						ICollection<CharFA<TAccept>> col = new List<CharFA<TAccept>>();
						if (!result.ContainsKey(s))
							result.Add(s, col);
						else
							col = result[s];
						if (!col.Contains(cmp))
							col.Add(cmp);
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Trims duplicate states from the graph.
		/// </summary>
		public void TrimDuplicates(IProgress<CharFAProgress> progress = null) => TrimDuplicates(FillClosure(), progress);
		/// <summary>
		/// Trims duplicate states from the graph
		/// </summary>
		/// <param name="closure">The closure to alter.</param>
		/// <param name="progress">The progress object used to report the progress of the task</param>
		public static void TrimDuplicates(IList<CharFA<TAccept>> closure, IProgress<CharFAProgress> progress = null)
		{
			var lclosure = closure;
			var dups = new Dictionary<CharFA<TAccept>, ICollection<CharFA<TAccept>>>();
			int oc = 0;
			int c = -1;
			var k = 0;
			// we may have to run this multiple times to remove all references
			while (c < oc)
			{
				if (null != progress)
					progress.Report(new CharFAProgress(CharFAStatus.TrimDuplicates, k));
				c = lclosure.Count;
				FillDuplicatesGroupedByState(lclosure, dups);
				if (0 < dups.Count)
				{
					// for each pair of duplicates basically we replace all references to the first
					// with references to the latter, thus eliminating the duplicate state:
					foreach (KeyValuePair<CharFA<TAccept>, ICollection<CharFA<TAccept>>> de in dups)
					{
						var replacement = de.Key;
						var targets = de.Value;
						for (int i = 0; i < c; ++i)
						{
							var s = lclosure[i];

							var repls = new List<KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>>();
							var td = (IDictionary<CharFA<TAccept>, ICollection<char>>)s.InputTransitions;
							foreach (var trns in td)
								if (targets.Contains(trns.Key))
									repls.Add(new KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>(trns.Key, replacement));
							foreach (var repl in repls)
							{
								var inps = td[repl.Key];
								td.Remove(repl.Key);
								ICollection<char> v;
								if (!td.TryGetValue(repl.Value, out v))
									td.Add(repl.Value, inps);
								else foreach (var inp in inps)
										if (!v.Contains(inp))
											v.Add(inp);

							}

							int lc = s.EpsilonTransitions.Count;
							for (int j = 0; j < lc; ++j)
								if (targets.Contains(s.EpsilonTransitions[j]))
									s.EpsilonTransitions[j] = de.Key;
						}
					}
					dups.Clear();
				}
				else
					break;
				oc = c;
				var f = lclosure[0];
				lclosure = f.FillClosure();
				c = lclosure.Count;
				++k;
			}
		}
	}
}
