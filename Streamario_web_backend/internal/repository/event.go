package repository

import (
	"database/sql"
	"fmt"
	"log/slog"
	"strings"
	"time"

	"streamerrio-backend/internal/model"

	"github.com/jmoiron/sqlx"
)

// EventRepository: イベント永続化用インタフェース
// 主要イベントクエリのトレースログを出力し、運用時の観測性を高める。
type EventRepository interface {
	CreateEvent(event *model.Event) error // 単一イベント挿入
	CreateEventsBatch(events []*model.Event) error // バッチ挿入（効率的）
	ListEventViewerCounts(roomID string) ([]model.EventAggregate, error)
	ListEventTotals(roomID string) ([]model.EventTotal, error)
	ListViewerTotals(roomID string) ([]model.ViewerTotal, error)
	ListViewerEventCounts(roomID, viewerID string) ([]model.ViewerEventCount, error)
}

type eventRepository struct {
	db     *sqlx.DB
	logger *slog.Logger
}

// NewEventRepository: 実装生成
func NewEventRepository(db *sqlx.DB, logger *slog.Logger) EventRepository {
	if logger == nil {
		logger = slog.Default()
	}
	return &eventRepository{db: db, logger: logger}
}

// CreateEvent: events テーブルへ挿入 (TriggeredAt 未設定なら現在時刻)
func (r *eventRepository) CreateEvent(event *model.Event) error {
	if event.TriggeredAt.IsZero() {
		event.TriggeredAt = time.Now()
	}
	q := `INSERT INTO events (room_id, viewer_id, event_type, triggered_at, metadata) VALUES ($1,$2,$3,$4,$5)`
	attrs := []any{
		slog.String("repo", "event"),
		slog.String("op", "create_event"),
		slog.String("room_id", event.RoomID),
		slog.String("event_type", string(event.EventType)),
		slog.Bool("has_viewer", event.ViewerID != nil),
	}
	if event.ViewerID != nil {
		attrs = append(attrs, slog.String("viewer_id", *event.ViewerID))
	}
	logger := r.logger.With(attrs...)
	start := time.Now()
	res, err := r.db.Exec(q, event.RoomID, event.ViewerID, event.EventType, event.TriggeredAt, event.Metadata)
	if err != nil {
		logger.Error("db.exec failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

// CreateEventsBatch: 複数イベントを効率的にバッチ挿入
// PostgreSQLのVALUES句を使用して1回のクエリで複数レコードを挿入
func (r *eventRepository) CreateEventsBatch(events []*model.Event) error {
	if len(events) == 0 {
		return nil
	}

	// 現在時刻を設定（未設定の場合）
	now := time.Now()
	for _, event := range events {
		if event.TriggeredAt.IsZero() {
			event.TriggeredAt = now
		}
	}

	// VALUES句を構築
	values := make([]string, len(events))
	args := make([]interface{}, 0, len(events)*5)

	for i, event := range events {
		values[i] = fmt.Sprintf("($%d,$%d,$%d,$%d,$%d)",
			i*5+1, i*5+2, i*5+3, i*5+4, i*5+5)
		args = append(args, event.RoomID, event.ViewerID, event.EventType, event.TriggeredAt, event.Metadata)
	}

	q := fmt.Sprintf(`INSERT INTO events (room_id, viewer_id, event_type, triggered_at, metadata) VALUES %s`,
		strings.Join(values, ","))

	attrs := []any{
		slog.String("repo", "event"),
		slog.String("op", "create_events_batch"),
		slog.Int("count", len(events)),
	}
	if len(events) > 0 {
		attrs = append(attrs,
			slog.String("room_id", events[0].RoomID),
			slog.String("event_type", string(events[0].EventType)))
	}
	logger := r.logger.With(attrs...)

	start := time.Now()
	res, err := r.db.Exec(q, args...)
	if err != nil {
		logger.Error("db.exec batch failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec batch", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

func (r *eventRepository) ListEventViewerCounts(roomID string) ([]model.EventAggregate, error) {
	rows := []struct {
		EventType  model.EventType `db:"event_type"`
		ViewerID   sql.NullString  `db:"viewer_id"`
		ViewerName sql.NullString  `db:"viewer_name"`
		Count      int             `db:"count"`
	}{}
	q := `SELECT e.event_type,
             e.viewer_id,
             v.name AS viewer_name,
             COUNT(*) AS count
      FROM events e
      LEFT JOIN viewers v ON v.id = e.viewer_id
      WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
      GROUP BY e.event_type, e.viewer_id, v.name`
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_event_viewer_counts"),
		slog.String("room_id", roomID),
	)
	start := time.Now()
	if err := r.db.Select(&rows, q, roomID); err != nil {
		logger.Error("db.query failed", slog.Any("error", err))
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
	q := `SELECT event_type, COUNT(*) AS count
        FROM events
        WHERE room_id = $1
        GROUP BY event_type`
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_event_totals"),
		slog.String("room_id", roomID),
	)
	start := time.Now()
	if err := r.db.Select(&rows, q, roomID); err != nil {
		logger.Error("db.query failed", slog.Any("error", err))
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
	q := `SELECT e.viewer_id,
             v.name AS viewer_name,
             COUNT(*) AS count
      FROM events e
      LEFT JOIN viewers v ON v.id = e.viewer_id
      WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
      GROUP BY e.viewer_id, v.name
      ORDER BY count DESC, e.viewer_id`
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_viewer_totals"),
		slog.String("room_id", roomID),
	)
	start := time.Now()
	if err := r.db.Select(&rows, q, roomID); err != nil {
		logger.Error("db.query failed", slog.Any("error", err))
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
	q := `SELECT event_type, COUNT(*) AS count
        FROM events
        WHERE room_id = $1 AND viewer_id = $2
        GROUP BY event_type`
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_viewer_event_counts"),
		slog.String("room_id", roomID),
		slog.String("viewer_id", viewerID),
	)
	start := time.Now()
	if err := r.db.Select(&rows, q, roomID, viewerID); err != nil {
		logger.Error("db.query failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Int("row_count", len(rows)), slog.Duration("elapsed", time.Since(start)))
	return rows, nil
}

func cloneString(s string) *string {
	val := s
	return &val
}
