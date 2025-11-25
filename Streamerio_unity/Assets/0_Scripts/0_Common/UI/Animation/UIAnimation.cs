// ============================================================================
// モジュール概要: UI アニメーション共通のインターフェースと DOTween ベースの基底クラス、およびパラメータ ScriptableObject を提供する。
// 外部依存: Cysharp.Threading.Tasks、DG.Tweening、UnityEngine。
// 使用例: 各種 UI 演出コンポーネントが IUIAnimationComponent を実装し、SequenceAnimationComponentBase で再生ロジックを共有する。
// ============================================================================

using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// UI アニメーションの基底インターフェース。
    /// - 各種 UI アニメーションコンポーネントが実装する
    /// - 非同期でアニメーションを実行し、キャンセル対応も可能
    /// <para>
    /// 【理由】演出処理を統一 API で扱い、Presenter から await 可能にするため。
    /// </para>
    /// </summary>
    public interface IUIAnimation
    {
        /// <summary>
        /// アニメーションを再生する。
        /// - UniTask を返すので await 可能
        /// - CancellationToken による中断に対応
        /// <para>
        /// 【理由】演出完了後に処理を進めたいケースに対応し、キャンセルも考慮する共通契約とする。
        /// </para>
        /// </summary>
        UniTask PlayAsync(CancellationToken ct, bool useInitial = true);
        
        void PlayImmediate();

        void Skip();
    }

    public interface IInitializableAnimation
    {
        void InitializeAnimation();
    }
    
    /// <summary>
    /// DOTween Sequence を使ったアニメーションの基底クラス。
    /// - コンストラクタで Sequence を生成し、AutoKill を無効化して使い回し可能に設定
    /// - 派生クラスはコンストラクタ内で Sequence にアニメーションを組み立てる
    /// - PlayAsync で Restart して最初から再生し、完了まで待機できる
    /// </summary>
    public abstract class SequenceAnimationBase : IUIAnimation, IDisposable
    {
        /// <summary>
        /// 管理対象の DOTween Sequence。
        /// - SetAutoKill(false) により一度完了しても破棄されず、再利用可能
        /// - Pause() により生成直後は停止状態
        /// <para>
        /// 【理由】都度 Sequence を生成すると GC が増えるため、再利用可能な形で保持する。
        /// </para>
        /// </summary>
        protected Sequence Sequence;
        
        protected SequenceAnimationBase()
        {
            Sequence = DOTween.Sequence()
                .SetAutoKill(false) // 完了しても破棄されず再利用可能
                .Pause();           // 初期状態は停止
        }
        
        /// <summary>
        /// アニメーションを再生（async 対応）。
        /// - Sequence.Restart() により最初から再生
        /// - UniTask.WaitUntil で再生完了まで待機
        /// - キャンセルが来たら途中で停止可能
        /// </summary>
        public virtual async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            Sequence.Restart();

            // IsActive=false または IsPlaying=false になったら完了
            await UniTask.WaitUntil(
                () => !Sequence.IsActive() || !Sequence.IsPlaying(),
                cancellationToken: ct
            );
        }
        
        public abstract void PlayImmediate();
        
        public virtual void Skip()
        {
            Sequence.Complete();
        }
        
        public void Dispose()
        {
            Sequence.Kill();
            Sequence = null;
        }
    }

    /// <summary>
    /// UI アニメーションの共通パラメータ。
    /// - 再生時間
    /// - イージング
    /// などの基本的な設定を保持
    /// </summary>
    public class UIAnimationComponentParamSO: ScriptableObject
    {
        /// <summary>
        /// 【目的】アニメーションの再生時間を秒指定で設定する。
        /// </summary>
        [Header("アニメーションの時間(秒)")]
        [SerializeField, Min(0.01f)]
        [Tooltip("アニメーションにかける時間。最小 0.01 秒。")]
        public float DurationSec = 0.5f;

        /// <summary>
        /// 【目的】補間のカーブ形状を指定し、演出の印象を左右する Ease 設定を制御する。
        /// </summary>
        [Header("イージング種類")]
        [SerializeField]
        [Tooltip("DOTween の Ease 種類。Linear/OutQuad などから選択。")]
        public Ease Ease = Ease.Linear;
    }
}
