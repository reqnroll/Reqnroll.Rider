using System;
using System.Collections;
using System.Collections.Generic;

namespace RE
{
	/// <summary>
	/// Represents a dictionary over a <see cref="IList{T}"/>. Allows null for a key and is explicitely ordered, but unindexed. All searches are linear time.
	/// </summary>
	/// <remarks>Best only to use this for small dictionaries or where indexing by key is infrequent.</remarks>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	class ListDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IList<KeyValuePair<TKey, TValue>>
	{
		IEqualityComparer<TKey> _equalityComparer;
		IList<KeyValuePair<TKey, TValue>> _inner;
		public ListDictionary(IList<KeyValuePair<TKey, TValue>> inner = null, IEqualityComparer<TKey> equalityComparer = null)
		{
			if (null == inner)
				inner = new List<KeyValuePair<TKey, TValue>>();
			_inner = inner;
			_equalityComparer = equalityComparer;
		}
		public TValue this[TKey key] {
			get {
				int i = IndexOfKey(key);
				if (0 > i) throw new KeyNotFoundException();
				return _inner[i].Value;
			}
			set {
				int i = IndexOfKey(key);
				if (0 > i)
					_inner.Add(new KeyValuePair<TKey, TValue>(key, value));
				else
					_inner[i] = new KeyValuePair<TKey, TValue>(key, value);
			}
		}
		public TKey GetKeyAt(int index)
			=> _inner[index].Key;
		public TValue GetAt(int index)
			=> _inner[index].Value;
		public void SetAt(int index, TValue value)
		{
			_inner[index] = new KeyValuePair<TKey, TValue>(_inner[index].Key, value);
		}
		public void SetAt(int index, KeyValuePair<TKey,TValue> item)
		{
			if (ContainsKey(item.Key))
				throw new ArgumentException("An item with the specified key already exists in the dictionary.", nameof(item));
			_inner[index] = item;
		}
		public void SetKeyAt(int index, TKey key)
		{
			if (ContainsKey(key) && index != IndexOfKey(key))
				throw new ArgumentException("An item with the specified key already exists in the dictionary.");
			_inner[index] = new KeyValuePair<TKey, TValue>(key, _inner[index].Value);
				
		}
		
		public void Insert(int index, TKey key, TValue value)
		{
			if (ContainsKey(key))
				throw new ArgumentException("The key already exists in the dictionary.");
			_inner.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
		}
		public ICollection<TKey> Keys { get => new _KeysCollection(_inner, _equalityComparer); }
		public ICollection<TValue> Values { get => new _ValuesCollection(_inner); }
		public int Count { get => _inner.Count; }
		public bool IsReadOnly { get => _inner.IsReadOnly; }

		KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index] {
			get => _inner[index];
			set {
				var i = IndexOfKey(value.Key);
				if (0 > i || i == index)
				{
					_inner[index] = value;
				}
				else
					throw new InvalidOperationException("An item with the specified key already exists in the collection.");
			}
		}

		public void Add(TKey key, TValue value)
			=> Add(new KeyValuePair<TKey, TValue>(key, value));

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			if (ContainsKey(item.Key))
				throw new InvalidOperationException("An item with the specified key already exists in the collection.");
			_inner.Add(item);
		}

		public void Clear()
			=> _inner.Clear();

		public bool Contains(KeyValuePair<TKey, TValue> item)
			=> _inner.Contains(item);

		public bool ContainsKey(TKey key)
			=> -1 < IndexOfKey(key);

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
			=> _inner.CopyTo(array, arrayIndex);

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			=> _inner.GetEnumerator();


		public bool Remove(TKey key)
		{
			var i = IndexOfKey(key);
			if (0 > i) return false;
			_inner.RemoveAt(i);
			return true;
		}
		public int IndexOfKey(TKey key)
		{
			var c = _inner.Count;
			if (null == _equalityComparer)
			{
				for (var i = 0; i < c; ++i)
					if (Equals(_inner[i].Key, key))
						return i;
			}
			else for (var i = 0; i < c; ++i)
					if (_equalityComparer.Equals(_inner[i].Key, key))
						return i;
			return -1;
		}
		public bool Remove(KeyValuePair<TKey, TValue> item)
			=> _inner.Remove(item);

		public bool TryGetValue(TKey key, out TValue value)
		{
			var c = _inner.Count;
			if (null == _equalityComparer)
				for (var i = 0; i < c; ++i)
				{
					var kvp = _inner[i];
					if (Equals(kvp.Key, key))
					{
						value = kvp.Value;
						return true;
					}
				}
			else
				for (var i = 0; i < c; ++i)
				{
					var kvp = _inner[i];
					if (_equalityComparer.Equals(kvp.Key, key))
					{
						value = kvp.Value;
						return true;
					}
				}

			value = default(TValue);
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public int IndexOf(KeyValuePair<TKey, TValue> item)
		{
			return _inner.IndexOf(item);
		}

		void IList<KeyValuePair<TKey,TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
		{
			if (0 > IndexOfKey(item.Key))
				_inner.Insert(index, item);
			else
				throw new InvalidOperationException("An item with the specified key already exists in the collection.");
		}

		public void RemoveAt(int index)
		{
			_inner.RemoveAt(index);
		}

		#region _KeysCollection
		sealed class _KeysCollection : ICollection<TKey>
		{
			IEqualityComparer<TKey> _equalityComparer;
			IList<KeyValuePair<TKey, TValue>> _inner;
			public _KeysCollection(IList<KeyValuePair<TKey, TValue>> inner, IEqualityComparer<TKey> equalityComparer)
			{
				_inner = inner;
				_equalityComparer = equalityComparer;
			}

			public int Count { get => _inner.Count; }
			public bool IsReadOnly { get => true; }

			void ICollection<TKey>.Add(TKey item)
			{
				throw new InvalidOperationException("The collection is read only.");
			}

			void ICollection<TKey>.Clear()
			{
				throw new InvalidOperationException("The collection is read only.");
			}

			public bool Contains(TKey item)
			{
				var c = _inner.Count;
				if (null == _equalityComparer)
				{
					for (var i = 0; i < c; ++i)
						if (Equals(_inner[i].Key, item))
							return true;
				}
				else
					for (var i = 0; i < c; ++i)
						if (_equalityComparer.Equals(_inner[i].Key, item))
							return true;

				return false;
			}

			public void CopyTo(TKey[] array, int arrayIndex)
			{
				var c = _inner.Count;
				if (c > (array.Length - arrayIndex))
					throw new ArgumentOutOfRangeException("arrayIndex");
				for (var i = 0; i < c; ++i)
					array[i + arrayIndex] = _inner[i].Key;
			}

			public IEnumerator<TKey> GetEnumerator()
			{
				foreach (var kvp in _inner)
					yield return kvp.Key;
			}

			bool ICollection<TKey>.Remove(TKey item)
			{
				throw new InvalidOperationException("The collection is read only.");
			}

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();
		}
		#endregion

		#region _ValuesCollection
		sealed class _ValuesCollection : ICollection<TValue>
		{
			IList<KeyValuePair<TKey, TValue>> _inner;
			public _ValuesCollection(IList<KeyValuePair<TKey, TValue>> inner)
			{
				_inner = inner;
			}

			public int Count { get => _inner.Count; }
			public bool IsReadOnly { get => true; }

			void ICollection<TValue>.Add(TValue item)
			{
				throw new InvalidOperationException("The collection is read only.");
			}

			void ICollection<TValue>.Clear()
			{
				throw new InvalidOperationException("The collection is read only.");
			}

			public bool Contains(TValue item)
			{
				var c = _inner.Count;

				for (var i = 0; i < c; ++i)
					if (Equals(_inner[i].Value, item))
						return true;
				return false;
			}

			public void CopyTo(TValue[] array, int arrayIndex)
			{
				var c = _inner.Count;
				if (c > (array.Length - arrayIndex))
					throw new ArgumentOutOfRangeException("arrayIndex");
				for (var i = 0; i < c; ++i)
					array[i + arrayIndex] = _inner[i].Value;
			}

			public IEnumerator<TValue> GetEnumerator()
			{
				foreach (var kvp in _inner)
					yield return kvp.Value;
			}

			bool ICollection<TValue>.Remove(TValue item)
			{
				throw new InvalidOperationException("The collection is read only.");
			}

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();
		}
		#endregion
	}
}
