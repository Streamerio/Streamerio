package repository

import (
	"log/slog"

	"github.com/jmoiron/sqlx"
)

// mustPrepare: 起動時に必ず成功させたい prepared statement 準備ヘルパ
func mustPrepare(db *sqlx.DB, logger *slog.Logger, query string) *sqlx.Stmt {
	stmt, err := db.Preparex(query)
	if err != nil {
		if logger == nil {
			logger = slog.Default()
		}
		logger.Error("failed to prepare statement", slog.Any("error", err), slog.String("query", query))
		panic(err)
	}
	return stmt
}
