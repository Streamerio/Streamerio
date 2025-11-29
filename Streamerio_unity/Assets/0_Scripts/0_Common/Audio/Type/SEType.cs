// モジュール概要:
// 効果音(SE) の種類を列挙し、ScriptableObject やプレイヤーで辞書キーとして利用する。
// 使用例: SEScriptableObject が AudioClip を割り当て、SEPlayer が再生対象を特定する。

namespace Common.Audio
{
    
    /// <summary>
    /// 【目的】効果音の識別子を定義する。
    /// 【理由】Enum を通じて SE を型安全に指定し、辞書やファサードで扱いやすくするため。
    /// </summary>
    public enum SEType
    {
		None = 0,
		Monster012 = 43,
		どん_効果音 = 41,
		敵のダウン = 42,
		Explosion01 = 19,
		Explosion02 = 20,
		Explosion03 = 21,
		Explosion04 = 22,
		Explosion05 = 23,
		Explosion06 = 24,
		Explosion07 = 25,
		Explosion08 = 26,
		PlayerAttack = 30,
		PlayerDamage = 31,
		PlayerGround = 32,
		PlayerJump = 33,
		NESRPG0112 = 29,
		SNESRPG01 = 28,
		ThunderBullet = 38,
		TuinURT = 35,
		UltThunder = 36,
		電撃魔法2 = 39,
		魔法1 = 37,

    }
}
