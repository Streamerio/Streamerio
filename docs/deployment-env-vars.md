# デプロイ時の環境変数設定

## 概要

Cloud Runへのデプロイ時に必要な環境変数の設定手順です。

## 必要な環境変数

### 必須項目

| 環境変数名 | 説明 | 例 |
|-----------|------|-----|
| `DATABASE_URL` | PostgreSQL接続URL | `postgresql://user:password@host:port/database?sslmode=require` |
| `REDIS_URL` | Redisアドレス | `redis-host:6379` または Upstash URL |

### オプション項目（デフォルト値あり）

| 環境変数名 | 説明 | デフォルト値 |
|-----------|------|------------|
| `PORT` | APIサーバのポート | `8888` |
| `FRONTEND_URL` | CORS許可先 | `*` (全許可) |
| `LOG_LEVEL` | ログレベル (debug/info/warn/error) | `info` |
| `LOG_FORMAT` | ログフォーマット (text/json) | `text` |
| `LOG_ADD_SOURCE` | ログに呼び出し元を付与 (true/false) | `false` |
| `UNITY_WS_PORT` | Unity WebSocket サーバーの待受ポート（Cloud Run では `8080` を指定） | `8890` |

> **備考**: Cloud Run 上ではプラットフォームが `PORT` を 8080 に固定するため、Unity WebSocket サービスでは `UNITY_WS_PORT=8080` を設定してアプリが同じポートでリッスンするようにしてください。

## GitHub Secretsの設定手順

1. GitHubリポジトリページにアクセス
2. `Settings` → `Secrets and variables` → `Actions` を選択
3. `New repository secret` をクリック
4. 以下のSecretを追加：

### 必須設定

```
Name: DATABASE_URL
Value: postgresql://user:password@host:port/database?sslmode=require
```

```
Name: REDIS_URL
Value: your-redis-url (例: Upstash Redis URL)
```

### オプション設定

本番環境では以下も設定することを推奨：

```
Name: PORT
Value: 8888
```

```
Name: FRONTEND_URL
Value: https://your-frontend-domain.com
```

```
Name: LOG_LEVEL
Value: info
```

```
Name: LOG_FORMAT
Value: json
```

```
Name: LOG_ADD_SOURCE
Value: false
```

## データベース接続の設定

### Supabaseを使用する場合

1. Supabaseダッシュボードで `Settings` → `Database` → `Connection string` を確認
2. `URI` タブの接続文字列をコピー
3. `[YOUR-PASSWORD]` を実際のパスワードに置き換え
4. GitHub Secretsの `DATABASE_URL` に設定

### 他のPostgreSQLを使用する場合

以下の形式で接続URLを作成：

```
postgresql://username:password@hostname:port/database?sslmode=require
```

## Redisの設定

### Upstash Redisを使用する場合

1. Upstashダッシュボードで Redis インスタンスを作成
2. 接続URLをコピー（`redis://...` 形式）
3. GitHub Secretsの `REDIS_URL` に設定

### 他のRedisを使用する場合

`host:port` 形式で指定：

```
your-redis-host.com:6379
```

## ローカル開発での設定

ローカル開発時は、`Streamario_web_backend` ディレクトリに `.env` ファイルを作成：

```bash
# Server Configuration
PORT=8888

# CORS Configuration
FRONTEND_URL=*

# Database Configuration
DATABASE_URL=postgresql://user:password@localhost:5432/streamerio?sslmode=disable

# Redis Configuration
REDIS_URL=localhost:6379

# Logging Configuration
LOG_LEVEL=debug
LOG_FORMAT=text
LOG_ADD_SOURCE=true
```

## デプロイフローでの環境変数の適用

GitHub Actionsのワークフロー (`.github/workflows/deploy.yml`) で、設定したSecretが自動的にCloud Runの環境変数として設定されます。

### Cloud Run サービス構成（2025-11-09以降）

- `streamario-web-backend`: REST API 用。Dockerfile = `Streamario_web_backend/Dockerfile`。
  - GitHub Actions で `FRONTEND_URL`, `DATABASE_URL`, `REDIS_URL` を注入。
- `streamario-unityws`: Unity 向け WebSocket 用。Dockerfile = `Streamario_web_backend/Dockerfile.unityws`。
  - `DATABASE_URL`, `REDIS_URL`, `UNITY_WS_PORT`（Cloud Run では 8080）を注入。

2 サービスは同じ Artifact Registry (`asia-northeast1-docker.pkg.dev/streamerio-472706/streamario`) に別名で保存され、`main` への push で同一バージョンタグがデプロイされます。

手動でCloud Runの環境変数を確認/変更する場合：

```bash
# 環境変数を確認
gcloud run services describe streamario-web-backend --region asia-northeast1 --format="value(spec.template.spec.containers[0].env)"

# 環境変数を個別に更新
gcloud run services update streamario-web-backend \
  --region asia-northeast1 \
  --set-env-vars "LOG_LEVEL=debug"
```

## トラブルシューティング

### 環境変数が反映されない

- GitHub Secretsの名前が正確か確認
- Cloud Runサービスの環境変数タブで実際の値を確認
- デプロイログでエラーがないか確認

### データベース接続エラー

- `DATABASE_URL` の形式が正しいか確認
- SSL設定（`sslmode=require`）が適切か確認
- ファイアウォール設定でCloud RunのIPが許可されているか確認

### Redis接続エラー

- `REDIS_URL` の形式が正しいか確認
- Upstash使用時はTLS設定を確認
- Redisサーバーへのアクセス権限を確認
