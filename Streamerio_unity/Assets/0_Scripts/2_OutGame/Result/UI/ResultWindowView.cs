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
        private string _defaultPlayerName = "名無しの視聴者";
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
                _allText.SetText(GetPlayerName(summary, WebSocketManager.GameEndSummaryNotification.AllKey));
                _enemyText.SetText(GetPlayerName(summary, WebSocketManager.GameEndSummaryNotification.EnemyKey));
                _skillText.SetText(GetPlayerName(summary, WebSocketManager.GameEndSummaryNotification.SkillKey));
            }
            
            await base.ShowAsync(ct);
            
            
            _clickTextAnimation.PlayAsync(destroyCancellationToken).Forget();
        }
        
        private string GetPlayerName(WebSocketManager.GameEndSummaryNotification summary, string key)
        {
            if(summary == null || !summary.SummaryDetails.TryGetValue(key, out var detail) || detail.viewer_name == null)
            {
                return _defaultPlayerName;
            }
            
            return detail.viewer_name;
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