using System;
using System.Collections;
using System.Collections.Generic;

namespace ArmyAnt.Generics
{
    [Serializable]
    public class Serialized<T> : IList<T>
    {
#if UNITY_ENGINE
        [UnityEngine.SerializeField]
#endif
        List<T> target;

        public int Count => target.Count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => target[index];
            set => target[index] = value;
        }

        public int IndexOf(T item) => target.IndexOf(item);

        public void Insert(int index, T item) => target.Insert(index, item);

        public void RemoveAt(int index) => target.RemoveAt(index);

        public void Add(T item) => target.Add(item);

        public void Clear() => target.Clear();

        public bool Contains(T item) => target.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => target.CopyTo(array, arrayIndex);

        public bool Remove(T item) => target.Remove(item);

        public IEnumerator<T> GetEnumerator() => target.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Serialized(List<T> target)
        {
            this.target = target;
        }
    }

    [Serializable]
    public class Serialized<TKey, TValue> : IDictionary<TKey, TValue>
#if UNITY_ENGINE
        ,UnityEngine.ISerializationCallbackReceiver
#endif
    {
#if UNITY_ENGINE
        [UnityEngine.SerializeField]
#endif
        List<TKey> keys;
#if UNITY_ENGINE
        [UnityEngine.SerializeField]
#endif
        List<TValue> values;

        Dictionary<TKey, TValue> target;

        public ICollection<TKey> Keys => keys;

        public ICollection<TValue> Values => values;

        public int Count => target.Count;

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get => target[key];
            set => Add(key, value);
        }

        public Serialized(Dictionary<TKey, TValue> target)
        {
            this.target = target;
            //OnBeforeSerialize();
        }

        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(target.Keys);
            values = new List<TValue>(target.Values);
        }

        public void OnAfterDeserialize()
        {
            var count = Math.Min(keys.Count, values.Count);
            target = new Dictionary<TKey, TValue>(count);
            for (var i = 0; i < count; ++i)
            {
                target.Add(keys[i], values[i]);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (!target.ContainsKey(key))
            {
                keys.Add(key);
                values.Add(value);
            }
            else
            {
                var index = keys.IndexOf(key);
                values[index] = value;
            }
            target.Add(key, value);
        }

        public bool ContainsKey(TKey key) => target.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (target.ContainsKey(key))
            {
                var index = keys.IndexOf(key);
                keys.RemoveAt(index);
                values.RemoveAt(index);
            }
            return target.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var ret = target.TryGetValue(key, out value);
            return ret;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
            target.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return keys.Contains(item.Key) && values.Contains(item.Value) && target[item.Key].Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int index = 0;
            foreach(var i in target)
            {
                if (arrayIndex > index)
                {
                    array[index++] = i;
                }
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return target.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => target.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
