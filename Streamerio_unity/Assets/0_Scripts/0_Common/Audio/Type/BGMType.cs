// モジュール概要:
// BGM 再生に使用する曲種別の列挙値を定義する。ScriptableObject やプレイヤーで辞書キーとして利用。
// 使用例: BGMScriptableObject が AudioClip を紐付け、BGMPlayer が再生対象を判別する。

namespace Common.Audio
{
    
    /// <summary>
    /// 【目的】アプリ内で扱う BGM の識別子を定義する。
    /// 【理由】ScriptableObject や AudioFacade で BGM を指定する際に、型安全に扱えるようにするため。
    /// </summary>
    public enum BGMType
    {
		None = 0,
		kuraituuro = 2,
		singetunoyami = 3,

    }
}
