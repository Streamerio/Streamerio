// モジュール概要:
// ダイアログ Presenter 用プレハブを管理する ScriptableObject リポジトリ。
// 依存関係: DisplayRepositorySOBase を継承して Presenter ↔ LifetimeScope のマッピングを提供する。
// 使用例: DialogService が本リポジトリ経由で Presenter の生成情報を取得する。

using System;
using System.Collections.Generic;
using Common.UI.Display;
using InGame.QRCode.UI;
using UnityEngine;

namespace Common.UI.Dialog
{
    /// <summary>
    /// 【目的】ダイアログ Presenter と対応する LifetimeScope のマッピングを提供する。
    /// 【理由】DisplayService が Presenter を生成する際に、対応するスコープを解決できるようにするため。
    /// </summary>
    [CreateAssetMenu(fileName = "DialogRepository", menuName = "SO/UI/DialogRepository")]
    public class DialogRepositorySO : DisplayRepositorySOBase
    {
        /// <summary>
        /// 【目的】Presenter 型と LifetimeScope 型との対応表を定義する。
        /// 【理由】DisplayRepositorySOBase がこのマップを利用してダイアログ生成時の依存スコープを決定するため。
        /// </summary>
        /// <returns>【戻り値】Presenter 型をキー、LifetimeScope 型を値とするマッピング辞書。</returns>
        protected override Dictionary<Type, Type> CreateTypeMap()
        {
            return new Dictionary<Type, Type>
            {
                {typeof(IQRCodeDialog), typeof(QRCodeDialogLifetimeScope)},
            };
        }
    }
}
