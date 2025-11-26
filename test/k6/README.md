# k6 負荷テスト

視聴者フロントエンドがバックエンドへ送信するリクエストを模倣した k6 スクリプトです。名前登録フローを除き、以下の順序で API を叩きます。

1. `GET /get_viewer_id`
   - 各 VU（仮想ユーザー）につき一度だけ実行し、`viewer_id` を払い出します。
2. `POST /api/rooms/{roomId}/events`
   - 1.2 秒間隔でリクエストを送り、`push_events` にランダムな 3 件を含めます。

## 実行方法

`test/k6/run.sh` が `.env` を `source` し、環境変数を設定してから k6 を起動します（`.env` は `KEY=value` 形式で記述してください）。

```bash
# デフォルトの test/k6/.env を使用
test/k6/run.sh test/k6/streamerio_load_test.js

# 任意の .env を指定
K6_ENV_FILE=./test/k6/.env.staging test/k6/run.sh test/k6/streamerio_load_test.js

# 直接環境変数で上書きすることも可能
BASE_URL=https://api.example.com ROOM_ID=01ABC... test/k6/run.sh test/k6/streamerio_load_test.js
```

利用可能な環境変数 (.env または直接指定):

| 変数 | デフォルト | 説明 |
| --- | --- | --- |
| `BASE_URL` | `http://localhost:8888` | バックエンド HTTP ベース URL |
| `ROOM_ID` | `dev-room` | テスト対象のルーム ID / streamer ID |
| `VUS` | `100` | 同時接続ユーザー数 |
| `DURATION` | `2m` | 負荷テスト継続時間 |
| `PUSH_INTERVAL` | `1.2` | イベント送信間隔（秒） |
| `EVENTS_PER_REQUEST` | `3` | 1 リクエストに含める event 件数 |
| `VIEWER_ID_RETRY` | `3` | `GET /get_viewer_id` のリトライ回数 |
| `VIEWER_ID_RETRY_INTERVAL` | `0.5` | viewer_id 取得失敗時のリトライ待機秒 |
| `IDENTITY_TIMEOUT` | `6s` | viewer_id 取得リクエストのタイムアウト |
| `EVENT_TIMEOUT` | `10s` | イベント送信リクエストのタイムアウト |
| `K6_ENV_FILE` | `test/k6/.env` | `run.sh` が読み込む .env パス |
| `K6_ENV_FILE` | `test/k6/.env` | 読み込む .env ファイルパス (`open()` 基準) |

## 期待される挙動

- `GET /get_viewer_id` は Cookie を受け取り、同一 VU では再利用されます。
- `POST /api/rooms/{roomId}/events` は 1.2 秒ごとに送信され、バックエンド側の 1.2 秒集計サイクルと近い条件で検証できます。
- `http_req_duration` と `http_req_failed` のしきい値を設定してあり、95 パーセンタイル遅延や失敗率が基準を超えるとテストが失敗します。
