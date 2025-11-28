using System.Collections.Generic;
using Alchemy.Inspector;
using Common;
using Common.Audio;
using Common.UI.Display.Window.Book.Page;
using UnityEngine;
using UnityEngine.UI;

namespace OutGame.Book.Option
{
    /// <summary>
    /// オプションパネルの View（見た目担当）。
    /// - 音量設定用のスライダーを保持
    /// - VolumeType ごとに Slider を辞書管理し、Presenter 側からアクセス可能にする
    /// - 実際のスライダー操作の処理（値の反映や保存）は Presenter 側で行う
    /// </summary>
    public class SoundPagePanelView : CommonPagePanelView, ISoundPagePanelView
    {
        [SerializeField, LabelText("サウンドスライダー")]
        private SerializeDictionary<SoundType, Slider> _soundSliderDict;

        /// <summary>
        /// 音量スライダーを VolumeType ごとに公開（読み取り専用）
        /// </summary>
        public IReadOnlyDictionary<SoundType, Slider> SoundSliderDict => _soundSliderDict.ToDictionary();
    }

    public interface ISoundPagePanelView : IPagePanelView
    {
        IReadOnlyDictionary<SoundType, Slider> SoundSliderDict { get; }
    }
}