// モジュール概要:
// ダイアログ表示用の DisplayService 契約と既定実装を定義する。
// 依存関係: Common.UI.Display の IDisplayService / DisplayService 基底クラス。
// 使用例: IDialogService を DI 経由で受け取り、ダイアログ開閉を DisplayService API で制御する。

using Common.UI.Display;

namespace Common.UI.Dialog
{
    /// <summary>
    /// 【目的】ダイアログ表示を管理するサービス契約を定義する。
    /// 【理由】DisplayService の機能を流用しつつ、ダイアログ専用の型で依存を明示したいため。
    /// </summary>
    public interface IDialogService : IDisplayService { }

    /// <summary>
    /// 【目的】DisplayService を継承したダイアログ向け標準実装を提供する。
    /// 【理由】機能追加が必要になるまでは継承のみで十分なため、軽量なラッパーとして定義している。
    /// </summary>
    public class DialogService : DisplayService, IDialogService { }
}
