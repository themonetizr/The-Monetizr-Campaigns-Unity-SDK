using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Monetizr.Campaigns
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        public SerializableDictionary()
        {

        }

        public SerializableDictionary(Dictionary<TKey, TValue> d)
        {
            dictionary = d;
        }

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format($"there are {keys.Count} keys and {values.Count} values after deserialization. Make sure that both key and value types are serializable."));

            for (var i = 0; i < keys.Count; i++)
                dictionary.Add(keys[i], values[i]);
        }

        public TValue GetParam(TKey p)
        {
            return dictionary.TryGetValue(p, out var value) ? value : default(TValue);
        }

        public int GetIntParam(TKey p, int defaultParam = 0)
        {
            if (!dictionary.ContainsKey(p))
                return defaultParam;

            var val = dictionary[p].ToString();

            if (!int.TryParse(val, out var result))
            {
                return defaultParam;
            }

            return result;
        }

        internal void Clear()
        {
            dictionary.Clear();
        }

        internal int RemoveAllByValue(Func<TValue, bool> predicate)
        {
            int count = 0;
            foreach (var item in dictionary.Where(kvp => predicate(kvp.Value)).ToList())
            {
                count++;
                dictionary.Remove(item.Key);
            }

            return count;
        }

        public TValue this[TKey k]
        {
            get => GetParam(k);
            set => dictionary[k] = value;
        }

        public bool ContainsKey(TKey k)
        {
            return dictionary.ContainsKey(k);
        }
    }
}