package repository

import (
	"database/sql"
	"log/slog"
	"time"

	"streamerrio-backend/internal/model"

	"github.com/jmoiron/sqlx"
)

type ViewerRepository interface {
	Create(viewer *model.Viewer) error
	Exists(id string) (bool, error)
	Get(id string) (*model.Viewer, error)
	Close() error
}

type viewerRepository struct {
	db     *sqlx.DB
	logger *slog.Logger

	// 準備済みステートメント
	createStmt *sqlx.Stmt
	existsStmt *sqlx.Stmt
	getStmt    *sqlx.Stmt
}

func NewViewerRepository(db *sqlx.DB, logger *slog.Logger) ViewerRepository {
	if logger == nil {
		logger = slog.Default()
	}

	return &viewerRepository{
		db:         db,
		logger:     logger,
		createStmt: mustPrepare(db, logger, queryCreateViewer),
		existsStmt: mustPrepare(db, logger, queryExistsViewer),
		getStmt:    mustPrepare(db, logger, queryGetViewer),
	}
}

func (r *viewerRepository) Create(viewer *model.Viewer) error {
	if viewer.CreatedAt.IsZero() {
		viewer.CreatedAt = time.Now()
	}
	logger := r.logger.With(
		slog.String("repo", "viewer"),
		slog.String("op", "create"),
		slog.String("viewer_id", viewer.ID),
	)
	start := time.Now()
	res, err := r.createStmt.Exec(viewer.ID, viewer.Name, viewer.CreatedAt, viewer.UpdatedAt)
	if err != nil {
		logger.Error("db.exec (prepared) failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

func (r *viewerRepository) Exists(id string) (bool, error) {
	var exists bool
	logger := r.logger.With(
		slog.String("repo", "viewer"),
		slog.String("op", "exists"),
		slog.String("viewer_id", id),
	)
	start := time.Now()
	if err := r.existsStmt.Get(&exists, id); err != nil {
		logger.Error("db.query (prepared) failed", slog.Any("error", err))
		return false, err
	}
	logger.Debug("db.query", slog.Bool("exists", exists), slog.Duration("elapsed", time.Since(start)))
	return exists, nil
}

func (r *viewerRepository) Get(id string) (*model.Viewer, error) {
	var viewer model.Viewer
	logger := r.logger.With(
		slog.String("repo", "viewer"),
		slog.String("op", "get"),
		slog.String("viewer_id", id),
	)
	start := time.Now()
	if err := r.getStmt.Get(&viewer, id); err != nil {
		if err == sql.ErrNoRows {
			logger.Debug("db.query (prepared)", slog.Bool("found", false), slog.Duration("elapsed", time.Since(start)))
			return nil, nil
		}
		logger.Error("db.query (prepared) failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query (prepared)", slog.Bool("found", true), slog.Duration("elapsed", time.Since(start)))
	return &viewer, nil
}

func (r *viewerRepository) Close() error {
	var firstErr error
	closeStmt := func(s *sqlx.Stmt) {
		if s == nil {
			return
		}
		if err := s.Close(); err != nil && firstErr == nil {
			firstErr = err
		}
	}
	closeStmt(r.createStmt)
	closeStmt(r.existsStmt)
	closeStmt(r.getStmt)
	return firstErr
}
