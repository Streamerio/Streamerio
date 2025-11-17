package counter

// Counter: イベント回数 & 視聴者アクティビティを抽象化するインタフェース
// すべてのメソッドは並行安全であること (goroutine から同時呼び出し想定)
type Counter interface {
    Increment(roomID, eventType string) (int64, error)        // カウント+1し現在値返す
    Get(roomID, eventType string) (int64, error)              // 現在カウント取得
    Reset(roomID, eventType string) error                     // カウントリセット(閾値到達後など)
    UpdateViewerActivity(roomID, viewerID string) error       // 視聴者アクティビティ更新(最終時刻記録)
    GetActiveViewerCount(roomID string) (int64, error)        // 一定期間内のアクティブ視聴者数
}
