// モジュール概要:
// 音楽用 ScriptableObject のカスタムエディタ基底クラス。Enum ごとの AudioClip 登録を支援し、MusicData を自動生成する。
// 依存関係: AutoSetDataScriptableObjectEditor を継承し、Inspector 上で Enum とアセットの紐付けを提供する。
// 使用例: BGMScriptableObjectEditor / SEScriptableObjectEditor が継承し、BGM/SE 用の編集 UI を実現する。

using Common.Editor;
using System;
using UnityEngine;

namespace Common.Audio.Editor
{
    /// <summary>
    /// 【目的】音楽系 ScriptableObject のエディタ拡張共通処理を提供する。
    /// 【理由】Enum ごとの AudioClip 登録で同じ処理を繰り返さないよう、共通クラスへまとめるため。
    /// </summary>
    /// <typeparam name="TKey">【用途】曲を識別する Enum 型。</typeparam>
    /// <typeparam name="TSO">【用途】対象となる ScriptableObject 派生型。</typeparam>
    public class MusicScriptableObjectEditor<TKey, TSO>: AutoSetDataScriptableObjectEditor<TKey, MusicData, AudioClip, TSO>
        where TKey: Enum
        where TSO: MusicScriptableObjectBase<TKey>
    {
        /// <summary>
        /// 【目的】エディタ初期化時に None 選択肢をセットし、Enum の全値を扱えるようにする。
        /// 【理由】AutoSetDataScriptableObjectEditor の初期設定に加え、音楽データ用の既定値を整えるため。
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            IsSetNone = true;
        }
        
        /// <summary>
        /// 【目的】指定した AudioClip から MusicData を生成する。
        /// 【理由】Clip と容量情報をまとめ、ScriptableObject に保存できる形へ変換するため。
        /// </summary>
        /// <param name="file">【用途】登録対象の AudioClip。</param>
        /// <returns>【戻り値】Clip と DefaultCapacity を含む MusicData。</returns>
        protected override MusicData CreateValue(AudioClip file)
        {
            return new MusicData()
            {
                Clip = file,
                Capacity = Target.DefaultCapacity
            };
        }
    }
}
