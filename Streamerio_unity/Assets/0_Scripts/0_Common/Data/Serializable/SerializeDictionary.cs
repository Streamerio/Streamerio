using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// シリアライズ可できる辞書
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class SerializeDictionary<TKey, TValue>: ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<KeyValuePair> _keyValuePairs;
        
        private Dictionary<TKey, TValue> _dictionary;
        
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                _dictionary[key] = value;

                int index = _keyValuePairs.FindIndex(kv => kv.Key.Equals(key));
                if (index >= 0)
                {
                    _keyValuePairs.RemoveAt(index);
                }
                _keyValuePairs.Add(new KeyValuePair(key, value));
            }
        }
        
        public SerializeDictionary()
        {
            _keyValuePairs = new List<KeyValuePair>();
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public SerializeDictionary(Dictionary<TKey, TValue> dictionary)
        {
            _keyValuePairs = new List<KeyValuePair>();
            _dictionary = new Dictionary<TKey, TValue>();
            
            foreach (var kv in dictionary)
            {
                _keyValuePairs.Add(new KeyValuePair(kv.Key, kv.Value));
                _dictionary[kv.Key] = kv.Value;
            }
        }
        
        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            _dictionary.Clear();
            foreach (var kv in _keyValuePairs)
            {
                _dictionary[kv.Key] = kv.Value;
            }
        }

        /// <summary>
        /// 辞書にキーが登録されているか
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 辞書を初期化
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
            _keyValuePairs.Clear();
        }

        /// <summary>
        /// 辞書型に変換
        /// </summary>
        /// <returns></returns>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            return _keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
        
        [Serializable]
        public class KeyValuePair
        {
            [SerializeField]
            private TKey _key;

            public TKey Key => _key;
            
            [SerializeField]
            private TValue _value;
            public TValue Value => _value;

            public KeyValuePair()
            {
                _key = default(TKey);
                _value = default(TValue);
            }

            public KeyValuePair(TKey key, TValue value)
            {
                _key = key;
                _value = value;
            }
        }
    }
}