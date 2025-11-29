using Common;
using Common.Audio;
using Common.UI.Display.Window.Book.Page;
using Volume = Common.Audio.Volume;

namespace OutGame.Book.Option
{
    public interface ISoundPagePanel: IPagePanel, IAttachable<SoundPagePanelContext>{}
    
    /// <summary>
    /// オプションパネルの Presenter。
    /// - サウンド系の設定スライダーを AudioManager とバインド
    /// - スライダーの値をリアルタイムに AudioManager に反映
    /// - パネルを閉じる際にボリューム設定を保存
    /// </summary>
    public class SoundPagePanelPresenter : PagePanelPresenterBase<ISoundPagePanelView, SoundPagePanelContext>, ISoundPagePanel
    {
        private IAudioFacade _audioFacade;

        protected override void AttachContext(SoundPagePanelContext context)
        {
            base.AttachContext(context);
            _audioFacade = context.AudioFacade;
        }

        protected override void SetEvent()
        {
            base.SetEvent();
        
            foreach (var slider in View.SoundSliderDict)
            {
                // スライダーの範囲を Volume の定義に合わせる
                slider.Value.minValue = Volume.MIN_VALUE;
                slider.Value.maxValue = Volume.MAX_VALUE;
                // 初期値を現在の AudioManager の設定から反映
                slider.Value.value = _audioFacade.VolumeDict[slider.Key].Value;
                
                // スライダー操作時に音量を変更
                slider.Value.onValueChanged.AddListener(value =>
                {
                    _audioFacade.ChangeVolume(slider.Key, new Volume(value));
                });
            }
        }
    }
    
    public class SoundPagePanelContext : PagePanelContext<ISoundPagePanelView>
    {
        public IAudioFacade AudioFacade;
    }
}