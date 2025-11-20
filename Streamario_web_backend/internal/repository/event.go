package repository

import (
	"database/sql"
	"fmt"
	"log/slog"
	"strings"
	"sync"
	"time"

	"streamerrio-backend/internal/model"

	"github.com/jmoiron/sqlx"
)

// EventRepository: イベント永続化用インタフェース
// 主要イベントクエリのトレースログを出力し、運用時の観測性を高める。
type EventRepository interface {
	CreateEvent(event *model.Event) error          // 単一イベント挿入
	CreateEventsBatch(events []*model.Event) error // バッチ挿入（効率的）
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

	// バッチ挿入用の prepared statement キャッシュ（バッチサイズごと）
	batchStmts map[int]*sqlx.Stmt
	batchMutex sync.RWMutex
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
		batchStmts:                make(map[int]*sqlx.Stmt),
	}
}

// CreateEvent: events テーブルへ挿入 (TriggeredAt 未設定なら現在時刻)
func (r *eventRepository) CreateEvent(event *model.Event) error {
	if event.TriggeredAt.IsZero() {
		event.TriggeredAt = time.Now()
	}
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
	res, err := r.createEventStmt.Exec(event.RoomID, event.ViewerID, event.EventType, event.TriggeredAt, event.Metadata)
	if err != nil {
		logger.Error("db.exec (prepared) failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

// CreateEventsBatch: 複数イベントを効率的にバッチ挿入
// PostgreSQLのVALUES句を使用して1回のクエリで複数レコードを挿入
// バッチサイズごとに prepared statement をキャッシュして使用
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

	batchSize := len(events)
	stmt := r.getOrCreateBatchStmt(batchSize)
	if stmt == nil {
		return fmt.Errorf("failed to get or create prepared statement for batch size %d", batchSize)
	}

	// 引数を構築
	args := make([]interface{}, 0, batchSize*5)
	for _, event := range events {
		args = append(args, event.RoomID, event.ViewerID, event.EventType, event.TriggeredAt, event.Metadata)
	}

	attrs := []any{
		slog.String("repo", "event"),
		slog.String("op", "create_events_batch"),
		slog.Int("count", batchSize),
		slog.String("room_id", events[0].RoomID),
		slog.String("event_type", string(events[0].EventType)),
	}
	logger := r.logger.With(attrs...)

	start := time.Now()
	res, err := stmt.Exec(args...)
	if err != nil {
		logger.Error("db.exec batch (prepared) failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec batch (prepared)", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

// getOrCreateBatchStmt: バッチサイズに応じた prepared statement を取得または作成
func (r *eventRepository) getOrCreateBatchStmt(batchSize int) *sqlx.Stmt {
	// 書き込みロックを取得して statement を作成
	r.batchMutex.Lock()
	defer r.batchMutex.Unlock()

	if stmt, exists := r.batchStmts[batchSize]; exists {
		return stmt
	}

	// VALUES句を構築
	values := make([]string, batchSize)
	for i := 0; i < batchSize; i++ {
		values[i] = fmt.Sprintf("($%d,$%d,$%d,$%d,$%d)",
			i*5+1, i*5+2, i*5+3, i*5+4, i*5+5)
	}

	query := fmt.Sprintf(`INSERT INTO events (room_id, viewer_id, event_type, triggered_at, metadata) VALUES %s`,
		strings.Join(values, ","))

	stmt, err := r.db.Preparex(query)
	if err != nil {
		r.logger.Error("failed to prepare batch statement",
			slog.Any("error", err),
			slog.Int("batch_size", batchSize),
			slog.String("query", query))
		return nil
	}

	r.batchStmts[batchSize] = stmt
	return stmt
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

	// バッチ用 prepared statement をすべてクローズ
	r.batchMutex.Lock()
	defer r.batchMutex.Unlock()
	for _, stmt := range r.batchStmts {
		closeStmt(stmt)
	}
	r.batchStmts = make(map[int]*sqlx.Stmt)

	return firstErr
}
