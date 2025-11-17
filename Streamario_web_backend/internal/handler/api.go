package handler

import (
	"net/http"
	"time"

	"streamerrio-backend/internal/model"
	"streamerrio-backend/internal/service"

	"github.com/labstack/echo/v4"
)

// APIHandler: REST エンドポイント集約 (ルーム取得 / イベント送信 / 統計取得)
type APIHandler struct {
	roomService    *service.RoomService
	eventService   *service.EventService
	sessionService *service.GameSessionService
	viewerService  *service.ViewerService
}

// NewAPIHandler: 依存するサービスを束ねて構築
func NewAPIHandler(roomService *service.RoomService, eventService *service.EventService, sessionService *service.GameSessionService, viewerService *service.ViewerService) *APIHandler {
	return &APIHandler{roomService: roomService, eventService: eventService, sessionService: sessionService, viewerService: viewerService}
}

// GetOrCreateViewerID: 視聴者端末識別用の ID を払い出す
func (h *APIHandler) GetOrCreateViewerID(c echo.Context) error {
	var existing string
	if cookie, err := c.Cookie("viewer_id"); err == nil {
		existing = cookie.Value
	}
	viewerID, err := h.viewerService.EnsureViewerID(existing)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}
	viewer, err := h.viewerService.GetViewer(viewerID)
	if err != nil {
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
	var req struct {
		EventType  string `json:"event_type"`
		ButtonName string `json:"button_name"`
		ViewerID   string `json:"viewer_id"`
	}
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "invalid body"})
	}
	evTypeStr := req.EventType
	if evTypeStr == "" {
		evTypeStr = req.ButtonName
	}
	if evTypeStr == "" {
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "event_type is required"})
	}
	evType := model.EventType(evTypeStr)
	var viewerID *string
	if req.ViewerID != "" {
		viewerID = &req.ViewerID
	}

	if room.Status == "ended" {
		if viewerID == nil {
			return c.JSON(http.StatusBadRequest, map[string]string{"error": "viewer_id is required after game end"})
		}
		summary, err := h.sessionService.GetViewerSummary(roomID, *viewerID)
		if err != nil {
			return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
		}
		return c.JSON(http.StatusOK, map[string]interface{}{
			"game_over":      true,
			"viewer_summary": summary,
		})
	}
	res, err := h.eventService.ProcessEvent(roomID, evType, viewerID)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}
	return c.JSON(http.StatusOK, res)
}

// GetRoomStats: 現在のイベント種別ごとのカウントと閾値を返す
func (h *APIHandler) GetRoomStats(c echo.Context) error {
	roomID := c.Param("id")
	if _, err := h.roomService.GetRoom(roomID); err != nil {
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}
	stats, err := h.eventService.GetRoomStats(roomID)
	if err != nil {
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
