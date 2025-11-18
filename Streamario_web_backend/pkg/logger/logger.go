package logger

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log/slog"
	"os"
	"runtime"
	"strings"
	"sync"
	"time"
)

// Config: ロガー初期化用設定
// Level は "debug"/"info"/"warn"/"error"、Format は "text" または "json" を想定。
type Config struct {
	Level     string    // ログレベル (デフォルト: info)
	Format    string    // 出力フォーマット (text/json)
	AddSource bool      // 呼び出し元情報を付与するか
	Output    io.Writer // 出力先 (nil の場合は os.Stdout)
	Service   string    // serviceContext 名称（Cloud Logging 用）
	Component string    // component など任意ラベル
}

// Init: Config に基づいて slog ロガーを初期化し、デフォルトロガーとして登録する。
func Init(cfg Config) (*slog.Logger, error) {
	handler, err := buildHandler(cfg)
	if err != nil {
		return nil, err
	}
	logger := slog.New(handler)
	slog.SetDefault(logger)
	return logger, nil
}

// L: デフォルトロガーを返すヘルパ。
func L() *slog.Logger {
	return slog.Default()
}

func buildHandler(cfg Config) (slog.Handler, error) {
	level, err := parseLevel(cfg.Level)
	if err != nil {
		return nil, err
	}
	lvlVar := new(slog.LevelVar)
	lvlVar.Set(level)

	opts := &slog.HandlerOptions{Level: lvlVar, AddSource: cfg.AddSource}

	writer := cfg.Output
	if writer == nil {
		writer = os.Stdout
	}

	switch strings.ToLower(cfg.Format) {
	case "", "text", "plain", "console":
		return slog.NewTextHandler(writer, opts), nil
	case "json", "structured":
		return newCloudLoggingHandler(writer, opts, cloudMetadata{Service: cfg.Service, Component: cfg.Component}), nil
	default:
		return nil, fmt.Errorf("unsupported log format: %s", cfg.Format)
	}
}

type cloudLoggingHandler struct {
	writer   io.Writer
	opts     *slog.HandlerOptions
	attrs    []slog.Attr
	metadata cloudMetadata
	mu       sync.Mutex
}

type cloudMetadata struct {
	Service   string
	Component string
}

func newCloudLoggingHandler(w io.Writer, opts *slog.HandlerOptions, meta cloudMetadata) slog.Handler {
	return &cloudLoggingHandler{writer: w, opts: opts, metadata: meta}
}

func (h *cloudLoggingHandler) Enabled(_ context.Context, level slog.Level) bool {
	return level >= h.opts.Level.Level()
}

func (h *cloudLoggingHandler) Handle(_ context.Context, record slog.Record) error {
	entry := map[string]any{
		"severity":  mapLevel(record.Level),
		"timestamp": record.Time.UTC().Format(time.RFC3339Nano),
		"message":   record.Message,
	}
	if h.metadata.Service != "" {
		entry["service"] = h.metadata.Service
	}
	if h.metadata.Component != "" {
		entry["component"] = h.metadata.Component
	}
	if h.opts.AddSource && record.PC != 0 {
		if frame, ok := runtime.CallersFrames([]uintptr{record.PC}).Next(); ok {
			entry["sourceLocation"] = map[string]any{
				"file":     frame.File,
				"line":     frame.Line,
				"function": frame.Function,
			}
		}
	}
	attrs := make(map[string]any)
	record.Attrs(func(attr slog.Attr) bool {
		attrs[attr.Key] = attr.Value.Any()
		return true
	})
	if len(attrs) > 0 {
		entry["fields"] = attrs
	}
	if len(h.attrs) > 0 {
		entry["labels"] = attrsToMap(h.attrs)
	}
	data, err := json.Marshal(entry)
	if err != nil {
		return err
	}
	h.mu.Lock()
	defer h.mu.Unlock()
	_, err = h.writer.Write(append(data, '\n'))
	return err
}

func (h *cloudLoggingHandler) WithAttrs(attrs []slog.Attr) slog.Handler {
	clone := *h
	clone.attrs = append(clone.attrs, attrs...)
	return &clone
}

func (h *cloudLoggingHandler) WithGroup(name string) slog.Handler {
	return h
}

func mapLevel(level slog.Level) string {
	switch {
	case level >= slog.LevelError:
		return "ERROR"
	case level >= slog.LevelWarn:
		return "WARNING"
	case level >= slog.LevelInfo:
		return "INFO"
	default:
		return "DEBUG"
	}
}

func attrsToMap(attrs []slog.Attr) map[string]any {
	result := make(map[string]any)
	for _, attr := range attrs {
		result[attr.Key] = attr.Value.Any()
	}
	return result
}

func parseLevel(level string) (slog.Level, error) {
	switch strings.ToLower(level) {
	case "", "info":
		return slog.LevelInfo, nil
	case "debug":
		return slog.LevelDebug, nil
	case "warn", "warning":
		return slog.LevelWarn, nil
	case "error", "err":
		return slog.LevelError, nil
	default:
		return slog.LevelInfo, fmt.Errorf("unsupported log level: %s", level)
	}
}
