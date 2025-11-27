# Log System Worker

Cloudflare Workers + Hono 製のログ中継サーバです。Unity / Frontend からのクライアントログを受信し、Cloud Logging へ直接転送します。無料プランでも動作するよう Queue には依存せず、失敗時のみ R2 (DLQ) に JSON バッチを退避します。

## ディレクトリ構成

```
log_system/
├─ src/
│  ├─ app.ts           # Hono アプリ本体
│  ├─ routes/          # ルーティング単位のハンドラ
│  ├─ security/        # トークン検証
│  ├─ services/        # GCP 連携やロギング処理
│  └─ utils/, types/   # 共通ユーティリティ
├─ package.json        # npm スクリプト / 依存
├─ tsconfig.json       # TypeScript 設定
└─ wrangler.toml       # Worker / R2 バインディング
```

## セットアップ

1. 依存インストール
   ```bash
   cd log_system
   npm install
   ```
2. Cloudflare Secrets を登録
   ```bash
   wrangler secret put CLIENT_LOG_TOKEN_SECRET
   wrangler secret put GCP_SERVICE_ACCOUNT_JSON
   ```
3. ローカル実行
   ```bash
   npm run dev
   ```

## 主な環境変数 (wrangler `vars`)

| 変数名 | 説明 |
| --- | --- |
| `GCP_LOG_NAME` | Cloud Logging の logName (`projects/<project>/logs/<name>`) |
| `GCP_RESOURCE_TYPE` | optional, 既定は `global` |
| `REPLAY_MAX_BATCH` | `/v1/replay` の一度に処理する最大キー数 |
| `DLQ_BUCKET` | R2 バケットのバインディング名 (wrangler `r2_buckets`) |

Queue は無料プランで利用できないため、当構成では Cloud Logging への直接書き込み＋失敗時の R2 退避のみを行います。必要に応じて将来的に Queue バインディングを追加してください。

## エンドポイント概要

- `POST /v1/ingest`
  - Bearer トークン (HMAC) を検証し、ログイベントを即時で Cloud Logging に書き込み
  - トークンは Go backend (`POST /api/log-token`) から発行される `client_id`/`room_id` 紐づき署名を使用
- `POST /v1/replay`
  - `log:replay` scope を持つトークンで DLQ に退避したバッチを再送
- `GET /healthz`
  - バインディング状態の簡易チェック

## テスト

現状は TypeScript 型チェック (`npm run check`) のみです。今後 wrangler の `integration` テストや Miniflare での e2e を追加予定です。

## 外部クライアント (Unity / Frontend) からの利用手順

1. **視聴者IDの取得**
   - 既存フロントでは `GET https://<backend>/get_viewer_id` を呼び出し、Cookie `viewer_id` を取得しておきます。

2. **ログトークンを発行**
   - `POST https://<backend>/api/log-token` に JSON を送信。
     ```json
     {
       "client_id": "viewer_xxx",
       "viewer_id": "viewer_xxx",
       "room_id": "room_123",
       "platform": "frontend",
       "scopes": ["log:write"]
     }
     ```
   - レスポンス例:
     ```json
     {
       "token": "<payload>.<signature>",
       "expires_at": "2025-11-10T12:34:56.789Z",
       "ttl_seconds": 600,
       "scopes": ["log:write"],
       "room_id": "room_123",
       "client_id": "viewer_xxx"
     }
     ```
   - `expires_at` 付近で再発行し、401 を受けたら即座に更新してください。

3. **ログ送信**
   - Worker へ `POST https://logs.example.com/v1/ingest` を実行。
   - Header: `Authorization: Bearer <token>`, `Content-Type: application/json`。
   - ボディ例:
     ```json
     {
       "events": [
         {
           "timestamp": "2025-11-10T12:34:56.789Z",
           "platform": "frontend",
           "roomId": "room_123",
           "viewerId": "viewer_xxx",
           "severity": "ERROR",
           "eventType": "ui_error",
           "message": "WebSocket reconnect failed",
           "tags": {"build": "1.4.2"},
           "extraJson": {"retryCount": 3}
         }
       ]
     }
     ```
   - 成功レスポンス例: `{ "status": "ok", "mode": "direct", "count": 1 }`。
   - 5xx や `deadLetterKey` が返った場合は、ローカルで再送キューを持つか、後述の `/v1/replay` を活用します。

4. **失敗バッチの再送 (運用向け)**
   - GCP 書き込みに失敗した場合、レスポンスへ `deadLetterKey` が含まれ、R2 に JSON が保存されます。
   - `POST https://logs.example.com/v1/replay` に `{ "keys": ["dlq/1731234567890-abcdef"] }` を渡すことで再送。`log:replay` scope を持つ管理者トークンが必要です。

5. **エラーハンドリングの推奨**
   - `401`: トークン失効 → `/api/log-token` を再実行。
   - `429`/`5xx`: 指数バックオフ (例: 1s, 2s, 4s) を最大3回。以降は端末側で永続キューを持ち、オンライン時に再送。
   - 重大な UI フローでは `tags.request_id` などを固定し、同一イベントの多重送信を検出可能にする。
