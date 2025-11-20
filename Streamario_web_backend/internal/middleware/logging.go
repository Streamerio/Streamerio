package middleware

import (
	"log/slog"
	"time"

	"github.com/labstack/echo/v4"
)

// StructuredLogger emits request logs through slog so that Cloud Logging can parse severity/fields.
func StructuredLogger(logger *slog.Logger) echo.MiddlewareFunc {
	log := logger
	if log == nil {
		log = slog.Default()
	}
	return func(next echo.HandlerFunc) echo.HandlerFunc {
		return func(c echo.Context) error {
			start := time.Now()
			err := next(c)
			latency := time.Since(start)

			req := c.Request()
			res := c.Response()
			if err != nil || res.Status >= 500 {
				log.Error("http_request",
					slog.String("method", req.Method),
					slog.String("path", req.URL.Path),
					slog.Int("status", res.Status),
					slog.String("remote_ip", c.RealIP()),
					slog.String("user_agent", req.UserAgent()),
					slog.Duration("latency", latency),
					slog.Any("error", err),
				)
			}
			return err
		}
	}
}
