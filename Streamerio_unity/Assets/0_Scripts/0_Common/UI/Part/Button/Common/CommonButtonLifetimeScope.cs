// モジュール概要:
// 共通ボタンの LifetimeScope を構築し、ボタン View と Presenter の依存を VContainer に登録する。
// 依存関係: UnityEngine.UI.Button/ObservableEventTrigger による入力、ClickEventBinder 経由の SE 再生、IAudioFacade を使用。
// 使用例: ボタンプレハブに本スコープを付与し、Dialog やメニュー画面の子スコープとして生成する。

using Alchemy.Inspector;
using Common.UI.Animation;
using UnityEngine;
using VContainer;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// 【目的】共通ボタンに必要な依存を登録し、Presenter と View を配線する。
    /// 【理由】プレハブ化したボタンを生成した際に、DI コンテナが即座に入力処理とアニメーションを利用できるようにするため。
    /// </summary>
    public class CommonButtonLifetimeScope: ButtonLifetimeScopeBase
    {
        [SerializeField, ReadOnly]
        private RectTransform _rectTransform;
        [SerializeField, ReadOnly]
        private CanvasGroup _canvasGroup;
        
        [SerializeField]
        private ScaleAnimationParamSO _pushDownAnimParam;
        [SerializeField]
        private ScaleAnimationParamSO _pushUpAnimParam;
        [SerializeField]
        private FadeAnimationParamSO _enterAnimParam;
        [SerializeField]
        private FadeAnimationParamSO _exitAnimParam;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            UIAnimationBinder.Bind(builder, new ScaleAnimation(_rectTransform, _pushDownAnimParam), AnimationType.PushDown);
            UIAnimationBinder.Bind(builder, new ScaleAnimation(_rectTransform, _pushUpAnimParam), AnimationType.PushUp);
            UIAnimationBinder.Bind(builder, new FadeAnimation(_canvasGroup, _enterAnimParam), AnimationType.Enter);
            UIAnimationBinder.Bind(builder, new FadeAnimation(_canvasGroup, _exitAnimParam), AnimationType.Exit);
            
            base.Configure(builder);
        }
    }
}
