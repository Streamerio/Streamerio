using Infra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Common.Editor
{
    /// <summary>
    /// フォルダ内のアセットから <see cref="AutoSetDataScriptableObject{TKey,TValue}"/> の辞書と対応する Enum を
    /// 自動的に生成・更新するための抽象エディタ。
    /// <para>
    /// ボタン一つでフォルダを走査し、Enum を再生成したうえで辞書を更新します。<br/>
    /// 除外フォルダ・複数拡張子・None 要素の自動追加など柔軟な設定が可能です。
    /// </para>
    /// </summary>
    /// <typeparam name="TKey">辞書キーとして使用する Enum 型。</typeparam>
    /// <typeparam name="TValue">辞書に格納する値の型。</typeparam>
    /// <typeparam name="TFile">フォルダからロードするファイルの型（例：Texture, AudioClip など）。</typeparam>
    /// <typeparam name="TSO">編集対象の ScriptableObject 型。</typeparam>
    public abstract class AutoSetDataScriptableObjectEditor<TKey, TValue, TFile, TSO> : UnityEditor.Editor
        where TKey : Enum
        where TFile : UnityEngine.Object
        where TSO : AutoSetDataScriptableObject<TKey, TValue>
    {
        /// <summary>
        /// 現在インスペクタで編集対象となっている ScriptableObject。
        /// </summary>
        protected TSO Target;

        /// <summary>Enum の先頭に追加する「None」項目の定義。</summary>
        private const string NoneParam = "None";

        /// <summary>
        /// true の場合、Enum の先頭に「None」を追加して Enum と辞書を生成します。
        /// </summary>
        protected bool IsSetNone = false;

        /// <summary>
        /// エディタが有効化されたときに呼ばれ、対象オブジェクトをキャッシュします。
        /// </summary>
        protected virtual void OnEnable()
        {
            Target = target as TSO;
        }

        /// <summary>
        /// インスペクタ描画処理。通常のプロパティ＋「登録」「リセット」ボタンを表示します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            var setDataButton = GUILayout.Button("辞書にデータを登録(重複は上書き)");
            var resetButton   = GUILayout.Button("リセット");
            
            base.OnInspectorGUI();

            if (setDataButton)
            {
                SetData();
            }

            if (resetButton)
            {
                Reset();
            }
        }

        /// <summary>
        /// 対象フォルダを走査し、Enum 生成 → 辞書更新 → ScriptableObject 保存まで一括で行います。
        /// </summary>
        private void SetData()
        {
            // 1. 対象ファイルのパスを収集
            string[] filePaths = GetPaths();
            
            // 2. Enum 値候補と TFile を読み込み
            LoadFiles(filePaths, out var enumParams, out var files);

            // 3. Enum ファイルを生成
            CreateEnumFile(enumParams);

            // 4. 辞書を更新（既存キーはスキップ）
            CreateDictionary(files, enumParams);

            // 5. ScriptableObject を保存
            Save();
            
            Debug.Log("辞書に登録完了");
        }

        /// <summary>
        /// ScriptableObject の辞書を初期状態に戻し、アセットを保存します。
        /// </summary>
        private void Reset()
        {
            Target.ResetDictionary();
            Save();
            Debug.Log("Reset Data");
        }
        
        /// <summary>
        /// 対象フォルダ内から、指定拡張子にマッチするファイルパスをすべて取得します。
        /// 除外フォルダ名がパスに含まれている場合はスキップします。
        /// </summary>
        private string[] GetPaths()
        {
            var paths = new List<string>();

            foreach (var extension in Target.FileExtentions)
            {
                var allPaths = Directory.GetFiles(
                    Target.FolderPath,
                    extension.ToPattern(),
                    SearchOption.AllDirectories
                );
                
                foreach (var path in allPaths)
                {
                    if (ContainsFolder(path, Target.IgnoreFolderName))
                        continue;
                    
                    paths.Add(path);
                }
            }
            
            return paths.ToArray();
        }
        
        /// <summary>
        /// 指定したパスが、指定したフォルダ名を含むかどうかを判定します。
        /// </summary>
        /// <param name="path">調査するフルパス。</param>
        /// <param name="folderNames">一致を調べるフォルダ名の一覧。</param>
        private bool ContainsFolder(string path, params string[] folderNames)
        {
            var directories = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            foreach (var folderName in folderNames)
            {
                if (directories.Contains(folderName, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ファイルパスから Enum 名候補（パラメータ）と対応するファイルオブジェクトをロードします。
        /// </summary>
        /// <param name="paths">対象ファイルパス一覧。</param>
        /// <param name="enumParams">Enum の値名を格納する配列。</param>
        /// <param name="files">ロードした TFile を格納する配列。</param>
        private void LoadFiles(string[] paths, out string[] enumParams, out TFile[] files)
        {
            int fileCount = paths.Length;
            int offset = 0;

            if (IsSetNone)
            {
                enumParams = new string[fileCount + 1];
                enumParams[0] = NoneParam;
                offset = 1;
            }
            else
            {
                enumParams = new string[fileCount];
            }

            files = new TFile[fileCount];

            for (int i = 0; i < fileCount; i++)
            {
                string path = paths[i];

                // ファイル名（拡張子を除く）
                var fileName = Path.GetFileNameWithoutExtension(path);

                // Enum 名に適さない文字を除去（空白、スラッシュなど）
                enumParams[i + offset] = fileName
                    .Replace(" ", string.Empty)
                    .Replace("/", string.Empty);
                
                // AssetDatabase から実体をロード
                files[i] = AssetDatabase.LoadAssetAtPath<TFile>(path);
            }
        }
        
        /// <summary>
        /// Enum ファイルを生成します。
        /// </summary>
        /// <param name="enumParams">生成する Enum の値名リスト。</param>
        private void CreateEnumFile(string[] enumParams)
        {
            EnumUtil.CreateEnum(enumParams, Target.EnumNameSpace, Target.EnumFileName, Target.EnumPath);
        }

        /// <summary>
        /// Enum とファイル対応に基づいて辞書を更新します。既存キーはスキップされます。
        /// </summary>
        /// <param name="files">ロードした TFile の配列。</param>
        /// <param name="enumParams">Enum 名の配列。</param>
        private void CreateDictionary(TFile[] files, string[] enumParams)
        {
            int length = files.Length;
            int offset = IsSetNone ? 1 : 0;
            
            for (int i = 0; i < length; i++)
            {
                var key = (TKey)Enum.Parse(typeof(TKey), enumParams[i + offset]);
                var value = CreateValue(files[i]);
                
                Target.Add(key, value);
            }
        }
        
        /// <summary>
        /// 辞書に登録する値を生成します。
        /// 派生クラスでファイルから TValue を作る処理を実装してください。
        /// </summary>
        /// <param name="file">元になる TFile アセット。</param>
        /// <returns>辞書に登録する値。</returns>
        protected abstract TValue CreateValue(TFile file);

        /// <summary>
        /// ScriptableObject を保存し、エディタを更新します。
        /// </summary>
        private void Save()
        {
            EditorUtility.SetDirty(Target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Repaint();

            Debug.Log("Save Data");
        }
    }
}