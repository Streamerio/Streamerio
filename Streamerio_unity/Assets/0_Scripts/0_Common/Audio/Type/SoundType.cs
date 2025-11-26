// モジュール概要:
// 音量・ミュートやプレイヤー区分などで利用するサウンド種別の列挙値を定義する。
// 使用例: VolumeMediator や AudioSourcePoolFactory が SoundType をキーとして辞書を管理する。

namespace Common.Audio
{
    /// <summary>
    /// 【目的】音関連処理で利用するサウンド種別を定義する。
    /// 【理由】マスターボリュームや BGM/SE を識別し、辞書キーとして活用するため。
    /// </summary>
    public enum SoundType
    {
		Master = 1,
		BGM = 2,
		SE = 3,
    }
}
