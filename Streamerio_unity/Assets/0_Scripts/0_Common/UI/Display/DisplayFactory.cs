// モジュール概要:
// Display 用 LifetimeScope を生成し、Presenter を解決するスパナークラス。
// 依存関係: IDisplayRepository でプレハブを解決し、VContainer の EnqueueParent を利用して親スコープを維持する。
// 使用例: DisplayLifetimeScope が DisplayCache を初期化する際に本クラスを渡し、UI を遅延生成する。

using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Display
{
    /// <summary>
    /// 【目的】Display プレハブを要求されたタイミングで生成する契約を定義する。
    /// </summary>
    public interface IDisplayFactory
    {
        /// <summary>
        /// 【目的】指定した Display 型を生成して返す。
        /// 【理由】プレハブ生成と依存解決を呼び出し側から隠蔽し、 Presenter 入手手続きを統一するため。
        /// </summary>
        /// <typeparam name="TDisplay">【用途】生成したい Display (Presenter) 型。</typeparam>
        /// <returns>【戻り値】VContainer で解決済みの Presenter。</returns>
        TDisplay Create<TDisplay>()
            where TDisplay : IDisplay;
    }
    
    /// <summary>
    /// 【目的】DisplayRepository から LifetimeScope プレハブを取得し、生成・解決までを一括で行う。
    /// 【理由】生成手順を集中管理し、親スコープの継承漏れや依存解決ミスを防ぐ。
    /// </summary>
    public class DisplayFactory : IDisplayFactory
    {
        /// <summary>
        /// 【目的】LifetimeScope プレハブの検索に利用するリポジトリ参照を保持する。
        /// 【理由】生成のたびに依存を解決し直すコストを避け、再利用性を高めるため。
        /// </summary>
        private readonly IDisplayRepository _repository;
        /// <summary>
        /// 【目的】生成した UI の親 Transform を保持する。
        /// 【理由】Instantiate 時に正しい階層へ配置し、Canvas 上のレンダリング順を維持するため。
        /// </summary>
        private readonly Transform _parent;
        /// <summary>
        /// 【目的】DI の親スコープを保持し、新しい LifetimeScope へ継承させる。
        /// 【理由】UI プレハブが解決する依存を親スコープで共有し、意図しない孤立を防ぐため。
        /// </summary>
        private readonly LifetimeScope _parentScope;
        
        /// <summary>
        /// 【目的】生成に必要な依存（リポジトリ・親 Transform・親スコープ）を受け取る。
        /// 【理由】生成先と DI の親子関係を明示して、UI ツリー内の整合性を保つ。
        /// </summary>
        /// <param name="repository">【用途】Presenter と LifetimeScope の対応を検索するリポジトリ。</param>
        /// <param name="parent">【用途】生成した UI をぶら下げる Transform。</param>
        /// <param name="parentScope">【用途】依存継承に使用する親 LifetimeScope。</param>
        public DisplayFactory(IDisplayRepository repository, Transform parent, LifetimeScope parentScope)
        {
            _repository = repository;
            _parent = parent;
            _parentScope = parentScope;
        }

        /// <summary>
        /// 【目的】Display Presenter を生成し、依存解決済みのインスタンスを返す。
        /// 【処理概要】
        ///   1. 対応する LifetimeScope プレハブをリポジトリから取得。
        ///   2. 親スコープを EnqueueParent で引き継ぎつつ Instantiate。
        ///   3. 生成した Scope から Presenter を Resolve して返却。
        /// 【理由】UI ごとに異なる DI 設定を安全に再利用するため。
        /// </summary>
        /// <typeparam name="TDisplay">【用途】生成したい Display (Presenter) 型。</typeparam>
        /// <returns>【戻り値】生成と依存解決を終えた Presenter。取得に失敗した場合は default。</returns>
        public TDisplay Create<TDisplay>()
            where TDisplay : IDisplay
        {
            var displayScope = _repository.FindDisplayLifetimeScope<TDisplay>();
            if (displayScope == null)
            {
                Debug.LogError($"[DisplaySpawner] {typeof(TDisplay).Name} に対応する LifetimeScope が見つかりません");
                return default;
            }

            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var instance = Object.Instantiate(displayScope, _parent);

                var display = instance.Container.Resolve<TDisplay>();
                display.Hide();
                return display;
            }
        }
    }
}
