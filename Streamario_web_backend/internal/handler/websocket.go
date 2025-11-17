package handler

import (
	"crypto/rand"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"sync"
	"time"

	"golang.org/x/net/websocket"

	"github.com/labstack/echo/v4"
	"github.com/oklog/ulid/v2"
	"streamerrio-backend/internal/service"
)

type WebSocketHandler struct {
	connections    map[string]*websocket.Conn
	mu             sync.RWMutex
	roomService    *service.RoomService
	sessionService *service.GameSessionService
	ulidEntropy    io.Reader
}

func NewWebSocketHandler() *WebSocketHandler {
	return &WebSocketHandler{
		connections: make(map[string]*websocket.Conn),
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

			// 接続登録
			id := h.register(ws, c)
			defer h.unregister(id, c)

			// 接続直後に必ずログを出す
			c.Logger().Infof("Client connected: %s id=%s", c.Request().RemoteAddr, id)

			payload := map[string]interface{}{
				"type":    "room_created",
				"room_id": id,
				"qr_code": "data:image/png;base64,...",
				"web_url": "https://example.com",
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
				switch incoming.Type {
				case "game_end":
					if h.sessionService == nil {
						c.Logger().Warn("game_end received but sessionService not set")
						continue
					}
					if _, err := h.sessionService.EndGame(id); err != nil {
						c.Logger().Errorf("game end handling failed id=%s err=%v", id, err)
					}
				default:
					// その他のメッセージは現状無視
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

func (h *WebSocketHandler) register(ws *websocket.Conn, c echo.Context) string {
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

func (h *WebSocketHandler) unregister(id string, c echo.Context) {
	h.mu.Lock()
	defer h.mu.Unlock()

	delete(h.connections, id)
	c.Logger().Infof("Client unregistered id=%s", id)
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
