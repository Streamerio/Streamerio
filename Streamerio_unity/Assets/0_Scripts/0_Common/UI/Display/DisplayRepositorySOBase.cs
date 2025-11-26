// モジュール概要:
// Display 用プレハブを ScriptableObject 経由で管理し、Presenter ↔ LifetimeScope のマッピングを提供する。
// 依存関係: Alchemy.Inspector で配列を整備し、VContainer LifetimeScope プレハブを解決する。
// 使用例: DisplaySpawner が本リポジトリ経由で UI 用 LifetimeScope を取得し、DI スコープを複製する。

using System;
using System.Collections.Generic;
using System.Linq;
using Alchemy.Inspector;
using UnityEngine;
using VContainer.Unity;

namespace Common.UI.Display
{
    /// <summary>
    /// 【目的】Inspector で登録した UI LifetimeScope プレハブを型に応じて引き当てる。
    /// 【理由】Display ごとに異なる Presenter/LifetimeScope を利用するため、マッピングを明示的に持つ必要がある。
    /// </summary>
    public abstract class DisplayRepositorySOBase : ScriptableObject, IDisplayRepository
    {
        /// <summary>
        /// 【目的】Presenter ごとの LifetimeScope プレハブを Inspector で管理する。
        /// 【理由】VContainer スコープをプレハブ化しておき、生成元をコード変更なく差し替えられるようにするため。
        /// </summary>
        [SerializeField, LabelText("UIのプレファブ")]
        [Tooltip("Presenter ごとに対応する LifetimeScope プレハブ。シーン外から複製して利用する。")]
        private LifetimeScope[] _displayLifetimeScopes;

        /// <summary>
        /// 【目的】Presenter 型と LifetimeScope 型のキャッシュマップを保持する。
        /// 【理由】CreateTypeMap の生成コストを最初の呼び出しだけに抑え、以降の検索を高速化するため。
        /// </summary>
        private Dictionary<Type, Type> _lifetimeScopeTypeMap;

        /// <summary>
        /// 【目的】Presenter 型と LifetimeScope 型の対応表を生成する。
        /// 【理由】派生クラスで各 UI ごとのマッピングを定義できるよう抽象メソッドとして公開する。
        /// </summary>
        /// <returns>【戻り値】Presenter 型をキーとした辞書。</returns>
        protected abstract Dictionary<Type, Type> CreateTypeMap();

        /// <summary>
        /// 【目的】指定された Display 型に対応する LifetimeScope プレハブを取得する。
        /// 【処理概要】
        ///   1. 型マップ未生成なら CreateTypeMap で初期化。
        ///   2. 登録済みプレハブ配列から該当スコープ型を探索。
        ///   3. 見つからない場合はエラーログを出力して null を返す。
        /// 【理由】不整合を早期に検出し、デバッグ時点でマッピング漏れに気付きやすくするため。
        /// </summary>
        /// <typeparam name="T">生成したい Display（Presenter）型。</typeparam>
        /// <returns>【戻り値】対応する LifetimeScope プレハブ。見つからなければ null。</returns>
        public LifetimeScope FindDisplayLifetimeScope<T>()
            where T : IDisplay
        {
            if (_lifetimeScopeTypeMap is null)
                _lifetimeScopeTypeMap = CreateTypeMap();

            if (_displayLifetimeScopes == null || _displayLifetimeScopes.Length == 0)
            {
                Debug.LogError("[DisplayRepository] 登録された LifetimeScope プレハブが空です");
                return null;
            }

            if (_lifetimeScopeTypeMap.TryGetValue(typeof(T), out var scopeType))
            {
                var scope = _displayLifetimeScopes
                    .FirstOrDefault(ls => scopeType.IsAssignableFrom(ls.GetType()));

                if (scope != null)
                    return scope;

                Debug.LogError(
                    $"[DisplayRepository] {typeof(T).Name} 用にマップされた {scopeType.Name} のプレハブが見つかりません");
                return null;
            }

            Debug.LogError($"[DisplayRepository] 型マップに {typeof(T).Name} のエントリがありません");
            return null;
        }
    }

    /// <summary>
    /// 【目的】Display 用プレハブを検索・取得するリポジトリ契約を定義する。
    /// 【理由】DisplaySpawner が型安全に LifetimeScope を取得できるよう、インターフェースで抽象化する。
    /// </summary>
    public interface IDisplayRepository
    {
        /// <summary>
        /// 【目的】指定した Display 型に対応する LifetimeScope を返す。
        /// 【理由】DisplaySpawner が型に応じた UI プレハブを安全に取得するため。
        /// </summary>
        /// <typeparam name="T">【用途】生成対象となる Display（Presenter）型。</typeparam>
        /// <returns>【戻り値】該当プレハブ。未登録の場合は null。</returns>
        LifetimeScope FindDisplayLifetimeScope<T>()
            where T : IDisplay;
    }
}
