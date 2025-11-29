// ============================================================================
// モジュール概要: UI 共通ユーティリティとしてフェード関連の既定値を一元管理し、演出の統一感を担保する。
// 外部依存: なし（Unity 標準 API のみ）。
// 使用例: CanvasGroup の初期化時に UIUtil.DEFAULT_SHOW_ALPHA を採用してフェード演出の基準値とする。
// ============================================================================

namespace Common.UI
{
    /// <summary>
    /// UI 演出で使い回す定数を集約するユーティリティ。
    /// <para>
    /// 【理由】散在しがちなマジックナンバーをここへ集約し、フェード表示の基準値を統一するため。
    /// </para>
    /// </summary>
    public static class UIUtil
    {
        /// <summary>
        /// 【目的】フェード表示時の既定アルファ値（完全に表示状態）を提供する。
        /// </summary>
        public const float DEFAULT_SHOW_ALPHA = 1f;

        /// <summary>
        /// 【目的】フェード非表示時の既定アルファ値（完全に非表示状態）を提供する。
        /// </summary>
        public const float DEFAULT_HIDE_ALPHA = 0f;
    }
}
