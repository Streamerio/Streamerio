// ============================================================================
// モジュール概要: Overlay 向けの DisplayService 派生クラスを定義し、契約を明示する。
// 外部依存: DisplayService。
// 使用例: Overlay 系 LifetimeScope が IOverlayService を解決し、UI スタック操作を行う。
// ============================================================================

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// オーバーレイ UI の表示制御を提供するサービス契約。
    /// </summary>
    public interface IOverlayService: IDisplayService { }

    /// <summary>
    /// DisplayService を継承したオーバーレイ専用サービス。
    /// </summary>
    public class OverlayService : DisplayService, IOverlayService { }
}
