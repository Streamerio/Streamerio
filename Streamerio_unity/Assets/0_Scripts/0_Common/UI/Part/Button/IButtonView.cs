// モジュール概要:
// ボタン View が満たすべき契約を定義し、Presenter からの演出指示 API を統一する。
// 依存関係: Cysharp.Threading.Tasks を利用して非同期アニメーションを扱う。
// 使用例: CommonButtonView や TextCommonButton が実装し、CommonButtonPresenter から呼び出される。

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// 【目的】共通ボタン View が提供する必要のある API を明示する。
    /// 【理由】Presenter 側が具体クラスへ依存せず、演出指示を一貫した形で呼び出すため。
    /// </summary>
    public interface IButtonView
    {
        /// <summary>
        /// 【目的】関連する GameObject インスタンスを公開する。
        /// 【理由】表示切り替えなどの操作を Presenter から行えるようにするため。
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// 【目的】PointerDown 時の演出を非同期で実行する。
        /// 【理由】押下フィードバックを View ごとの挙動に委ねるため。
        /// </summary>
        /// <param name="ct">【用途】演出途中で処理を中断するための CancellationToken。</param>
        /// <returns>【戻り値】演出完了を示す UniTask。</returns>
        UniTask PlayPointerDownAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】PointerUp 時の演出を非同期で実行する。
        /// 【理由】押下前の状態へ戻す演出を各 View が実装できるようにするため。
        /// </summary>
        /// <param name="ct">【用途】演出途中で処理を中断するための CancellationToken。</param>
        /// <returns>【戻り値】演出完了を示す UniTask。</returns>
        UniTask PlayPointerUpAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】PointerEnter 時の演出を非同期で実行する。
        /// 【理由】ホバー状態の表現を View ごとに差し替えられるようにするため。
        /// </summary>
        /// <param name="ct">【用途】演出途中で処理を中断するための CancellationToken。</param>
        /// <returns>【戻り値】演出完了を示す UniTask。</returns>
        UniTask PlayPointerEnterAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】PointerExit 時の演出を非同期で実行する。
        /// 【理由】ホバー解除時の見た目復帰を View 内に閉じ込めるため。
        /// </summary>
        /// <param name="ct">【用途】演出途中で処理を中断するための CancellationToken。</param>
        /// <returns>【戻り値】演出完了を示す UniTask。</returns>
        UniTask PlayPointerExitAsync(CancellationToken ct);
        
        /// <summary>
        /// 【目的】PointerClick 時の演出を非同期で実行する。
        /// 【理由】クリック時のフィードバックを View ごとにカスタマイズできるようにするため。
        /// </summary>
        /// <param name="ct">【用途】演出途中で処理を中断するための CancellationToken。</param>
        /// <returns>【戻り値】演出完了を示す UniTask。</returns>
        UniTask PlayPointerClickAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】ボタンの演出状態を既定値へリセットする。
        /// 【理由】表示切り替えや連続操作後に状態が残らないようにするため。
        /// </summary>
        void ResetButtonState();
    }
}
