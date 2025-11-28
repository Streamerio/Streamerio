// モジュール概要:
// Display Presenter を生成・キャッシュし、再利用するためのインフラを提供する。
// 依存関係: IDisplaySpawner で生成処理を委譲し、Dictionary で型ごとのインスタンスを管理する。
// 使用例: DisplayServiceContext が本キャッシュを保持し、OpenDisplay 系 API から Presenter を取得する。

using System;
using System.Collections.Generic;

namespace Common.UI.Display
{
    /// <summary>
    /// 【目的】Display Presenter を取得・キャッシュする契約を定義する。
    /// 【理由】生成コストの高い UI を再利用し、GC 負荷や Instantiate コストを抑えるため。
    /// </summary>
    public interface IDisplayCache
    {
        /// <summary>
        /// 【目的】指定した Display 型のインスタンスを取得する。
        /// 【理由】キャッシュ済みインスタンスを再利用し、描画と初期化負荷を抑えるため。
        /// </summary>
        /// <typeparam name="TDisplay">【用途】要求する Display の型。</typeparam>
        /// <returns>【戻り値】キャッシュ済みインスタンス、存在しない場合は生成されたインスタンス。</returns>
        TDisplay GetDisplay<TDisplay>() 
            where TDisplay : IDisplay;
    }
    
    /// <summary>
    /// 【目的】Display の生成とキャッシュ管理を実装する。
    /// 【理由】Spawn ロジックを一元化して再利用性を高めつつ、型ごとのキャッシュを維持するため。
    /// </summary>
    public class DisplayCache : IDisplayCache
    {
        /// <summary>
        /// 【目的】Display を生成するためのスパナーを保持する。
        /// 【理由】生成手順を委譲し、キャッシュ側は生成タイミングの制御に集中するため。
        /// </summary>
        private readonly IDisplayFactory _factory;

        /// <summary>
        /// 【目的】Presenter を型ごとに保持し、再度の生成を避ける。
        /// 【理由】Instantiate や依存解決を繰り返さず、描画レスポンスを一定に保つため。
        /// </summary>
        private readonly Dictionary<Type, IDisplay> _displayCache;
        
        /// <summary>
        /// 【目的】DisplaySpawner を受け取り、キャッシュを初期化する。
        /// 【理由】生成手順を外部に委譲しつつ、キャッシュの初期状態を空にしておく。
        /// </summary>
        /// <param name="factory">【用途】Display プレハブを生成するスパナー。</param>
        public DisplayCache(IDisplayFactory factory)
        {
            _factory = factory;
            _displayCache = new();
        }
        
        /// <summary>
        /// 【目的】Display インスタンスを取得し、未生成なら生成してキャッシュする。
        /// 【処理概要】辞書から探索→存在すれば返却→なければ Spawn→キャッシュ登録→返却。
        /// 【理由】Presenter 初期化コストを抑え、再表示時のパフォーマンスを安定させる。
        /// </summary>
        /// <typeparam name="TDisplay">【用途】取得したい Display (Presenter) 型。</typeparam>
        /// <returns>【戻り値】キャッシュに存在する、または新規生成された Display。</returns>
        public TDisplay GetDisplay<TDisplay>()
            where TDisplay : IDisplay
        {
            if (_displayCache.TryGetValue(typeof(TDisplay), out var existDisplay))
            {
                return (TDisplay)existDisplay;
            }
            
            var display = _factory.Create<TDisplay>();
            _displayCache.Add(typeof(TDisplay), display);
            
            return display;
        }
    }
}
