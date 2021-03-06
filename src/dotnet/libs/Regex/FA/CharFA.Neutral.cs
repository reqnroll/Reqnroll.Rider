using System;
using System.Collections.Generic;
namespace RE
{
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Indicates whether or not the state is neutral
		/// </summary>
		public bool IsNeutral {
			get { return !IsAccepting && 0 == InputTransitions.Count && 1 == EpsilonTransitions.Count; }
		}

		static bool _TryForwardNeutral(CharFA<TAccept> fa, out CharFA<TAccept> result)
		{
			if (!fa.IsNeutral)
			{
				result = fa;
				return false;
			}
			result = fa.EpsilonTransitions[0];
			return fa != result; // false if circular
		}
		static CharFA<TAccept> _ForwardNeutrals(CharFA<TAccept> fa)
		{
			if (null == fa)
				throw new ArgumentNullException(nameof(fa));
			var result = fa;

			while (_TryForwardNeutral(result, out result)) ;
			

			return result;
		}
		/// <summary>
		/// Trims the neutral states from this machine
		/// </summary>
		public void TrimNeutrals() { TrimNeutrals(FillClosure()); }
		/// <summary>
		/// Trims the neutral states from the specified closure
		/// </summary>
		/// <param name="closure">The set of all states</param>
		public static void TrimNeutrals(IEnumerable<CharFA<TAccept>> closure)
		{
			var cl = new List<CharFA<TAccept>>(closure);
			foreach (var s in cl)
			{
				var repls = new List<KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>>();
				var td = s.InputTransitions.CharactersByState;
				var inputTransitions = s.InputTransitions;
				foreach (var fa in td.Keys)
				{
					var fa2 = _ForwardNeutrals(fa);
					if (null == fa2)
						throw new InvalidProgramException("null in forward neutrals support code");
					if (fa != fa2)
						repls.Add(new KeyValuePair<CharFA<TAccept>, CharFA<TAccept>>(fa, fa2));
				}
				foreach (var repl in repls)
				{
					var inps = td[repl.Key];
					inputTransitions.Remove(repl.Key);
					inputTransitions.Add(repl.Value, inps);
				}
				var ec = s.EpsilonTransitions.Count;
				for (int j = 0; j < ec; ++j)
					s.EpsilonTransitions[j] = _ForwardNeutrals(s.EpsilonTransitions[j]);
			}
		}
	}
}
