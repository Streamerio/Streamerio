using System.Threading;
using Common.UI.Animation;
using Common.UI.Display.Window;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using VContainer;

namespace OutGame.Result.UI
{
    /// <summary>
    /// ゲームクリア時のオーバーレイ View。
    /// - プレイヤーにクリックを促すテキストを表示
    /// - Presenter からアニメーションの開始/停止を制御される
    /// </summary>
    public class ResultWindowView : WindowViewBase, IResultWindowView
    {
        [SerializeField]
        private TMP_Text _allText;
        [SerializeField]
        private TMP_Text _enemyText;
        [SerializeField]
        private TMP_Text _skillText;
        
        private IUIAnimation _clickTextAnimation;
        private IWebSocketManager _webSocketManager;
        [Inject]
        public void Construct([Key(AnimationType.FlashText)] IUIAnimation clickTextAnimation, IWebSocketManager webSocketManager)
        {
            _clickTextAnimation = clickTextAnimation;
            _webSocketManager = webSocketManager;
        }

        /// <summary>
        /// アニメーション付き表示。
        /// - クリック誘導テキストのアニメーションを開始
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        { 
            if(_webSocketManager.GameEndSummary != null)
            {
                WebSocketManager.GameEndSummaryNotification summary = _webSocketManager.GameEndSummary;
                _allText.text = (summary.SummaryDetails[WebSocketManager.GameEndSummaryNotification.AllKey] ==null || summary.SummaryDetails[WebSocketManager.GameEndSummaryNotification.AllKey].viewer_name == null) ? "名無しの視聴者" : summary.SummaryDetails[WebSocketManager.GameEndSummaryNotification.AllKey].viewer_name;
                _enemyText.text = (summary.SummaryDetails[WebSocketManager.GameEndSummaryNotification.EnemyKey] ==null || summary.SummaryDetails[WebSocketManager.GameEndSummaryNotification.EnemyKey].viewer_name == null) ? "名無しの視聴者" : summary.SummaryDetails[WebSocketManager.GameEndSummaryNotification.EnemyKey].viewer_name;
                _skillText.text = (summary.SummaryDetails[WebSocketManager.GameEndSummaryNotification.SkillKey] ==null || summary.SummaryDetails[WebSocketManager.GameEndSummaryNotification.SkillKey].viewer_name == null) ? "名無しの視聴者" : summary.SummaryDetails[WebSocketManager.GameEndSummaryNotification.SkillKey].viewer_name;
            }
            
            await base.ShowAsync(ct);
            
            
            _clickTextAnimation.PlayAsync(destroyCancellationToken).Forget();
        }
        
        /// <summary>
        /// 即時表示。
        /// - クリック誘導テキストのアニメーションを開始
        /// </summary>
        public override void Show()
        {
            base.Show();
            _clickTextAnimation.PlayAsync(destroyCancellationToken).Forget();
        }
        
        /// <summary>
        /// アニメーション付き非表示。
        /// - クリック誘導テキストのアニメーションを停止
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            _clickTextAnimation.Skip();
            await base.HideAsync(ct);
        }
        
        /// <summary>
        /// 即時非表示。
        /// - クリック誘導テキストのアニメーションを停止
        /// </summary>
        public override void Hide()
        {
            _clickTextAnimation.Skip();
            base.Hide();
        }
        
        public void SkipShowAnimation()
        {
            ShowAnim.Skip();
            ShowPartsAnim.Skip();
        }
    }
    
    public interface IResultWindowView : IWindowView
    {
        void SkipShowAnimation();
    }
}