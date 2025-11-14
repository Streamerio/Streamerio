package counter

import (
	"sync"
	"time"
)

// memoryCounter: プロトタイプ/テスト用のインメモリ実装 (再起動で消える)
type memoryCounter struct {
	mu      sync.RWMutex
	counts  map[string]map[string]int64 // roomID -> eventType -> count
	viewers map[string]map[string]int64 // roomID -> viewerID -> lastUnix(秒)
	window  time.Duration               // アクティブ判定窓
}

// NewMemoryCounter: インメモリ実装生成 (5分窓)
func NewMemoryCounter() Counter {
	return &memoryCounter{
		counts:  make(map[string]map[string]int64),
		viewers: make(map[string]map[string]int64),
		window:  5 * time.Minute,
	}
}

// Increment: カウントをvalueだけ加算して現在値返却
func (m *memoryCounter) Increment(roomID, eventType string, value int64) (int64, error) {
	m.mu.Lock()
	defer m.mu.Unlock()
	if _, ok := m.counts[roomID]; !ok {
		m.counts[roomID] = make(map[string]int64)
	}
	m.counts[roomID][eventType] += value
	return m.counts[roomID][eventType], nil
}

// Get: 現在カウント取得
func (m *memoryCounter) Get(roomID, eventType string) (int64, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()
	if evMap, ok := m.counts[roomID]; ok {
		return evMap[eventType], nil
	}
	return 0, nil
}

// Reset: 指定イベント種別カウントを0クリア
func (m *memoryCounter) Reset(roomID, eventType string) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	if evMap, ok := m.counts[roomID]; ok {
		evMap[eventType] = 0
	}
	return nil
}

// SetExcess: 閾値超過分をカウントに設定（超過分を捨てない）
func (m *memoryCounter) SetExcess(roomID, eventType string, excess int64) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	if _, ok := m.counts[roomID]; !ok {
		m.counts[roomID] = make(map[string]int64)
	}
	m.counts[roomID][eventType] = excess
	return nil
}

// UpdateViewerActivity: 視聴者最終アクセス時刻を更新
func (m *memoryCounter) UpdateViewerActivity(roomID, viewerID string) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	if _, ok := m.viewers[roomID]; !ok {
		m.viewers[roomID] = make(map[string]int64)
	}
	m.viewers[roomID][viewerID] = time.Now().Unix()
	return nil
}

// GetActiveViewerCount: 窓内(5分) の視聴者数を数え古いものは削除
func (m *memoryCounter) GetActiveViewerCount(roomID string) (int64, error) {
	cutoff := time.Now().Add(-m.window).Unix()
	m.mu.Lock()
	defer m.mu.Unlock()
	vMap, ok := m.viewers[roomID]
	if !ok {
		return 0, nil
	}
	var c int64
	for id, ts := range vMap {
		if ts >= cutoff {
			c++
		} else {
			delete(vMap, id)
		}
	}
	return c, nil
}
