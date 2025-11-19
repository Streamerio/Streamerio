package counter

// Counter: イベント回数 & 視聴者アクティビティを抽象化するインタフェース
// すべてのメソッドは並行安全であること (goroutine から同時呼び出し想定)
type Counter interface {
    Increment(roomID, eventType string, value int64) (int64, error)        // カウントをvalueだけ増やして現在のカウントを返す
    Get(roomID, eventType string) (int64, error)              // 現在カウント取得
    Reset(roomID, eventType string) error                     // カウントリセット(閾値到達後など)
    SetExcess(roomID, eventType string, excess int64) error   // 閾値超過分をカウントに設定（超過分を捨てない）
    UpdateViewerActivity(roomID, viewerID string) error       // 視聴者アクティビティ更新(最終時刻記録)
    GetActiveViewerCount(roomID string) (int64, error)        // 一定期間内のアクティブ視聴者数
}
