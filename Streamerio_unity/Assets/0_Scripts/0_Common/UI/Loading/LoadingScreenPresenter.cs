// ============================================================================
// モジュール概要: ローディング画面の Presenter 層を定義し、外部 API と View の橋渡しを担う。
// 外部依存: Cysharp.Threading.Tasks（UniTask）、UnityEngine（Vector3）、IAttachable（依存注入契約）。
// 使用例: シーン遷移時に ILoadingScreen.OpenAsync 系 API を呼び出し、統一されたローディング演出を再生する。
// ============================================================================

using System.Threading;
using Common.UI.Display;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Loading
{
    /// <summary>
    /// ローディング画面の公開 API 契約。
    /// <para>
    /// 【理由】Presenter への依存を抽象化し、DI 経由でモジュール境界を明確にする。
    /// </para>
    /// </summary>
    public interface ILoadingScreen : IDisplay, IAttachable<LoadingScreenContext>
    {
        /// <summary>
        /// 【目的】任意座標を中心としたイリス演出でローディング画面を表示する。
        /// </summary>
        UniTask ShowAsync(Vector3 centerCirclePosition, CancellationToken ct);
        
        UniTask HideAsync(Vector3 centerCirclePosition, CancellationToken ct);
    }
    
    /// <summary>
    /// ローディング画面のプレゼンター（シングルトン）。
    /// - 外部コードからの呼び出し窓口
    /// - View の表示/非表示/遷移アニメーションを操作
    /// - 表示中は UI の操作を無効化し、演出が終わると再度制御を戻す
    /// </summary>
    public class LoadingScreenPresenter: DisplayPresenterBase<ILoadingScreenView, LoadingScreenContext>, ILoadingScreen
    {
        private Animator _playerAnimator;
        
        protected override void AttachContext(LoadingScreenContext context)
        {
            View = context.View;
            _playerAnimator = context.PlayerAnimator;
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            _playerAnimator.enabled = true;
            await base.ShowAsync(ct);
        }

        public async UniTask ShowAsync(Vector3 centerCirclePosition, CancellationToken ct)
        {
            _isShow = true;
            _playerAnimator.enabled = true;
            await View.ShowAsync(centerCirclePosition, ct);
            View.SetInteractable(true);
        }
        
        public override void Show()
        {
            _playerAnimator.enabled = true;
            base.Show();
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await base.HideAsync(ct);
            _playerAnimator.enabled = false;
        }
        
        public async UniTask HideAsync(Vector3 centerCirclePosition, CancellationToken ct)
        {
            await View.HideAsync(centerCirclePosition, ct);
            View.SetInteractable(false);
            _playerAnimator.enabled = false;
            _isShow = false;
        }
        
        public override void Hide()
        {
            base.Hide();
            _playerAnimator.enabled = false;
        }
    }
    
    /// <summary>
    /// ローディング画面 Presenter へ渡すコンテキスト。
    /// <para>
    /// 【理由】DI から渡す依存が増えた際も型安全に拡張できるよう、構造体でまとめておく。
    /// </para>
    /// </summary>
    public class LoadingScreenContext
    {
        /// <summary>
        /// 【目的】Presenter が操作対象とする View 参照を保持する。
        /// </summary>
        public ILoadingScreenView View;
        
        public Animator PlayerAnimator;
    }
}
