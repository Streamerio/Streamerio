// ============================================================================
// モジュール概要: ローディング画面の View 実装として、Iris 系アニメーションの初期化と演出再生を担う。
// 外部依存: Cysharp.Threading.Tasks、DG.Tweening、UnityEngine.UI、VContainer。
// 使用例: LoadingScreenPresenter が ILoadingScreenView を介して ShowAsync/HideAsync を呼び出し、画面遷移演出を統一する。
// ============================================================================

using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Loading
{
    /// <summary>
    /// ローディング画面の View。
    /// - Image に専用マテリアルを割り当て、シェーダプロパティを書き換えてイリス演出を実現
    /// - IrisIn/IrisOut コンポーネントを用いて表示/非表示/シーン遷移をアニメーション制御
    /// <para>
    /// 【理由】Presenter 側のロジックを軽量に保ち、演出関連の責務を View へ切り出すため。
    /// </para>
    /// </summary>
    public class LoadingScreenView : DisplayViewBase, ILoadingScreenView
    {
        /// <summary>
        /// 【目的】シェーダーマテリアルを適用するターゲット Image を保持する。
        /// </summary>
        [SerializeField, ReadOnly]
        [Tooltip("イリス演出用マテリアルを適用する Image コンポーネント。")]
        private Image _image;

        /// <summary>
        /// 実行時に複製して利用するマテリアル。
        /// （共有マテリアルを直接操作すると他オブジェクトへ影響するため注意）
        /// </summary>
        private Material _irisAnimationMaterial;

        private IIrisAnimation _showAnimation;
        private IIrisAnimation _hideAnimation;
        
        private IUIAnimation _panelShowAnimation;
        private IUIAnimation _panelHideAnimation;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _image ??= GetComponent<Image>();
        }
#endif

        [Inject]
        public void Construct(
            Material irisOutMaterial,
            [Key(AnimationType.Show)] IIrisAnimation showAnimation,
            [Key(AnimationType.Hide)] IIrisAnimation hideAnimation,
            [Key(AnimationType.Show)] IUIAnimation panelShowAnimation,
            [Key(AnimationType.Hide)] IUIAnimation panelHideAnimation)
        {
            _image.material = irisOutMaterial;

            _showAnimation = showAnimation;
            _hideAnimation = hideAnimation;
            
            _panelShowAnimation = panelShowAnimation;
            _panelHideAnimation = panelHideAnimation;
        }

        /// <summary>
        /// アニメーションで表示（外→内へ収束）。
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimation.PlayAsync(ct);
            await _panelShowAnimation.PlayAsync(ct);
        }

        /// <summary>
        /// 任意のワールド座標を中心にしてアニメーション表示。
        /// - WorldToViewportPoint で座標を UV に変換して中心に設定
        /// </summary>
        public async UniTask ShowAsync(Vector3 centerCirclePosition, CancellationToken ct)
        {
            await _showAnimation.PlayAsync(WorldToViewportPoint(centerCirclePosition), ct);
            await _panelShowAnimation.PlayAsync(ct);
        }

        /// <summary>
        /// 即時表示。
        /// - 半径を最小値に設定（閉じた状態）
        /// </summary>
        public override void Show()
        {
            _showAnimation.PlayImmediate();
            _panelShowAnimation.PlayImmediate();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _panelHideAnimation.PlayAsync(ct);
            await _hideAnimation.PlayAsync(ct);
        }
        
        public async UniTask HideAsync(Vector3 centerCirclePosition, CancellationToken ct)
        {
            await _panelHideAnimation.PlayAsync(ct);
            await _hideAnimation.PlayAsync(WorldToViewportPoint(centerCirclePosition), ct);
        }

        public override void Hide()
        {
            _panelHideAnimation.PlayImmediate();
            _hideAnimation.PlayImmediate();
        }
        
        private Vector2 WorldToViewportPoint(Vector3 worldPosition)
        {
            return Camera.main.WorldToViewportPoint(worldPosition);
        }
    }
    
    /// <summary>
    /// ローディング画面の View 契約。
    /// <para>
    /// 【理由】Presenter から View 操作を抽象化し、テストや差し替えを容易にするため。
    /// </para>
    /// </summary>
    public interface ILoadingScreenView: IDisplayView
    {
        /// <summary>
        /// 【目的】任意中心座標を指定して表示する。
        /// </summary>
        UniTask ShowAsync(Vector3 centerCirclePosition, CancellationToken ct);
        
        UniTask HideAsync(Vector3 centerCirclePosition, CancellationToken ct);
    }
}
