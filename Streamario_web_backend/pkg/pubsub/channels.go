package pubsub

// チャネル名定数
// Pub/Sub で使用するチャネル（トピック）名を定義
// タイポ防止と一元管理のため、チャネル名は必ずこの定数を使用すること

const (
	// ChannelGameEvents: ゲームイベント通知チャネル
	// REST API → WebSocket → Unity への閾値到達イベント配信に使用
	//
	// Payload 例:
	//   {
	//     "type": "game_event",
	//     "room_id": "01HXXX...",
	//     "event_type": "skill1",
	//     "trigger_count": 5,
	//     "viewer_count": 12
	//   }
	ChannelGameEvents = "game_events"

	// ChannelGameEnd: ゲーム終了通知チャネル
	// Unity からのゲーム終了を WebSocket が受信し、必要に応じて他サーバーへ通知
	//
	// Payload 例:
	//   {
	//     "type": "game_end",
	//     "room_id": "01HXXX...",
	//     "ended_at": "2025-10-05T12:00:00Z"
	//   }
	ChannelGameEnd = "game_end_notifications"

	// ChannelCountUpdates: ゲージ更新配信チャネル（SSE用）
	// REST API → SSE Handler → 視聴者フロントエンド へのカウント更新配信に使用
	//
	// Payload 例:
	//   {
	//     "type": "count_update",
	//     "room_id": "01HXXX...",
	//     "event_type": "skill1",
	//     "current_count": 3,
	//     "required_count": 5,
	//     "viewer_count": 8
	//   }
	ChannelCountUpdates = "count_updates"
)
