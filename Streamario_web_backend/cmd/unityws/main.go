package main

import (
	"context"
	"fmt"
	"log/slog"
	"net/http"
	"net/url"
	"os"
	"os/signal"
	"strings"
	"syscall"

	"streamerrio-backend/internal/config"
	"streamerrio-backend/internal/handler"
	"streamerrio-backend/internal/repository"
	"streamerrio-backend/internal/service"
	"streamerrio-backend/pkg/counter"
	"streamerrio-backend/pkg/logger"
	"streamerrio-backend/pkg/pubsub"

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
	log := appLogger.With(slog.String("component", "unityws-bootstrap"))

	// 4. DB 接続確立
	host, port, dbname, sslmode := extractConnInfo(cfg.DatabaseURL)
	log.Info("connecting to database", slog.String("host", host), slog.String("port", port), slog.String("db", dbname), slog.String("sslmode", sslmode))

	db, err := sqlx.Connect("postgres", cfg.DatabaseURL)
	if err != nil {
		log.Error("failed to connect to database", slog.Any("error", err))
		os.Exit(1)
	}
	defer db.Close()

	// 5. Redis 初期化 & カウンタ
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

	// 6. Pub/Sub 初期化
	ps := pubsub.NewRedisPubSub(rdb, appLogger.With(slog.String("component", "pubsub")))

	// 7. リポジトリ
	repoLogger := appLogger.With(slog.String("component", "repository"))
	eventRepo := repository.NewEventRepository(db, repoLogger.With(slog.String("repository", "event")))
	roomRepo := repository.NewRoomRepository(db, repoLogger.With(slog.String("repository", "room")))
	viewerRepo := repository.NewViewerRepository(db, repoLogger.With(slog.String("repository", "viewer")))

	// 8. サービス層
	roomService := service.NewRoomService(roomRepo, cfg)
	wsHandlerLogger := appLogger.With(slog.String("component", "websocket_handler"))
	wsHandler := handler.NewWebSocketHandler(ps, wsHandlerLogger)
	wsHandler.SetRoomService(roomService)
	sender := webSocketAdapter{ws: wsHandler}
	sessionLogger := appLogger.With(slog.String("component", "session_service"))
	sessionService := service.NewGameSessionService(roomService, eventRepo, viewerRepo, redisCounter, sender, sessionLogger)
	wsHandler.SetGameSessionService(sessionService)

	// 9. シグナルハンドリングと Pub/Sub 購読開始
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	go func() {
		if err := wsHandler.StartPubSubSubscription(ctx); err != nil {
			log.Error("pubsub subscription terminated", slog.Any("error", err))
		}
	}()

	sigCh := make(chan os.Signal, 1)
	signal.Notify(sigCh, syscall.SIGINT, syscall.SIGTERM)
	defer signal.Stop(sigCh)

	// 10. Echo 初期化
	e := echo.New()
	e.Logger.SetLevel(elog.DEBUG)
	e.Use(middleware.Logger())
	e.Use(middleware.Recover())

	e.GET("/", healthCheck)
	e.GET("/ws-unity", wsHandler.HandleUnityConnection)
	e.GET("/clients", wsHandler.ListClients)

	log.Info("starting unity websocket server", slog.String("port", cfg.UnityWSPort))

	go func() {
		select {
		case <-sigCh:
			log.Info("shutdown signal received")
			cancel()
			e.Shutdown(context.Background())
		}
	}()

	if err := e.Start(":" + cfg.UnityWSPort); err != nil && err != http.ErrServerClosed {
		log.Error("server stopped", slog.Any("error", err))
		os.Exit(1)
	}
}

func healthCheck(c echo.Context) error {
	return c.JSON(http.StatusOK, map[string]string{
		"status":  "ok",
		"service": "streamerrio-unityws",
		"version": "1.0.0",
	})
}

// webSocketAdapter: WebSocketHandler をサービス側インタフェースに適合させる薄いアダプタ
type webSocketAdapter struct{ ws *handler.WebSocketHandler }

func (a webSocketAdapter) SendEventToUnity(roomID string, payload map[string]interface{}) error {
	return a.ws.SendEventToUnity(roomID, payload)
}

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
