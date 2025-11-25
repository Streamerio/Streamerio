package handler

import (
	"context"
	"crypto/rand"
	"encoding/json"
	"fmt"
	"io"
	"log/slog"
	"net/http"
	"sync"
	"time"

	"golang.org/x/net/websocket"

	"streamerrio-backend/internal/service"
	"streamerrio-backend/pkg/pubsub"

	"github.com/labstack/echo/v4"
	"github.com/oklog/ulid/v2"
)

// WebSocketMessageType: WebSocket通信で使用するメッセージタイプ
// Unity ↔ Backend 間の通信で使用されるメッセージタイプを定義
type WebSocketMessageType string

const (
	// UnityからBackendへ送信されるメッセージタイプ

	// MessageTypeGameStart: ゲーム開始通知
	// Unityから送信され、ゲームセッションの開始を通知
	MessageTypeGameStart WebSocketMessageType = "game_start"

	// MessageTypeGameEnd: ゲーム終了通知
	// Unityから送信され、ゲームセッションの終了を通知
	MessageTypeGameEnd WebSocketMessageType = "game_end"

	// BackendからUnityへ送信されるメッセージタイプ

	// MessageTypeRoomCreated: ルーム作成通知
	// 新規接続時にBackendからUnityへ送信される
	MessageTypeRoomCreated WebSocketMessageType = "room_created"

	// MessageTypeRoomReady: ルーム準備完了通知
	// 再接続時にBackendからUnityへ送信される
	MessageTypeRoomReady WebSocketMessageType = "room_ready"
)

type WebSocketHandler struct {
	connections    map[string]*websocket.Conn
	mu             sync.RWMutex
	roomService    *service.RoomService
	sessionService *service.GameSessionService
	pubsub         pubsub.PubSub
	logger         *slog.Logger
	ulidEntropy    io.Reader
}

func NewWebSocketHandler(ps pubsub.PubSub, logger *slog.Logger) *WebSocketHandler {
	if logger == nil {
		logger = slog.Default()
	}
	return &WebSocketHandler{
		connections: make(map[string]*websocket.Conn),
		pubsub:      ps,
		logger:      logger,
		ulidEntropy: ulid.Monotonic(rand.Reader, 0),
	}
}

// Unity接続管理
// /ws-unity に接続されたら、この関数が呼ばれる
func (h *WebSocketHandler) HandleUnityConnection(c echo.Context) error {
	s := websocket.Server{
		Handshake: func(cfg *websocket.Config, r *http.Request) error {
			// 全オリジン許可（必要ならここで厳密にチェック）
			return nil
		},
		Handler: func(ws *websocket.Conn) {
			defer ws.Close()

			// 接続登録（再接続の場合は同一 room_id を維持）
			requestedID := c.QueryParam("room_id")
			var id string
			if requestedID != "" {
				id = h.registerWithID(requestedID, ws, c)
			} else {
				id = h.registerNew(ws, c)
			}
			defer h.unregister(id, ws, c)

			// 接続直後に必ずログを出す
			c.Logger().Infof("Client connected: %s id=%s", c.Request().RemoteAddr, id)

			// 初期メッセージ（再接続時はタイプのみ変える）
			initType := MessageTypeRoomCreated
			if requestedID != "" {
				initType = MessageTypeRoomReady
			}
			payload := map[string]interface{}{
				"type":    string(initType),
				"room_id": id,
			}
			if err := h.SendEventToUnity(id, payload); err != nil {
				c.Logger().Errorf("initial send failed: %v", err)
				return
			}

			for {
				// Client からのメッセージを読み込む
				msg := ""
				err := websocket.Message.Receive(ws, &msg)

				// エラー処理
				// メッセージを受信できなかった場合は、接続を切断する
				if err != nil {
					if err == io.EOF {
						c.Logger().Infof("Client disconnected id=%s", id)
					} else {
						c.Logger().Errorf("receive failed: %v", err)
					}
					return
				}

				var incoming struct {
					Type string `json:"type"`
				}
				if err := json.Unmarshal([]byte(msg), &incoming); err != nil {
					continue
				}

				// 文字列をWebSocketMessageTypeに変換
				messageType := WebSocketMessageType(incoming.Type)

				switch messageType {
				case MessageTypeGameStart:
					if h.sessionService == nil {
						c.Logger().Warn("game_start received but sessionService not set")
						continue
					}
					c.Logger().Infof("game start received id=%s", id)
					c.Logger().Infof("game start を受け取りました\n 具体的な実装はまだです")

				case MessageTypeGameEnd:
					if h.sessionService == nil {
						c.Logger().Warn("game_end received but sessionService not set")
						continue
					}
					if _, err := h.sessionService.EndGame(id); err != nil {
						c.Logger().Errorf("game end handling failed id=%s err=%v", id, err)
					}
				default:
					c.Logger().Warn("unhandled message type", slog.String("type", incoming.Type))
					continue
				}
			}
		},
	}

	s.ServeHTTP(c.Response(), c.Request())
	return nil
}

func (h *WebSocketHandler) RelayActionToUnity(c echo.Context) error {
	// リクエストボディをそのまま JSON として受け取り、Unity へ転送する
	var payload map[string]interface{}
	if err := c.Bind(&payload); err != nil {
		return c.JSON(http.StatusBadRequest, map[string]interface{}{"error": fmt.Sprintf("invalid json: %v", err)})
	}

	// room_idが含まれているか確認
	roomID, ok := payload["room_id"].(string)
	if !ok || roomID == "" {
		return c.JSON(http.StatusBadRequest, map[string]interface{}{"error": "room_id is required"})
	}

	// room_id を除去して転送用のペイロードを作成
	forward := make(map[string]interface{}, len(payload))
	for k, v := range payload {
		if k == "room_id" {
			continue
		}
		forward[k] = v
	}

	// Unity へ送信
	if err := h.SendEventToUnity(roomID, forward); err != nil {
		return c.JSON(http.StatusNotFound, map[string]interface{}{"error": err.Error()})
	}

	return c.JSON(http.StatusOK, map[string]interface{}{"status": "ok"})
}

// SetRoomService: 後から RoomService を注入
func (h *WebSocketHandler) SetRoomService(rs *service.RoomService) { h.roomService = rs }

// SetGameSessionService: ゲーム終了処理サービスを注入
func (h *WebSocketHandler) SetGameSessionService(gs *service.GameSessionService) {
	h.sessionService = gs
}

// registerNew: 新規接続用に新しい roomID を払い出して登録
func (h *WebSocketHandler) registerNew(ws *websocket.Conn, c echo.Context) string {
	id := ulid.MustNew(ulid.Timestamp(time.Now()), h.ulidEntropy).String()

	if h.roomService != nil {
		if err := h.roomService.CreateIfNotExists(id, "unity"); err != nil {
			c.Logger().Errorf("room db create failed id=%s err=%v", id, err)
		} else {
			c.Logger().Infof("room db created id=%s", id)
		}
	}

	h.mu.Lock()
	h.connections[id] = ws
	h.mu.Unlock()
	return id
}

// registerWithID: 指定 roomID で接続を登録（再接続時）
// 既存接続がある場合は置き換える
func (h *WebSocketHandler) registerWithID(id string, ws *websocket.Conn, c echo.Context) string {
	// 既存の DB レコードは触らない（既に存在している前提）。無い場合のみ作成。
	if h.roomService != nil {
		if err := h.roomService.CreateIfNotExists(id, "unity"); err != nil {
			c.Logger().Errorf("room db ensure failed id=%s err=%v", id, err)
		}
	}

	h.mu.Lock()
	h.connections[id] = ws
	h.mu.Unlock()
	c.Logger().Infof("room re-registered id=%s", id)
	return id
}

// unregister: 接続が同一の場合のみ削除（置換時の誤削除防止）
func (h *WebSocketHandler) unregister(id string, ws *websocket.Conn, c echo.Context) {
	h.mu.Lock()
	defer h.mu.Unlock()

	cur := h.connections[id]
	if cur == ws {
		delete(h.connections, id)
		c.Logger().Infof("Client unregistered id=%s", id)
	} else {
		// すでに別の接続に置き換わっている
		c.Logger().Infof("Skip unregister (replaced) id=%s", id)
	}
}

func (h *WebSocketHandler) SendEventToUnity(roomID string, payload interface{}) error {

	// 排他制御
	h.mu.RLock()
	defer h.mu.RUnlock()

	// 接続先にメッセージを送信
	client := h.connections[roomID]
	if client == nil {
		return fmt.Errorf("no websocket client for roomID=%s", roomID)
	}
	if err := websocket.JSON.Send(client, payload); err != nil {
		return fmt.Errorf("send failed: %v", err)
	}
	return nil
}

func (h *WebSocketHandler) ListClients(c echo.Context) error {
	h.mu.RLock()
	defer h.mu.RUnlock()
	ids := make([]string, 0, len(h.connections))
	for id := range h.connections {
		ids = append(ids, id)
	}
	return c.JSON(http.StatusOK, map[string]interface{}{"clients": ids})
}

// StartPubSubSubscription: Pub/Sub購読を開始（別goroutineで実行）
// REST APIからのイベントをUnityに配信する
func (h *WebSocketHandler) StartPubSubSubscription(ctx context.Context) error {
	handler := func(channel string, message []byte) error {
		var payload map[string]interface{}
		if err := json.Unmarshal(message, &payload); err != nil {
			h.logger.Error("pubsub message unmarshal failed", slog.Any("error", err))
			return err
		}

		// room_idを取得
		roomID, ok := payload["room_id"].(string)
		if !ok || roomID == "" {
			h.logger.Warn("pubsub message missing room_id", slog.Any("payload", payload))
			return fmt.Errorf("room_id not found in payload")
		}

		// 自分が接続を持っている場合のみ配信
		if err := h.SendEventToUnity(roomID, payload); err != nil {
			// 接続がないのは正常（他のインスタンスが持っている）
			h.logger.Debug("no local connection for room, skip delivery",
				slog.String("room_id", roomID),
				slog.String("event_type", fmt.Sprintf("%v", payload["event_type"])))
			return nil
		}

		h.logger.Info("event delivered to unity via pubsub",
			slog.String("room_id", roomID),
			slog.String("event_type", fmt.Sprintf("%v", payload["event_type"])))
		return nil
	}

	// 購読開始（ブロッキング）
	h.logger.Info("starting pubsub subscription", slog.String("channel", pubsub.ChannelGameEvents))
	if err := h.pubsub.Subscribe(ctx, pubsub.ChannelGameEvents, handler); err != nil {
		h.logger.Error("pubsub subscription failed", slog.Any("error", err))
		return err
	}
	return nil
}
