package main

import (
	"fmt"
	"log/slog"
	"net/http"
	"net/url"
	"os"
	"strings"

	"streamerrio-backend/internal/config"
	"streamerrio-backend/internal/handler"
	"streamerrio-backend/internal/repository"
	"streamerrio-backend/internal/service"
	"streamerrio-backend/pkg/counter"
	"streamerrio-backend/pkg/logger"

	// PostgreSQLドライバー
	"github.com/jmoiron/sqlx"
	"github.com/joho/godotenv"
	"github.com/labstack/echo/v4"
	"github.com/labstack/echo/v4/middleware"
	elog "github.com/labstack/gommon/log"
	_ "github.com/lib/pq"
	"github.com/redis/go-redis/v9"
)

func main() {
	// 1. 環境変数読み込み (.env があれば適用)
	godotenv.Load()

	// 2. 設定ロード
	cfg, err := config.Load()
	if err != nil {
		fmt.Fprintf(os.Stderr, "failed to load config: %v\n", err)
		os.Exit(1)
	}

	// 3. ロガー初期化
	logCfg := logger.Config{Level: cfg.LogLevel, Format: cfg.LogFormat, AddSource: cfg.LogAddSource}
	appLogger, err := logger.Init(logCfg)
	if err != nil {
		fmt.Fprintf(os.Stderr, "failed to init logger: %v\n", err)
		os.Exit(1)
	}
	log := appLogger.With(slog.String("component", "bootstrap"))

	// 4. DB 接続確立
	// 接続先の概要を安全にログ（パスワードは出力しない）
	host, port, dbname, sslmode := extractConnInfo(cfg.DatabaseURL)
	log.Info("connecting to database", slog.String("host", host), slog.String("port", port), slog.String("db", dbname), slog.String("sslmode", sslmode))

	db, err := sqlx.Connect("postgres", cfg.DatabaseURL)
	if err != nil {
		log.Error("failed to connect to database", slog.Any("error", err))
		os.Exit(1)
	}
	defer db.Close()

	// 5. Redis 初期化 & カウンタ (イベント数 / 視聴者アクティビティ)
	var rdb *redis.Client
	if strings.HasPrefix(cfg.RedisURL, "redis://") || strings.HasPrefix(cfg.RedisURL, "rediss://") {
		opt, err := redis.ParseURL(cfg.RedisURL)
		if err != nil {
			log.Error("invalid redis url", slog.String("redis_url", cfg.RedisURL), slog.Any("error", err))
			os.Exit(1)
		}
		rdb = redis.NewClient(opt)
	} else {
		rdb = redis.NewClient(&redis.Options{Addr: cfg.RedisURL})
	}
	redisCounter := counter.NewRedisCounter(rdb, appLogger.With(slog.String("component", "redis_counter")))

	// 6. リポジトリ (永続層) 準備
	repoLogger := appLogger.With(slog.String("component", "repository"))
	eventRepo := repository.NewEventRepository(db, repoLogger.With(slog.String("repository", "event")))
	roomRepo := repository.NewRoomRepository(db, repoLogger.With(slog.String("repository", "room")))
	viewerRepo := repository.NewViewerRepository(db, repoLogger.With(slog.String("repository", "viewer")))

	// 7. サービス層生成
	roomService := service.NewRoomService(roomRepo, cfg)
	wsHandler := handler.NewWebSocketHandler()
	wsHandler.SetRoomService(roomService)
	sender := webSocketAdapter{ws: wsHandler}
	eventLogger := appLogger.With(slog.String("component", "event_service"))
	sessionLogger := appLogger.With(slog.String("component", "session_service"))
	eventService := service.NewEventService(redisCounter, eventRepo, sender, eventLogger)
	sessionService := service.NewGameSessionService(roomService, eventRepo, viewerRepo, redisCounter, sender, sessionLogger)
	viewerService := service.NewViewerService(viewerRepo)
	wsHandler.SetGameSessionService(sessionService)
	apiHandler := handler.NewAPIHandler(roomService, eventService, sessionService, viewerService)

	// 8. Echo フレームワーク初期化 & ミドルウェア
	e := echo.New()
	e.Logger.SetLevel(elog.DEBUG)
	e.Use(middleware.Logger())  // アクセスログ
	e.Use(middleware.Recover()) // パニック回復

	// 9. CORS 設定
	// 認証付き（Cookie 同送）要求に対応するため AllowCredentials=true とし、
	// オリジンは allowlist（環境変数 FRONTEND_URL）に限定する。
	// 注意: AllowCredentials=true の場合、"*" は使用できない。
	allowCredentials := true
	allowOrigins := []string{cfg.FrontendURL}
	if cfg.FrontendURL == "*" {
		// デフォルト設定時は資格情報を扱わない想定
		allowCredentials = false
	}
	e.Use(middleware.CORSWithConfig(middleware.CORSConfig{
		AllowOrigins:     allowOrigins,
		AllowMethods:     []string{http.MethodGet, http.MethodPost, http.MethodOptions},
		AllowHeaders:     []string{"ngrok-skip-browser-warning", echo.HeaderContentType},
		AllowCredentials: allowCredentials,
	}))

	// 10. ルーティング定義
	e.GET("/", healthCheck)
	e.GET("/get_viewer_id", apiHandler.GetOrCreateViewerID)
	// WebSocket
	e.GET("/ws-unity", wsHandler.HandleUnityConnection)
	e.GET("/clients", wsHandler.ListClients)
	// REST API
	api := e.Group("/api")
	api.GET("/rooms/:id", apiHandler.GetRoom)
	api.POST("/rooms/:id/events", apiHandler.SendEvent)
	api.GET("/rooms/:id/stats", apiHandler.GetRoomStats)
	api.GET("/rooms/:id/results", apiHandler.GetRoomResult)
	api.POST("/viewers/set_name", apiHandler.SetViewerName)

	// 11. サーバ起動
	log.Info("starting http server", slog.String("port", cfg.Port))
	if err := e.Start(":" + cfg.Port); err != nil {
		log.Error("server stopped", slog.Any("error", err))
		os.Exit(1)
	}
}

func healthCheck(c echo.Context) error {
	return c.JSON(http.StatusOK, map[string]string{
		"status":  "ok",
		"service": "streamerrio",
		"version": "1.0.0",
	})
}

// webSocketAdapter: 既存 WebSocketHandler をサービス側インタフェースに適合させる薄いアダプタ
type webSocketAdapter struct{ ws *handler.WebSocketHandler }

func (a webSocketAdapter) SendEventToUnity(roomID string, payload map[string]interface{}) error {
	return a.ws.SendEventToUnity(roomID, payload)
}

// extractConnInfo: DSN/URL から host/port/dbname/sslmode を抽出（ログ用途）
func extractConnInfo(dsn string) (host, port, dbname, sslmode string) {
	host, port, dbname, sslmode = "", "", "", ""
	lower := strings.ToLower(dsn)
	if strings.HasPrefix(lower, "postgres://") || strings.HasPrefix(lower, "postgresql://") {
		if u, err := url.Parse(dsn); err == nil {
			host = u.Hostname()
			port = u.Port()
			if u.Path != "" && u.Path != "/" {
				dbname = strings.TrimPrefix(u.Path, "/")
			}
			if v := u.Query().Get("sslmode"); v != "" {
				sslmode = v
			}
		}
		return
	}
	// キーバリュースタイル: key=value key=value ...
	// 例: host=... port=5432 user=... password=... dbname=... sslmode=require
	parts := strings.Fields(dsn)
	for _, p := range parts {
		kv := strings.SplitN(p, "=", 2)
		if len(kv) != 2 {
			continue
		}
		k := strings.ToLower(strings.TrimSpace(kv[0]))
		v := strings.TrimSpace(kv[1])
		switch k {
		case "host":
			host = v
		case "port":
			port = v
		case "dbname":
			dbname = v
		case "sslmode":
			sslmode = v
		}
	}
	return
}
