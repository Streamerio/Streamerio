using System;
using System.Collections.Generic;
using UnityEngine;
using Infra;

namespace Common
{
    /// <summary>
    /// 自動でenumとデータのセットを生成するスクリプタブルオブジェクト
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public abstract class AutoSetDataScriptableObject<TKey, TValue> : ScriptableObject
        where TKey : Enum
    {
        [Header("取得ファイル名の最初の文字が (アルファベット | 日本語 | \"_\") となるように、文字を削除する")]
        /* パス */
        [Header("取得するファイルが入ったフォルダのパス(最後の/は不要)"), SerializeField]
        private string _folderPath = "Assets/";
#if UNITY_EDITOR
        public string FolderPath => _folderPath;
#endif
        [Header("Enumファイルを保存するフォルダのパス(最後の/は不要)"), SerializeField]
        private string _enumPath = "Assets/0_Scripts/";
#if UNITY_EDITOR
        public string EnumPath => _enumPath;
#endif
        
        /* Enum */
        [Header("Enumの名前空間(設定なしの場合は名前空間が生成されない)"), SerializeField]
        private string _enumNameSpace = "";
#if UNITY_EDITOR
        public string EnumNameSpace => _enumNameSpace;  
#endif
        [Header("Enumファイル名"), SerializeField]
        private string _enumFileName = "";
#if UNITY_EDITOR
        public string EnumFileName => _enumFileName;
#endif
        
        [Header("拡張子"), SerializeField]
        private Extention[] _fileExtension;
#if UNITY_EDITOR
        public Extention[] FileExtentions => _fileExtension;
#endif
        
        [Header("取り除くフォルダ名"), SerializeField]
        private string[] ignoreFolderName;
#if UNITY_EDITOR
        public string[] IgnoreFolderName => ignoreFolderName;
#endif
        
        /* 辞書 */
        [SerializeField]
        private SerializeDictionary<TKey, TValue> _dictionary = new ();
        /// <summary>
        /// 辞書
        /// </summary>
        public IReadOnlyDictionary<TKey, TValue> Dictionary => _dictionary.ToDictionary();
        
#if UNITY_EDITOR
        public void Add(TKey key, TValue value)
        {
                if (!Dictionary.ContainsKey(key))
                {
                        _dictionary[key] = value;
                }
        }

        public void ResetDictionary()
        {
            _dictionary.Clear();
        }
#endif
    }
}