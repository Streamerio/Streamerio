package service

import (
	"crypto/rand"
	"errors"
	"fmt"
	"time"

	"streamerrio-backend/internal/config"
	"streamerrio-backend/internal/model"
	"streamerrio-backend/internal/repository"

	"github.com/oklog/ulid/v2"
)

// RoomService: ルームのライフサイクル管理 (取得/生成/存在保証)
type RoomService struct {
	repo repository.RoomRepository
	cfg  *config.Config
}

// NewRoomService: 依存注入してサービス生成
func NewRoomService(repo repository.RoomRepository, cfg *config.Config) *RoomService {
	return &RoomService{repo: repo, cfg: cfg}
}

// CreateRoom: ルームを永続化 (ID必須, CreatedAt補完)
func (s *RoomService) CreateRoom(room *model.Room) error {
	if room.ID == "" {
		return errors.New("room id required")
	}
	if room.CreatedAt.IsZero() {
		room.CreatedAt = time.Now()
	}
	return s.repo.Create(room)
}

// GetRoom: 存在しない/期限切れならエラーを返す取得処理
func (s *RoomService) GetRoom(id string) (*model.Room, error) {
	room, err := s.repo.Get(id)
	if err != nil {
		return nil, err
	}
	if room == nil {
		return nil, errors.New("room not found")
	}
	if room.ExpiresAt != nil && time.Now().After(*room.ExpiresAt) {
		return nil, errors.New("room expired")
	}
	return room, nil
}

// GenerateRoom: ULIDを用いて新規ルームを生成し保存
func (s *RoomService) GenerateRoom(streamerID string) (*model.Room, error) {
	entropy := ulid.Monotonic(rand.Reader, 0)
	id := ulid.MustNew(ulid.Timestamp(time.Now()), entropy).String()
	room := &model.Room{
		ID:         id,
		StreamerID: streamerID,
		CreatedAt:  time.Now(),
		Status:     "active",
		Settings:   "{}",
		EndedAt:    nil,
	}
	// TTL 機能は現状未実装 (Config 拡張で追加予定)
	if err := s.repo.Create(room); err != nil {
		return nil, err
	}
	return room, nil
}

// CreateIfNotExists: (WebSocket発行IDをDBへ確定させる用途) 存在しなければ指定 streamerID で作成
func (s *RoomService) CreateIfNotExists(id, streamerID string) error {
	existing, err := s.repo.Get(id)
	if err != nil {
		return err
	}
	if existing != nil {
		return nil
	}
	now := time.Now()
	return s.repo.Create(&model.Room{ID: id, StreamerID: streamerID, CreatedAt: now, Status: "active", Settings: "{}", EndedAt: nil})
}

// MarkEnded: ルームを終了状態へ更新
func (s *RoomService) MarkEnded(id string, endedAt time.Time) error {
	return s.repo.MarkEnded(id, endedAt)
}

// UpdateRoom: ルームを更新
func (s *RoomService) UpdateRoom(id string, room *model.Room) error {
	return s.repo.Update(id, room)
}

// DeleteRoom: ルームを削除
func (s *RoomService) DeleteRoom(id string) error {
	return s.repo.Delete(id)
}

// RestartRoom: 終了済みルームを再開可能にする
func (s *RoomService) RestartRoom(id string) error {
	room, err := s.repo.Get(id)
	if err != nil {
		return err
	}
	if room == nil {
		return errors.New("room not found")
	}

	// 既に active なら何もしない
	if room.Status == "active" {
		return nil
	}

	// ended 以外の状態からは再開できない
	if room.Status != "ended" {
		return fmt.Errorf("room status must be 'ended' to restart, got: %s", room.Status)
	}

	// ステータスを active に戻す
	room.Status = "active"
	room.EndedAt = nil

	return s.repo.Update(id, room)
}
