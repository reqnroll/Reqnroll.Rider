using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	partial class CharFA<TAccept>
	{
		// this class provides a series of comparers for various CharFA operations
		// these are primarily used during duplicate checking and in the powerset 
		// construction
		// see: https://www.codeproject.com/Articles/5251448/Implementing-Value-Equality-in-Csharp
		private sealed class _SetComparer : 
			IEqualityComparer<IList<CharFA<TAccept>>>, 
			IEqualityComparer<ICollection<CharFA<TAccept>>>, 
			IEqualityComparer<IDictionary<char, CharFA<TAccept>>>
		{
			// unordered comparison
			public bool Equals(IList<CharFA<TAccept>> lhs, IList<CharFA<TAccept>> rhs)
			{
				if (ReferenceEquals(lhs, rhs))
					return true;
				else if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs))
					return false;
				if (lhs.Count != rhs.Count)
					return false;
				using (var xe = lhs.GetEnumerator())
				using (var ye = rhs.GetEnumerator())
					while (xe.MoveNext() && ye.MoveNext())
						if (!rhs.Contains(xe.Current) || !lhs.Contains(ye.Current))
							return false;
				return true;
			}
			// unordered comparison
			public bool Equals(ICollection<CharFA<TAccept>> lhs, ICollection<CharFA<TAccept>> rhs)
			{
				if (ReferenceEquals(lhs, rhs))
					return true;
				else if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs))
					return false;
				if (lhs.Count != rhs.Count)
					return false;
				using (var xe = lhs.GetEnumerator())
				using (var ye = rhs.GetEnumerator())
					while (xe.MoveNext() && ye.MoveNext())
						if (!rhs.Contains(xe.Current) || !lhs.Contains(ye.Current))
							return false;
				return true;
			}
			public bool Equals(IDictionary<char, CharFA<TAccept>> lhs, IDictionary<char, CharFA<TAccept>> rhs)
			{
				if (ReferenceEquals(lhs, rhs))
					return true;
				else if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs))
					return false;
				if (lhs.Count != rhs.Count)
					return false;
				using (var xe = lhs.GetEnumerator())
				using (var ye = rhs.GetEnumerator())
					while (xe.MoveNext() && ye.MoveNext())
						if (!rhs.Contains(xe.Current) || !lhs.Contains(ye.Current))
							return false;
				return true;
			}
			public bool Equals(IDictionary<CharFA<TAccept>, ICollection<char>> lhs, IDictionary<CharFA<TAccept>, ICollection<char>> rhs)
			{
				if (ReferenceEquals(lhs, rhs))
					return true;
				else if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs))
					return false;
				if (lhs.Count != rhs.Count)
					return false;
				foreach (var trns in lhs)
				{
					ICollection<char> col;
					if (!rhs.TryGetValue(trns.Key, out col))
						return false;
					using (var xe = trns.Value.GetEnumerator())
					using (var ye = col.GetEnumerator())
						while (xe.MoveNext() && ye.MoveNext())
							if (!col.Contains(xe.Current) || !trns.Value.Contains(ye.Current))
								return false;
				}

				return true;
			}
			public static bool _EqualsInput(ICollection<char> lhs, ICollection<char> rhs)
			{
				if (ReferenceEquals(lhs, rhs))
					return true;
				else if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs))
					return false;
				if (lhs.Count != rhs.Count)
					return false;
				using (var xe = lhs.GetEnumerator())
				using (var ye = rhs.GetEnumerator())
					while (xe.MoveNext() && ye.MoveNext())
						if (!rhs.Contains(xe.Current) || !lhs.Contains(ye.Current))
							return false;
				return true;
			}
			public int GetHashCode(IList<CharFA<TAccept>> lhs)
			{
				var result = 0;
				for (int ic = lhs.Count, i = 0; i < ic; ++i)
				{
					var fa = lhs[i];
					if (null != fa)
						result ^= fa.GetHashCode();
				}
				return result;
			}
			public int GetHashCode(ICollection<CharFA<TAccept>> lhs)
			{
				var result = 0;
				foreach (var fa in lhs)
					if (null != fa)
						result ^= fa.GetHashCode();
				return result;
			}
			public int GetHashCode(IDictionary<char, CharFA<TAccept>> lhs)
			{
				var result = 0;
				foreach (var kvp in lhs)
					result ^= kvp.GetHashCode();
				return result;
			}
			public static readonly _SetComparer Default = new _SetComparer();
		}
	}
}
