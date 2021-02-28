using System;
using System.Collections;
using System.Collections.Generic;

namespace RE
{
	// a specialized input transition container dictionary.
	// this isn't required for a working regex engine but 
	// can make some common operations significantly faster.
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// This is a specialized transition container that can return its transitions in 3 different ways:
		/// 1. a dictionary where each transition state is keyed by an individual input character (default)
		/// 2. a dictionary where each collection of inputs is keyed by the transition state (used mostly by optimizations)
		/// 3. an indexable list of pairs where the key is the transition state and the value is the collection of inputs
		/// use casts to get at the appropriate interface for your operation.
		/// </summary>
		private class _InputTransitionDictionary :
			IDictionary<char, CharFA<TAccept>>, // #1
			IDictionary<CharFA<TAccept>, ICollection<char>>, // #2
			IList<KeyValuePair<CharFA<TAccept>, ICollection<char>>> // #3
		{
			IDictionary<CharFA<TAccept>, ICollection<char>> _inner =
				new ListDictionary<CharFA<TAccept>, ICollection<char>>();

			public CharFA<TAccept> this[char key] {
				get {
					foreach (var trns in _inner)
					{
						if (trns.Value.Contains(key))
							return trns.Key;
					}
					throw new KeyNotFoundException();
				}
				set {
					Remove(key);
					ICollection<char> hs;
					if (_inner.TryGetValue(value, out hs))
					{
						hs.Add(key);
					}
					else
					{
						hs = new HashSet<char>();
						hs.Add(key);
						_inner.Add(value, hs);
					}
				}
			}

			public ICollection<char> Keys {
				get {
					return new _KeysCollection(_inner);
				}

			}

			sealed class _KeysCollection : ICollection<char>
			{
				IDictionary<CharFA<TAccept>, ICollection<char>> _inner;
				public _KeysCollection(IDictionary<CharFA<TAccept>, ICollection<char>> inner)
				{
					_inner = inner;
				}
				public int Count {
					get {
						var result = 0;
						foreach (var val in _inner.Values)
							result += val.Count;
						return result;
					}
				}
				void _ThrowReadOnly() { throw new NotSupportedException("The collection is read-only."); }
				public bool IsReadOnly => true;

				public void Add(char item)
				{
					_ThrowReadOnly();
				}

				public void Clear()
				{
					_ThrowReadOnly();
				}

				public bool Contains(char item)
				{
					foreach (var val in _inner.Values)
						if (val.Contains(item))
							return true;
					return false;
				}

				public void CopyTo(char[] array, int arrayIndex)
				{
					var si = arrayIndex;
					foreach (var val in _inner.Values)
					{
						val.CopyTo(array, si);
						si += val.Count;
					}
				}

				public IEnumerator<char> GetEnumerator()
				{
					foreach (var val in _inner.Values)
						foreach (var ch in val)
							yield return ch;
				}

				public bool Remove(char item)
				{
					_ThrowReadOnly();
					return false;
				}

				IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			}
			sealed class _ValuesCollection : ICollection<CharFA<TAccept>>
			{
				IDictionary<CharFA<TAccept>, ICollection<char>> _inner;
				public _ValuesCollection(IDictionary<CharFA<TAccept>, ICollection<char>> inner)
				{
					_inner = inner;
				}
				public int Count {
					get {
						var result = 0;
						foreach (var val in _inner.Values)
							result += val.Count;
						return result;
					}
				}
				void _ThrowReadOnly() { throw new NotSupportedException("The collection is read-only."); }
				public bool IsReadOnly => true;

				public void Add(CharFA<TAccept> item)
				{
					_ThrowReadOnly();
				}

				public void Clear()
				{
					_ThrowReadOnly();
				}

				public bool Contains(CharFA<TAccept> item)
				{
					return _inner.Keys.Contains(item);
				}

				public void CopyTo(CharFA<TAccept>[] array, int arrayIndex)
				{
					var si = arrayIndex;
					foreach (var trns in _inner)
					{
						foreach (var ch in trns.Value)
						{
							array[si] = trns.Key;
							++si;
						}
					}
				}

				public IEnumerator<CharFA<TAccept>> GetEnumerator()
				{
					foreach (var trns in _inner)
						foreach (var ch in trns.Value)
							yield return trns.Key;
				}

				public bool Remove(CharFA<TAccept> item)
				{
					_ThrowReadOnly();
					return false;
				}

				IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			}
			public ICollection<CharFA<TAccept>> Values => new _ValuesCollection(_inner);

			public int Count {
				get {
					var result = 0;
					foreach (var trns in _inner)
						result += trns.Value.Count;
					return result;
				}
			}
			IList<KeyValuePair<CharFA<TAccept>, ICollection<char>>> _InnerList => _inner as IList<KeyValuePair<CharFA<TAccept>, ICollection<char>>>;
			ICollection<CharFA<TAccept>> IDictionary<CharFA<TAccept>, ICollection<char>>.Keys => _inner.Keys;
			ICollection<ICollection<char>> IDictionary<CharFA<TAccept>, ICollection<char>>.Values => _inner.Values;
			int ICollection<KeyValuePair<CharFA<TAccept>, ICollection<char>>>.Count => _inner.Count;
			public bool IsReadOnly => _inner.IsReadOnly;

			KeyValuePair<CharFA<TAccept>, ICollection<char>> IList<KeyValuePair<CharFA<TAccept>, ICollection<char>>>.this[int index] { get => _InnerList[index]; set { _InnerList[index] = value; } }

			ICollection<char> IDictionary<CharFA<TAccept>, ICollection<char>>.this[CharFA<TAccept> key] { get { return _inner[key]; } set { _inner[key] = value; } }

			public void Add(char key, CharFA<TAccept> value)
			{
				if (null == value)
					throw new ArgumentNullException(nameof(value));
				if (ContainsKey(key))
					throw new InvalidOperationException("The key is already present in the dictionary.");
				if (null == value)
					throw new ArgumentNullException(nameof(value));
				ICollection<char> hs;
				if (_inner.TryGetValue(value, out hs))
				{
					hs.Add(key);
				}
				else
				{
					hs = new HashSet<char>();
					hs.Add(key);
					_inner.Add(value, hs);
				}
			}

			public void Add(KeyValuePair<char, CharFA<TAccept>> item)
				=> Add(item.Key, item.Value);


			public void Clear()
				=> _inner.Clear();


			public bool Contains(KeyValuePair<char, CharFA<TAccept>> item)
			{
				ICollection<char> hs;
				return _inner.TryGetValue(item.Value, out hs) && hs.Contains(item.Key);
			}

			public bool ContainsKey(char key)
			{
				foreach (var trns in _inner)
				{
					if (trns.Value.Contains(key))
						return true;
				}
				return false;
			}

			public void CopyTo(KeyValuePair<char, CharFA<TAccept>>[] array, int arrayIndex)
			{
				using (var e = ((IEnumerable<KeyValuePair<char, CharFA<TAccept>>>)this).GetEnumerator())
				{
					var i = arrayIndex;
					while (e.MoveNext())
					{
						array[i] = e.Current;
						++i;
					}
				}
			}

			public IEnumerator<KeyValuePair<char, CharFA<TAccept>>> GetEnumerator()
			{
				foreach (var trns in _inner)
					foreach (var ch in trns.Value)
						yield return new KeyValuePair<char, CharFA<TAccept>>(ch, trns.Key);
			}

			public bool Remove(char key)
			{
				CharFA<TAccept> rem = null;
				foreach (var trns in _inner)
				{
					if (trns.Value.Contains(key))
					{
						trns.Value.Remove(key);
						if (0 == trns.Value.Count)
						{
							rem = trns.Key;
							break;
						}
						return true;
					}
				}
				if (null != rem)
				{
					_inner.Remove(rem);
					return true;
				}
				return false;
			}

			public bool Remove(KeyValuePair<char, CharFA<TAccept>> item)
			{
				ICollection<char> hs;
				if (_inner.TryGetValue(item.Value, out hs))
				{
					if (hs.Contains(item.Key))
					{
						if (1 == hs.Count)
							_inner.Remove(item.Value);
						else
							hs.Remove(item.Key);
						return true;
					}
				}
				return false;
			}

			public bool TryGetValue(char key, out CharFA<TAccept> value)
			{
				foreach (var trns in _inner)
				{
					if (trns.Value.Contains(key))
					{
						value = trns.Key;
						return true;
					}
				}
				value = null;
				return false;
			}

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();

			void IDictionary<CharFA<TAccept>, ICollection<char>>.Add(CharFA<TAccept> key, ICollection<char> value)
			{
				if (null == value)
					throw new ArgumentNullException(nameof(value));
				_inner.Add(key, value);
			}

			bool IDictionary<CharFA<TAccept>, ICollection<char>>.ContainsKey(CharFA<TAccept> key)
				=> _inner.ContainsKey(key);

			bool IDictionary<CharFA<TAccept>, ICollection<char>>.Remove(CharFA<TAccept> key)
				=> _inner.Remove(key);


			bool IDictionary<CharFA<TAccept>, ICollection<char>>.TryGetValue(CharFA<TAccept> key, out ICollection<char> value)
				=> _inner.TryGetValue(key, out value);


			void ICollection<KeyValuePair<CharFA<TAccept>, ICollection<char>>>.Add(KeyValuePair<CharFA<TAccept>, ICollection<char>> item)
			{
				if (null == item.Key)
					throw new ArgumentNullException(nameof(item), "The state cannot be null");
				if (null == item.Value)
					throw new ArgumentNullException(nameof(item),"The collection cannot be null");
				_inner.Add(item);
			}

			bool ICollection<KeyValuePair<CharFA<TAccept>, ICollection<char>>>.Contains(KeyValuePair<CharFA<TAccept>, ICollection<char>> item)
				=> _inner.Contains(item);


			void ICollection<KeyValuePair<CharFA<TAccept>, ICollection<char>>>.CopyTo(KeyValuePair<CharFA<TAccept>, ICollection<char>>[] array, int arrayIndex)
				=> _inner.CopyTo(array, arrayIndex);


			bool ICollection<KeyValuePair<CharFA<TAccept>, ICollection<char>>>.Remove(KeyValuePair<CharFA<TAccept>, ICollection<char>> item)
				=> _inner.Remove(item);


			IEnumerator<KeyValuePair<CharFA<TAccept>, ICollection<char>>> IEnumerable<KeyValuePair<CharFA<TAccept>, ICollection<char>>>.GetEnumerator()
			=> _inner.GetEnumerator();


			int IList<KeyValuePair<CharFA<TAccept>, ICollection<char>>>.IndexOf(KeyValuePair<CharFA<TAccept>, ICollection<char>> item)
				=> _InnerList.IndexOf(item);

			void IList<KeyValuePair<CharFA<TAccept>, ICollection<char>>>.Insert(int index, KeyValuePair<CharFA<TAccept>, ICollection<char>> item)
			{
				if (null == item.Key)
					throw new ArgumentNullException(nameof(item),"The state cannot be null");
				if (null == item.Value)
					throw new ArgumentNullException(nameof(item), "The collection cannot be null");
				_InnerList.Insert(index, item);
			}

			void IList<KeyValuePair<CharFA<TAccept>, ICollection<char>>>.RemoveAt(int index)
				=> _InnerList.RemoveAt(index);
		}
	}
}
