package config

import (
	"fmt"
	"os"
	"strings"
	"time"
)

// Config: アプリケーション全体の設定値コンテナ
// 取得元は基本的に環境変数。存在しない項目はデフォルトを適用。
type Config struct {
	Port         string // APIサーバ待受ポート
	UnityWSPort  string // Unity向けWebSocketサーバ待受ポート
	FrontendURL  string // CORS 許可先 ("*" は全許可)
	DatabaseURL  string // PostgreSQL 接続 DSN or URL
	RedisURL     string // Redis アドレス (host:port)
	LogLevel     string // ログレベル (debug/info/warn/error)
	LogFormat    string // ログ出力フォーマット (text/json)
	LogAddSource bool   // ログに呼び出し元を付与するか
	// Log relay token 発行用設定
	LogRelayTokenSecret   string
	LogRelayTokenTTL      time.Duration
	LogRelayDefaultScopes []string
	LogRelayAllowedScopes []string
}

// Load: 環境変数から設定を組み立て (不足はデフォルト補完)
// 優先順位:
//  1. 直接 URL (DATABASE_URL / REDIS_URL)
//  2. 個別値を合成 (DB_HOST, DB_PORT, ...)
//  3. デフォルト (localhost 等)
func Load() (*Config, error) {
	cfg := &Config{}

	// Port
	cfg.Port = getEnv("PORT", "8888")

	// Unity WebSocket Port
	cfg.UnityWSPort = getEnv("UNITY_WS_PORT", "8890")

	// Frontend (CORS)
	cfg.FrontendURL = getEnv("FRONTEND_URL", "*")

	// Database URL (Supabase/Postgres)
	// 優先順:
	//  1. DATABASE_URL（Supabase推奨: ダッシュボードの接続文字列）
	//  2. SUPABASE_DB_URL（別名）
	//  3. 個別値からDSNを生成（sslmodeは DB_SSLMODE で制御）
	if url := os.Getenv("DATABASE_URL"); url != "" {
		cfg.DatabaseURL = url
	} else if url := os.Getenv("SUPABASE_DB_URL"); url != "" {
		cfg.DatabaseURL = url
	} else {
		host := getEnv("DB_HOST", "localhost")
		port := getEnv("DB_PORT", "5432")
		user := getEnv("DB_USER", "postgres")
		pass := getEnv("DB_PASSWORD", "postgres")
		name := getEnv("DB_NAME", "streamerio")
		sslmode := getEnv("DB_SSLMODE", "require") // Supabase では基本 require を推奨
		cfg.DatabaseURL = fmt.Sprintf(
			"host=%s port=%s user=%s password=%s dbname=%s sslmode=%s",
			host, port, user, pass, name, sslmode,
		)
	}

	// Redis URL (addr only)
	if rurl := os.Getenv("REDIS_URL"); rurl != "" {
		cfg.RedisURL = rurl
	} else if addr := os.Getenv("REDIS_ADDR"); addr != "" { // fallback key
		cfg.RedisURL = addr
	} else {
		cfg.RedisURL = "localhost:6379"
	}

	// Logging
	cfg.LogLevel = getEnv("LOG_LEVEL", "error")
	cfg.LogFormat = getEnv("LOG_FORMAT", "text")
	cfg.LogAddSource = getEnvBool("LOG_ADD_SOURCE", false)

	// Log relay token
	cfg.LogRelayTokenSecret = getEnv("LOG_RELAY_TOKEN_SECRET", "local-dev-log-token-secret")
	cfg.LogRelayTokenTTL = parseDuration(getEnv("LOG_RELAY_TOKEN_TTL", "10m"), 10*time.Minute)
	defaultScopes := parseCSV(getEnv("LOG_RELAY_DEFAULT_SCOPES", "log:write"))
	if len(defaultScopes) == 0 {
		defaultScopes = []string{"log:write"}
	}
	allowedScopes := parseCSV(os.Getenv("LOG_RELAY_ALLOWED_SCOPES"))
	if len(allowedScopes) == 0 {
		allowedScopes = append([]string{}, defaultScopes...)
	}
	cfg.LogRelayDefaultScopes = defaultScopes
	cfg.LogRelayAllowedScopes = allowedScopes

	return cfg, nil
}

func getEnv(key, def string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return def
}

func getEnvBool(key string, def bool) bool {
	if v := os.Getenv(key); v != "" {
		switch strings.ToLower(v) {
		case "1", "true", "yes", "on":
			return true
		case "0", "false", "no", "off":
			return false
		}
	}
	return def
}

func parseDuration(value string, fallback time.Duration) time.Duration {
	if value == "" {
		return fallback
	}
	if d, err := time.ParseDuration(value); err == nil && d > 0 {
		return d
	}
	return fallback
}

func parseCSV(value string) []string {
	if value == "" {
		return nil
	}
	parts := strings.Split(value, ",")
	var result []string
	for _, part := range parts {
		trimmed := strings.TrimSpace(part)
		if trimmed != "" {
			result = append(result, trimmed)
		}
	}
	return result
}
