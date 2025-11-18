package service

import (
	"crypto/hmac"
	"crypto/sha256"
	"encoding/base64"
	"encoding/json"
	"errors"
	"fmt"
	"strings"
	"time"
)

type LogTokenIssueInput struct {
	ClientID string
	RoomID   string
	ViewerID string
	Platform string
	Scopes   []string
}

type LogTokenIssueResult struct {
	Token     string
	IssuedAt  time.Time
	ExpiresAt time.Time
	Scopes    []string
}

type LogTokenService struct {
	secret        []byte
	ttl           time.Duration
	defaultScopes []string
	allowedScopes map[string]struct{}
}

func NewLogTokenService(secret string, ttl time.Duration, defaultScopes, allowedScopes []string) (*LogTokenService, error) {
	if strings.TrimSpace(secret) == "" {
		return nil, errors.New("log token secret is required")
	}
	if ttl <= 0 {
		return nil, errors.New("log token ttl must be positive")
	}
	filteredDefault := compactScopes(defaultScopes)
	if len(filteredDefault) == 0 {
		filteredDefault = []string{"log:write"}
	}
	allowed := compactScopes(allowedScopes)
	if len(allowed) == 0 {
		allowed = append([]string{}, filteredDefault...)
	}
	allowedSet := make(map[string]struct{}, len(allowed))
	for _, scope := range allowed {
		allowedSet[scope] = struct{}{}
	}
	return &LogTokenService{
		secret:        []byte(secret),
		ttl:           ttl,
		defaultScopes: filteredDefault,
		allowedScopes: allowedSet,
	}, nil
}

func compactScopes(scopes []string) []string {
	seen := make(map[string]struct{})
	var result []string
	for _, raw := range scopes {
		trimmed := strings.TrimSpace(raw)
		if trimmed == "" {
			continue
		}
		if _, ok := seen[trimmed]; ok {
			continue
		}
		seen[trimmed] = struct{}{}
		result = append(result, trimmed)
	}
	return result
}

func (s *LogTokenService) IssueToken(input LogTokenIssueInput) (*LogTokenIssueResult, error) {
	if input.ClientID == "" {
		return nil, errors.New("client_id is required")
	}
	if input.RoomID == "" {
		return nil, errors.New("room_id is required")
	}
	scopes := s.filterScopes(input.Scopes)
	if len(scopes) == 0 {
		scopes = append([]string{}, s.defaultScopes...)
	}
	issuedAt := time.Now().UTC()
	expiresAt := issuedAt.Add(s.ttl)

	payload := map[string]interface{}{
		"clientId":  input.ClientID,
		"roomId":    input.RoomID,
		"viewerId":  input.ViewerID,
		"platform":  input.Platform,
		"scopes":    scopes,
		"issuedAt":  issuedAt.Format(time.RFC3339Nano),
		"expiresAt": expiresAt.Format(time.RFC3339Nano),
	}

	payloadJSON, err := json.Marshal(payload)
	if err != nil {
		return nil, fmt.Errorf("encode payload: %w", err)
	}

	payloadPart := base64.RawURLEncoding.EncodeToString(payloadJSON)
	mac := hmac.New(sha256.New, s.secret)
	mac.Write([]byte(payloadPart))
	signature := base64.RawURLEncoding.EncodeToString(mac.Sum(nil))

	token := fmt.Sprintf("%s.%s", payloadPart, signature)

	return &LogTokenIssueResult{
		Token:     token,
		IssuedAt:  issuedAt,
		ExpiresAt: expiresAt,
		Scopes:    scopes,
	}, nil
}

func (s *LogTokenService) filterScopes(candidates []string) []string {
	var filtered []string
	seen := make(map[string]struct{})
	for _, scope := range candidates {
		trimmed := strings.TrimSpace(scope)
		if trimmed == "" {
			continue
		}
		if _, ok := s.allowedScopes[trimmed]; !ok {
			continue
		}
		if _, exists := seen[trimmed]; exists {
			continue
		}
		seen[trimmed] = struct{}{}
		filtered = append(filtered, trimmed)
	}
	return filtered
}
