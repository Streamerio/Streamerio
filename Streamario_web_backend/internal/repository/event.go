package repository

import (
	"database/sql"
	"log/slog"
	"time"

	"streamerrio-backend/internal/model"

	"github.com/jmoiron/sqlx"
)

// EventRepository: イベント永続化用インタフェース
// 主要イベントクエリのトレースログを出力し、運用時の観測性を高める。
type EventRepository interface {
	CreateEvent(roomID string, PushEventMap map[model.EventType]int64, viewerID *string) error // 単一イベント挿入
	ListEventViewerCounts(roomID string) ([]model.EventAggregate, error)
	ListEventTotals(roomID string) ([]model.EventTotal, error)
	ListViewerTotals(roomID string) ([]model.ViewerTotal, error)
	ListViewerEventCounts(roomID, viewerID string) ([]model.ViewerEventCount, error)
	Close() error
}

type eventRepository struct {
	db     *sqlx.DB
	logger *slog.Logger

	// 準備済みステートメントを保持
	createEventStmt           *sqlx.Stmt
	listEventViewerCountsStmt *sqlx.Stmt
	listEventTotalsStmt       *sqlx.Stmt
	listViewerTotalsStmt      *sqlx.Stmt
	listViewerEventCountsStmt *sqlx.Stmt
}

// NewEventRepository: 実装生成
func NewEventRepository(db *sqlx.DB, logger *slog.Logger) EventRepository {
	if logger == nil {
		logger = slog.Default()
	}

	return &eventRepository{
		db:                        db,
		logger:                    logger,
		createEventStmt:           mustPrepare(db, logger, queryCreateEvent),
		listEventViewerCountsStmt: mustPrepare(db, logger, queryListEventViewerCounts),
		listEventTotalsStmt:       mustPrepare(db, logger, queryListEventTotals),
		listViewerTotalsStmt:      mustPrepare(db, logger, queryListViewerTotals),
		listViewerEventCountsStmt: mustPrepare(db, logger, queryListViewerEventCounts),
	}
}

// CreateEvent: events テーブルへ挿入 (TriggeredAt 未設定なら現在時刻)
func (r *eventRepository) CreateEvent(roomID string, PushEventMap map[model.EventType]int64, viewerID *string) error {
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "create_event"),
		slog.String("room_id", roomID),
		slog.Any("PushEventMap", PushEventMap),
		slog.Bool("has_viewer", viewerID != nil),
	)
	start := time.Now()
	res, err := r.createEventStmt.Exec(roomID, viewerID, time.Now(), "{}", PushEventMap[model.SKILL1], PushEventMap[model.SKILL2], PushEventMap[model.SKILL3], PushEventMap[model.ENEMY1], PushEventMap[model.ENEMY2], PushEventMap[model.ENEMY3])
	if err != nil {
		logger.Error("db.exec (prepared) failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

func (r *eventRepository) ListEventViewerCounts(roomID string) ([]model.EventAggregate, error) {
	rows := []struct {
		EventType  model.EventType `db:"event_type"`
		ViewerID   sql.NullString  `db:"viewer_id"`
		ViewerName sql.NullString  `db:"viewer_name"`
		Count      int             `db:"count"`
	}{}
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_event_viewer_counts"),
		slog.String("room_id", roomID),
	)
	start := time.Now()
	if err := r.listEventViewerCountsStmt.Select(&rows, roomID); err != nil {
		logger.Error("db.query (prepared) failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Int("row_count", len(rows)), slog.Duration("elapsed", time.Since(start)))

	aggs := make([]model.EventAggregate, 0, len(rows))
	for _, row := range rows {
		if !row.ViewerID.Valid {
			continue
		}
		var namePtr *string
		if row.ViewerName.Valid {
			namePtr = cloneString(row.ViewerName.String)
		}
		aggs = append(aggs, model.EventAggregate{EventType: row.EventType, ViewerID: row.ViewerID.String, ViewerName: namePtr, Count: row.Count})
	}
	return aggs, nil
}

func (r *eventRepository) ListEventTotals(roomID string) ([]model.EventTotal, error) {
	rows := []model.EventTotal{}
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_event_totals"),
		slog.String("room_id", roomID),
	)
	start := time.Now()
	if err := r.listEventTotalsStmt.Select(&rows, roomID); err != nil {
		logger.Error("db.query (prepared) failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Int("row_count", len(rows)), slog.Duration("elapsed", time.Since(start)))
	return rows, nil
}

func (r *eventRepository) ListViewerTotals(roomID string) ([]model.ViewerTotal, error) {
	rows := []struct {
		ViewerID   sql.NullString `db:"viewer_id"`
		ViewerName sql.NullString `db:"viewer_name"`
		Count      int            `db:"count"`
	}{}
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_viewer_totals"),
		slog.String("room_id", roomID),
	)
	start := time.Now()
	if err := r.listViewerTotalsStmt.Select(&rows, roomID); err != nil {
		logger.Error("db.query (prepared) failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Int("row_count", len(rows)), slog.Duration("elapsed", time.Since(start)))

	totals := make([]model.ViewerTotal, 0, len(rows))
	for _, row := range rows {
		if !row.ViewerID.Valid || row.ViewerID.String == "" {
			continue
		}
		var namePtr *string
		if row.ViewerName.Valid {
			namePtr = cloneString(row.ViewerName.String)
		}
		totals = append(totals, model.ViewerTotal{ViewerID: row.ViewerID.String, ViewerName: namePtr, Count: row.Count})
	}
	return totals, nil
}

func (r *eventRepository) ListViewerEventCounts(roomID, viewerID string) ([]model.ViewerEventCount, error) {
	rows := []model.ViewerEventCount{}
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_viewer_event_counts"),
		slog.String("room_id", roomID),
		slog.String("viewer_id", viewerID),
	)
	start := time.Now()
	if err := r.listViewerEventCountsStmt.Select(&rows, roomID, viewerID); err != nil {
		logger.Error("db.query (prepared) failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Int("row_count", len(rows)), slog.Duration("elapsed", time.Since(start)))
	return rows, nil
}

func cloneString(s string) *string {
	val := s
	return &val
}

func (r *eventRepository) Close() error {
	var firstErr error
	closeStmt := func(s *sqlx.Stmt) {
		if s == nil {
			return
		}
		if err := s.Close(); err != nil && firstErr == nil {
			firstErr = err
		}
	}

	closeStmt(r.createEventStmt)
	closeStmt(r.listEventViewerCountsStmt)
	closeStmt(r.listEventTotalsStmt)
	closeStmt(r.listViewerTotalsStmt)
	closeStmt(r.listViewerEventCountsStmt)

	return firstErr
}
