package handler

import (
	"log/slog"
	"net/http"
	"time"

	"streamerrio-backend/internal/model"
	"streamerrio-backend/internal/service"

	"github.com/labstack/echo/v4"
)

// APIHandler: REST エンドポイント集約 (ルーム取得 / イベント送信 / 統計取得)
type APIHandler struct {
	roomService     *service.RoomService
	eventService    *service.EventService
	sessionService  *service.GameSessionService
	viewerService   *service.ViewerService
	logTokenService *service.LogTokenService
	logger          *slog.Logger
}

// NewAPIHandler: 依存するサービスを束ねて構築
func NewAPIHandler(
	roomService *service.RoomService,
	eventService *service.EventService,
	sessionService *service.GameSessionService,
	viewerService *service.ViewerService,
	logTokenService *service.LogTokenService,
) *APIHandler {
	return &APIHandler{
		roomService:     roomService,
		eventService:    eventService,
		sessionService:  sessionService,
		viewerService:   viewerService,
		logTokenService: logTokenService,
		logger:          slog.Default(),
	}
}

func (h *APIHandler) WithLogger(logger *slog.Logger) *APIHandler {
	if logger == nil {
		return h
	}
	h.logger = logger
	return h
}

// GetOrCreateViewerID: 視聴者端末識別用の ID を払い出す
func (h *APIHandler) GetOrCreateViewerID(c echo.Context) error {
	var existing string
	if cookie, err := c.Cookie("viewer_id"); err == nil {
		existing = cookie.Value
	}
	viewerID, err := h.viewerService.EnsureViewerID(existing)
	if err != nil {
		h.logger.Error("ensure_viewer_id_failed", slog.Any("error", err))
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}
	viewer, err := h.viewerService.GetViewer(viewerID)
	if err != nil {
		h.logger.Error("get_viewer_failed", slog.String("viewer_id", viewerID), slog.Any("error", err))
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}
	// クロスサイト（フロント: vercel.app, バックエンド: Cloud Run）で
	// Cookie を送受信できるように SameSite=None; Secure を指定する。
	// NOTE: Secure=true は HTTPS 前提。本番環境（Cloud Run/Vercel）は HTTPS なので問題なし。
	cookie := &http.Cookie{
		Name:     "viewer_id",
		Value:    viewerID,
		Path:     "/",
		MaxAge:   365 * 24 * 60 * 60,
		Secure:   true,
		HttpOnly: false,
		SameSite: http.SameSiteNoneMode,
	}
	c.SetCookie(cookie)
	var name interface{}
	if viewer != nil && viewer.Name != nil {
		name = *viewer.Name
	}
	return c.JSON(http.StatusOK, map[string]interface{}{
		"viewer_id": viewerID,
		"name":      name,
	})
}

// SetViewerName: 視聴者名を登録/更新する
func (h *APIHandler) SetViewerName(c echo.Context) error {
	var req struct {
		ViewerID string `json:"viewer_id"`
		Name     string `json:"name"`
	}
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "invalid body"})
	}
	viewer, err := h.viewerService.SetViewerName(req.ViewerID, req.Name)
	if err != nil {
		h.logger.Error("set_viewer_name_failed", slog.String("viewer_id", req.ViewerID), slog.Any("error", err))
		return c.JSON(http.StatusBadRequest, map[string]string{"error": err.Error()})
	}
	var name interface{}
	if viewer != nil && viewer.Name != nil {
		name = *viewer.Name
	}
	return c.JSON(http.StatusOK, map[string]interface{}{
		"viewer_id": viewer.ID,
		"name":      name,
	})
}

// IssueLogToken: Cloudflare Worker 向けのログトークンを発行
func (h *APIHandler) IssueLogToken(c echo.Context) error {
	var req struct {
		ClientID string   `json:"client_id"`
		ViewerID string   `json:"viewer_id"`
		RoomID   string   `json:"room_id"`
		Platform string   `json:"platform"`
		Scopes   []string `json:"scopes"`
	}
	if err := c.Bind(&req); err != nil {
		h.logger.Warn("invalid_log_token_body", slog.Any("error", err))
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "invalid body"})
	}

	clientID := req.ClientID
	if clientID == "" {
		clientID = req.ViewerID
	}
	if clientID == "" {
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "client_id or viewer_id is required"})
	}
	if req.RoomID == "" {
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "room_id is required"})
	}
	if _, err := h.roomService.GetRoom(req.RoomID); err != nil {
		h.logger.Warn("log_token_room_not_found", slog.String("room_id", req.RoomID), slog.Any("error", err))
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}

	result, err := h.logTokenService.IssueToken(service.LogTokenIssueInput{
		ClientID: clientID,
		ViewerID: req.ViewerID,
		RoomID:   req.RoomID,
		Platform: req.Platform,
		Scopes:   req.Scopes,
	})
	if err != nil {
		h.logger.Error("issue_log_token_failed", slog.String("room_id", req.RoomID), slog.Any("error", err))
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}

	return c.JSON(http.StatusOK, map[string]interface{}{
		"token":       result.Token,
		"issued_at":   result.IssuedAt,
		"expires_at":  result.ExpiresAt,
		"scopes":      result.Scopes,
		"client_id":   clientID,
		"viewer_id":   req.ViewerID,
		"room_id":     req.RoomID,
		"platform":    req.Platform,
		"ttl_seconds": int(result.ExpiresAt.Sub(result.IssuedAt).Seconds()),
	})
}

// JoinRoom: 視聴者がルームに参加したことを通知 (QRスキャン時など)
func (h *APIHandler) JoinRoom(c echo.Context) error {
	roomID := c.Param("id")
	var req struct {
		ViewerID string `json:"viewer_id"`
	}
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "invalid body"})
	}
	if req.ViewerID == "" {
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "viewer_id is required"})
	}

	if _, err := h.roomService.GetRoom(roomID); err != nil {
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}

	if err := h.eventService.JoinRoom(roomID, req.ViewerID); err != nil {
		h.logger.Error("join_room_failed", slog.String("room_id", roomID), slog.String("viewer_id", req.ViewerID), slog.Any("error", err))
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}

	return c.JSON(http.StatusOK, map[string]string{"status": "joined"})
}

// GetRoom: ルーム情報取得 (存在しない場合 404)
func (h *APIHandler) GetRoom(c echo.Context) error {
	id := c.Param("id")
	room, err := h.roomService.GetRoom(id)
	if err != nil {
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}
	return c.JSON(http.StatusOK, room)
}

// SendEvent: イベントを受信し閾値チェックまで実施
func (h *APIHandler) SendEvent(c echo.Context) error {
	roomID := c.Param("id")
	room, err := h.roomService.GetRoom(roomID)
	if err != nil {
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}

	type PushEvent struct {
		ButtonName string `json:"button_name"`
		PushCount  int64  `json:"push_count"`
	}

	var req struct {
		EventType  string      `json:"event_type"`
		ViewerID   string      `json:"viewer_id"`
		ViewerName string      `json:"viewer_name"`
		PushEvents []PushEvent `json:"push_events"`
	}

	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "invalid body"})
	}

	var viewerID *string
	var viewerName *string
	if req.ViewerID != "" {
		viewerID = &req.ViewerID
	}
	if req.ViewerName != "" {
		viewerName = &req.ViewerName
	}

	// ゲーム終了済みの場合はサマリーを返す（既存の処理）
	if room.Status == "ended" {
		if viewerID == nil {
			return c.JSON(http.StatusBadRequest, map[string]string{"error": "viewer_id is required after game end"})
		}
		summary, err := h.sessionService.GetViewerSummary(roomID, *viewerID)
		if err != nil {
			h.logger.Error("viewer_summary_failed", slog.String("room_id", roomID), slog.String("viewer_id", *viewerID), slog.Any("error", err))
			return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
		}
		return c.JSON(http.StatusOK, map[string]interface{}{
			"game_over":      true,
			"viewer_summary": summary,
		})
	}

	// ゲームが開始されていない場合はイベントを処理しない
	if room.Status != "in_game" {
		h.logger.Info("game not started yet, rejecting event", slog.String("room_id", roomID), slog.String("status", room.Status))
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "game not started"})
	}

	// PushCount合計
	totalPushCount := int64(0)
	// PushEventMap: ボタン名とPushCountのマップ
	PushEventMap := map[model.EventType]int64{
		model.SKILL1: 0,
		model.SKILL2: 0,
		model.SKILL3: 0,
		model.ENEMY1: 0,
		model.ENEMY2: 0,
		model.ENEMY3: 0,
	}

	// PushEventsのバリデーション
	// PushEventMapへの登録もここで行う
	for _, event := range req.PushEvents {
		eventType := model.EventType(event.ButtonName)
		// EventTypeが未定義の場合はエラー
		if _, ok := PushEventMap[eventType]; !ok {
			return c.JSON(http.StatusBadRequest, map[string]string{"error": "invalid event type"})
		}
		pushCount := event.PushCount
		if pushCount <= 0 {
			return c.JSON(http.StatusBadRequest, map[string]string{"error": "push count must be greater than 0"})
		}
		// 連打攻撃防止
		// 1つのボタンごとに20回までしか押せないようにする
		if pushCount > 20 {
			pushCount = 20
		}
		totalPushCount += pushCount
		PushEventMap[eventType] = pushCount
	}

	responses, err := h.eventService.ProcessEvent(roomID, PushEventMap, viewerID, viewerName)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}

	// 最新の統計情報を取得
	stats, err := h.eventService.GetRoomStats(roomID)
	if err != nil {
		// 統計取得失敗はログに出すが、イベント送信自体は成功しているので続行するか、エラーにするか
		// ここではフロントエンドが stats 依存になったため、不整合を防ぐためエラーログを出して stats は空にするか、500にする
		h.logger.Error("failed to get room stats after event", slog.String("room_id", roomID), slog.Any("error", err))
		// 必要であれば return c.JSON(...) でエラーを返してください
	}

	// 現在の視聴者数を取得（イベント結果のいずれかから取得）
	currentViewerCount := 0
	if len(responses) > 0 {
		currentViewerCount = responses[0].ViewerCount
	} else if len(stats) > 0 {
		currentViewerCount = stats[0].ViewerCount
	}

	// 配列として結果を返す
	return c.JSON(http.StatusOK, map[string]interface{}{
		"event_results": responses,
		"viewer_count":  currentViewerCount, // フロントエンド向けに視聴者数を追加
		"stats":         stats,
	})
}

// GetRoomStats: 現在のイベント種別ごとのカウントと閾値を返す
func (h *APIHandler) GetRoomStats(c echo.Context) error {
	roomID := c.Param("id")
	if _, err := h.roomService.GetRoom(roomID); err != nil {
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}
	stats, err := h.eventService.GetRoomStats(roomID)
	if err != nil {
		h.logger.Error("get_room_stats_failed", slog.String("room_id", roomID), slog.Any("error", err))
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}
	return c.JSON(http.StatusOK, map[string]interface{}{
		"room_id": roomID,
		"stats":   stats,
		"time":    time.Now(),
	})
}

// GetRoomResult: 終了後の集計結果を取得
func (h *APIHandler) GetRoomResult(c echo.Context) error {
	roomID := c.Param("id")
	room, err := h.roomService.GetRoom(roomID)
	if err != nil || room == nil {
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}
	if room.Status != "ended" {
		return c.JSON(http.StatusConflict, map[string]string{"error": "room not ended"})
	}
	summary, err := h.sessionService.GetRoomResult(roomID)
	if err != nil {
		h.logger.Error("get_room_result_failed", slog.String("room_id", roomID), slog.Any("error", err))
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}
	var viewerSummary *model.ViewerSummary
	if viewerID := c.QueryParam("viewer_id"); viewerID != "" {
		if vs, err := h.sessionService.GetViewerSummary(roomID, viewerID); err == nil {
			viewerSummary = vs
		}
	}
	return c.JSON(http.StatusOK, map[string]interface{}{
		"game_over":      true,
		"room_id":        summary.RoomID,
		"ended_at":       summary.EndedAt,
		"top_by_event":   summary.TopByEvent,
		"top_overall":    summary.TopOverall,
		"event_totals":   summary.EventTotals,
		"viewer_totals":  summary.ViewerTotals,
		"viewer_summary": viewerSummary,
	})
}
