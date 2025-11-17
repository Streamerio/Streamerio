package counter

import (
	"context"
	"fmt"
	"log/slog"
	"time"

	"github.com/redis/go-redis/v9"
)

// redisCounter: Redis を利用した本番向けカウンタ実装
// 各コマンドの遅延を計測しログへ記録する。
type redisCounter struct {
	rdb    *redis.Client
	window time.Duration // アクティブ視聴判定窓 (デフォルト5分)
	logger *slog.Logger
}

// NewRedisCounter: 実装生成 (5分窓)
func NewRedisCounter(rdb *redis.Client, logger *slog.Logger) Counter {
	if logger == nil {
		logger = slog.Default()
	}
	return &redisCounter{rdb: rdb, window: 5 * time.Minute, logger: logger}
}

func (rc *redisCounter) keyCount(roomID, eventType string) string {
	return fmt.Sprintf("room:%s:cnt:%s", roomID, eventType)
}
func (rc *redisCounter) keyViewers(roomID string) string {
	return fmt.Sprintf("room:%s:viewers", roomID)
}

// Increment: Redis INCR で+1し現在値返却
func (rc *redisCounter) Increment(roomID, eventType string) (int64, error) {
	key := rc.keyCount(roomID, eventType)
	logger := rc.logger.With(
		slog.String("op", "increment"),
		slog.String("room_id", roomID),
		slog.String("event_type", eventType),
		slog.String("key", key),
	)
	start := time.Now()
	val, err := rc.rdb.Incr(context.Background(), key).Result()
	if err != nil {
		logger.Error("redis.incr failed", slog.Any("error", err))
		return 0, err
	}
	logger.Debug("redis.incr", slog.Int64("value", val), slog.Duration("elapsed", time.Since(start)))
	return val, nil
}

// Get: 現在カウント取得 (キー無ければ0)
func (rc *redisCounter) Get(roomID, eventType string) (int64, error) {
	key := rc.keyCount(roomID, eventType)
	logger := rc.logger.With(
		slog.String("op", "get"),
		slog.String("room_id", roomID),
		slog.String("event_type", eventType),
		slog.String("key", key),
	)
	start := time.Now()
	v, err := rc.rdb.Get(context.Background(), key).Int64()
	if err == redis.Nil {
		logger.Debug("redis.get", slog.Bool("hit", false), slog.Duration("elapsed", time.Since(start)))
		return 0, nil
	}
	if err != nil {
		logger.Error("redis.get failed", slog.Any("error", err))
		return 0, err
	}
	logger.Debug("redis.get", slog.Bool("hit", true), slog.Int64("value", v), slog.Duration("elapsed", time.Since(start)))
	return v, nil
}

// Reset: カウントキー削除
func (rc *redisCounter) Reset(roomID, eventType string) error {
	key := rc.keyCount(roomID, eventType)
	logger := rc.logger.With(
		slog.String("op", "reset"),
		slog.String("room_id", roomID),
		slog.String("event_type", eventType),
		slog.String("key", key),
	)
	start := time.Now()
	if err := rc.rdb.Del(context.Background(), key).Err(); err != nil {
		logger.Warn("redis.del failed", slog.Any("error", err))
		return err
	}
	logger.Debug("redis.del", slog.Duration("elapsed", time.Since(start)))
	return nil
}

// UpdateViewerActivity: ZSET に時刻をスコアとして追加し古い視聴者をクリーン
func (rc *redisCounter) UpdateViewerActivity(roomID, viewerID string) error {
	key := rc.keyViewers(roomID)
	logger := rc.logger.With(
		slog.String("op", "update_viewer_activity"),
		slog.String("room_id", roomID),
		slog.String("viewer_id", viewerID),
		slog.String("key", key),
	)
	ctx := context.Background()

	start := time.Now()
	if err := rc.rdb.ZAdd(ctx, key, redis.Z{Score: float64(time.Now().Unix()), Member: viewerID}).Err(); err != nil {
		logger.Error("redis.zadd failed", slog.Any("error", err))
		return err
	}
	logger.Debug("redis.zadd", slog.Duration("elapsed", time.Since(start)))

	cutoff := time.Now().Add(-rc.window).Unix()
	pruneStart := time.Now()
	if err := rc.rdb.ZRemRangeByScore(ctx, key, "-inf", fmt.Sprintf("%f", float64(cutoff-1))).Err(); err != nil {
		logger.Warn("redis.zremrangebyscore failed", slog.Any("error", err))
		return err
	}
	logger.Debug("redis.zremrangebyscore", slog.Duration("elapsed", time.Since(pruneStart)), slog.Int64("cutoff", cutoff))
	return nil
}

// GetActiveViewerCount: ZSET から窓内の要素数をカウント
func (rc *redisCounter) GetActiveViewerCount(roomID string) (int64, error) {
	key := rc.keyViewers(roomID)
	logger := rc.logger.With(
		slog.String("op", "get_active_viewer_count"),
		slog.String("room_id", roomID),
		slog.String("key", key),
	)
	cutoff := time.Now().Add(-rc.window).Unix()
	ctx := context.Background()
	start := time.Now()
	count, err := rc.rdb.ZCount(ctx, key, fmt.Sprintf("%f", float64(cutoff)), "+inf").Result()
	if err != nil {
		logger.Error("redis.zcount failed", slog.Any("error", err))
		return 0, err
	}
	logger.Debug("redis.zcount", slog.Int64("count", count), slog.Duration("elapsed", time.Since(start)), slog.Int64("cutoff", cutoff))
	return count, nil
}
