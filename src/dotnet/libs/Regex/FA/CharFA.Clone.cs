using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	partial class CharFA<TAccept> : ICloneable
	{
		/// <summary>
		/// Deep copies the finite state machine to a new state machine
		/// </summary>
		/// <returns>The new machine</returns>
		public CharFA<TAccept> Clone()
		{
			var closure = FillClosure();
			var nclosure = new CharFA<TAccept>[closure.Count];
			for (var i = 0; i < nclosure.Length; i++)
			{
				nclosure[i] = new CharFA<TAccept>(closure[i].IsAccepting, closure[i].AcceptSymbol);
				nclosure[i].Tag = closure[i].Tag;
			}
			for (var i = 0; i < nclosure.Length; i++)
			{
				var t = nclosure[i].InputTransitions;
				var e = nclosure[i].EpsilonTransitions;
				foreach (var trns in closure[i].InputTransitions)
				{
					var id = closure.IndexOf(trns.Value);
					t.Add(trns.Key, nclosure[id]);
				}
				foreach (var trns in closure[i].EpsilonTransitions)
				{
					var id = closure.IndexOf(trns);
					e.Add(nclosure[id]);
				}
			}
			return nclosure[0];
		}
		object ICloneable.Clone() => Clone();

		/// <summary>
		/// Returns a duplicate state machine, except one that only goes from this state to the state specified in <paramref name="to"/>. Any state that does not lead to that state is eliminated from the resulting graph.
		/// </summary>
		/// <param name="to">The state to track the path to</param>
		/// <returns>A new state machine that only goes from this state to the state indicated by <paramref name="to"/></returns>
		public CharFA<TAccept> ClonePathTo(CharFA<TAccept> to)
		{
			var closure = FillClosure();
			var nclosure = new CharFA<TAccept>[closure.Count];
			for (var i = 0; i < nclosure.Length; i++)
			{
				nclosure[i] = new CharFA<TAccept>(closure[i].IsAccepting, closure[i].AcceptSymbol);
				nclosure[i].Tag = closure[i].Tag;
			}
			for (var i = 0; i < nclosure.Length; i++)
			{
				var t = nclosure[i].InputTransitions;
				var e = nclosure[i].EpsilonTransitions;
				foreach (var trns in closure[i].InputTransitions)
				{
					if (trns.Value.FillClosure().Contains(to))
					{
						var id = closure.IndexOf(trns.Value);

						t.Add(trns.Key, nclosure[id]);
					}
				}
				foreach (var trns in closure[i].EpsilonTransitions)
				{
					if (trns.FillClosure().Contains(to))
					{
						var id = closure.IndexOf(trns);
						e.Add(nclosure[id]);
					}
				}
			}
			return nclosure[0];
		}
		/// <summary>
		/// Returns a duplicate state machine, except one that only goes from this state to any state specified in <paramref name="to"/>. Any state that does not lead to one of those states is eliminated from the resulting graph.
		/// </summary>
		/// <param name="to">The collection of destination states</param>
		/// <returns>A new state machine that only goes from this state to the states indicated by <paramref name="to"/></returns>
		public CharFA<TAccept> ClonePathToAny(IEnumerable<CharFA<TAccept>> to)
		{
			var closure = FillClosure();
			var nclosure = new CharFA<TAccept>[closure.Count];
			for (var i = 0; i < nclosure.Length; i++)
			{
				nclosure[i] = new CharFA<TAccept>(closure[i].IsAccepting, closure[i].AcceptSymbol);
				nclosure[i].Tag = closure[i].Tag;
			}
			for (var i = 0; i < nclosure.Length; i++)
			{
				var t = nclosure[i].InputTransitions;
				var e = nclosure[i].EpsilonTransitions;
				foreach (var trns in closure[i].InputTransitions)
				{
					if (_ContainsAny(trns.Value.FillClosure(), to))
					{
						var id = closure.IndexOf(trns.Value);

						t.Add(trns.Key, nclosure[id]);
					}
				}
				foreach (var trns in closure[i].EpsilonTransitions)
				{
					if (_ContainsAny(trns.FillClosure(), to))
					{
						var id = closure.IndexOf(trns);
						e.Add(nclosure[id]);
					}
				}
			}
			return nclosure[0];
		}

		static bool _ContainsAny(ICollection<CharFA<TAccept>> col, IEnumerable<CharFA<TAccept>> any)
		{
			foreach (var fa in any)
				if (col.Contains(fa))
					return true;
			return false;
		}
	}
}
