## 2025-11-14 Viewer向け操作ガイドモーダル実装

### 目的
- 視聴者ページ入場時に必ず操作説明と名前入力を完了してもらい、匿名状態でのボタン送信を防ぐ。
- 操作手順を後から見返せるようにし、GIF 等のリッチメディアを差し替えやすい土台を整える。

### 実装概要
- `front/src/components/OnboardingModal.tsx` を新設し、ステップ配列＋任意メディアを受け取って汎用モーダルとして描画。前後移動とカスタム完了アクションをサポート。
- 視聴者ページ (`front/src/app/page.tsx`) でモーダル状態を管理。ページ入場時は必ず表示し、最終ステップで表示名を保存するまで閉じられないよう制御。
- `ButtonGrid` への入力はガイド完了前は無効化し、ヘッダーの `?` ボタンからいつでも再オープンできるようにした。
- 表示名保存処理を `saveName` に集約し、空文字送信を防止。ヘッダーの保存ボタンやモーダルの完了ボタンは viewerId 取得後のみ有効化。

### 意図・補足
- GIF を差し替えられるよう、モーダルは `media` スロットを設けダミー表示を配置。将来のアセット差込を UI 変更なしで行える。
- 高凝集: ガイド関連 state を `ViewerContent` に閉じ、他コンポーネントへ副作用を漏らさない。低結合: `OnboardingModal` は props 駆動で他ページでも再利用可能。
- 完了情報は `sessionStorage` に持ちつつ、サーバーに保存済みの viewer 名があれば即完了扱いとしてユーザビリティを確保。

## 2025-10-03 WebSocket再接続（roomId維持）対応

### 目的
- Cloud Run スケールインやネットワーク断で Unity 側の WebSocket が切断された際、同一 `room_id` のまま速やかに再接続できるようにする。

### 実装概要
- `GET /ws-unity?room_id=...` を受けると、既存の `room_id` に紐づく接続を置換して再登録するように変更。
- 新規接続は従来通り新しい `room_id` を払い出す。
- `unregister` は引数の接続が現在の接続と一致する場合のみ削除し、再接続置換時の誤削除を防止。
- 再接続時の初期メッセージは `type: "room_ready"` にしてクライアント側の分岐を容易に。

### 変更ファイル
- `internal/handler/websocket.go`
  - `registerNew`/`registerWithID` を新設
  - `unregister(id, ws, c)` に変更し、接続一致時のみ削除
  - `HandleUnityConnection` で `room_id` クエリを解釈し、初期メッセージ種別を `room_created`/`room_ready` で出し分け

### 意図・設計上の判断
- 高凝集: 接続登録/解除の責務を `WebSocketHandler` 内に閉じる。
- 低結合: 既存サービス層への影響を最小化し、ハンドラ内のみで再接続制御を完結。
- 安全性: defer の `unregister` が新接続を消してしまう競合を防ぐため、接続ポインタ比較で保護。

### 今後の課題
- インスタンス跨ぎ（Cloud Run 水平スケール時）の接続管理は別途 Redis などで分散管理が必要。
- Unity クライアント側で `room_ready` 受信をトリガに状態復元（必要なら）を検討。
# 開発ログ (game-end-handling)

- 2024-09-21 02:00 作業ブランチ `game-end-handling` を作成し、既存仕様と `docs/game_end_plan.md` を再確認。終了イベントと集計要件を把握し、命名統一済み (`skill*/enemy*`) を前提に進める方針を明文化。
- 2024-09-21 02:35 バックエンドにゲーム終了処理の骨格を実装。`GameSessionService` を追加して終了集計・Unity へのサマリー送信・ルーム状態管理を集約し、WebSocket ハンドラで `game_end` メッセージを捕捉するよう調整。終了後リクエストに応答するため、API へ viewer サマリー返却と結果取得エンドポイントを追加。
- 2024-09-21 03:05 フロントエンドに終了検知とリザルトページを追加。イベント送信後に `game_over` 応答を受け取った場合は viewer サマリーをセッション保存し、`/result/[roomId]` へ遷移して集計 API の結果と組み合わせて表示するようにした。
- 2024-09-21 03:25 視聴者端末識別用の `/get_viewer_id` エンドポイントを追加。ULID を払い出して `viewers` テーブルに保存し、Cookie に設定することで既存フロントの `fetchViewerId` が動作するようにした。
- 2024-09-21 03:45 視聴者名を管理する `viewers` テーブル拡張と `/api/viewers/set_name` を追加。フロントのヘッダーに表示名編集フォームを用意し、サーバ側で24文字までに正規化して永続化するよう対応。
- 2025-09-20 12:10 CORS/クッキー運用の見直し。Cloud Run(backend) と Vercel(frontend) 間で `credentials: 'include'` を使うため、Echo の CORS を allowlist（`FRONTEND_URL`）+ `AllowCredentials=true` に変更。`/get_viewer_id` の Cookie を `SameSite=None; Secure` に更新してクロスサイト送受信を許可。ローカルデフォルトの `FRONTEND_URL="*"` の場合は `AllowCredentials=false` とし、意図せずワイルドカード+資格情報が混在しないようガード。
- 2024-09-21 04:10 終了サマリーに視聴者名を含めるようバックエンドを拡張し、Unity への `game_end_summary` と REST レスポンス双方で表示名を送出。リザルト画面は名前を優先表示し、未設定の場合は ULID をフォールバックとして表示するよう更新。

## 2025-10-03 ConnectWebSocketメソッドオーバーロード実装

### 目的
- Unity側のWebSocket接続時に、カスタムURLを指定できるオプションを提供
- 既存の引数なし呼び出しとの互換性を維持
- テスト環境や異なるサーバー環境への接続を可能にする

### 実装概要
- `ConnectWebSocket()` - 引数なし版（既存の動作を維持）
- `ConnectWebSocket(string customUrl)` - カスタムURL指定版
- 引数なし版は内部的にnullを渡して引数あり版を呼び出すことで実装を統一

### 変更ファイル
- `Streamerio_unity/Assets/0_Scripts/0_Common/Websocket/WebsocketManager.cs`
  - メソッドオーバーロードを追加
  - デフォルトURL使用時とカスタムURL使用時の分岐処理を実装

### 意図・設計上の判断
- 高凝集: WebSocket接続の責務をWebsocketManager内に閉じる
- 低結合: 既存の呼び出し元に影響を与えない後方互換性を維持
- 拡張性: 将来的な開発環境やテスト環境での柔軟な接続先変更を可能にする

## 2025-10-07 GitHub Actions デプロイ時の環境変数設定

### 目的
- Cloud Runへのデプロイ時に実行環境で必要な環境変数を設定できるようにする
- GitHub Secretsを使用してセキュアに環境変数を管理する
- データベース（Supabase）やRedis（Upstash）などの外部サービスへの接続情報を安全に設定する

### 実装概要
- `.github/workflows/deploy.yml` の Cloud Run デプロイステップに `--set-env-vars` フラグを追加
- 以下の環境変数をGitHub Secretsから取得してCloud Runに設定：
  - `PORT`: APIサーバのポート（デフォルト: 8888）
  - `FRONTEND_URL`: CORS許可先（本番では具体的なドメインを指定推奨）
  - `DATABASE_URL`: PostgreSQL接続URL（Supabase等）
  - `REDIS_URL`: Redisアドレス（Upstash等）
  - `LOG_LEVEL`: ログレベル（info/debug/warn/error）
  - `LOG_FORMAT`: ログフォーマット（text/json、本番はjson推奨）
  - `LOG_ADD_SOURCE`: ログに呼び出し元情報を付与するか（true/false）

### 変更ファイル
- `.github/workflows/deploy.yml`
  - Deploy to Cloud Runステップに環境変数設定を追加
- `docs/deployment-env-vars.md`（新規作成）
  - 環境変数の詳細説明とGitHub Secretsの設定手順をドキュメント化
  - ローカル開発時の設定例も記載
  - トラブルシューティングガイドを追加

### 意図・設計上の判断
- セキュリティ: 機密情報（DB接続情報等）をGitHub Secretsで管理し、コードに含めない
- 環境分離: 開発/本番環境で異なる設定を使用できるよう柔軟性を確保
- 運用性: 環境変数の変更がデプロイフローで自動的に反映される仕組みを構築
- ドキュメント: 設定手順を明文化し、チーム開発や運用時の参照を容易にする

### 今後の課題
- GitHub Secretsに実際の本番環境の値を設定する必要がある
- 本番環境では `FRONTEND_URL` を `*` から具体的なドメインに変更してCORS制御を厳密化
- ログフォーマットを本番環境では `json` に設定し、構造化ログで監視しやすくする

 
## 2025-10-14 Unity: URL設定の外部化と集中管理

### 目的
- Unity側でハードコードされていた Backend/Frontend/WS のURLを外部設定に移し、保守性と環境切替（dev/stg/prod）の容易さを高める。

### 実装概要
- `Assets/Resources/ServerConfig.json` を追加し、`backendHttpBaseUrl` / `backendWsBaseUrl` / `frontendUrlFormat` を定義。
- `WebsocketManager.cs` で `Resources.Load<TextAsset>("ServerConfig")` を用いて起動時に読込む。失敗時は現行運用中のURLをデフォルトとしてフォールバック。
- フロントURL生成（`GetFrontUrlAsync`）は `frontendUrlFormat` を利用し、`roomId` を埋め込み。
- WebSocket接続URLは `backendWsBaseUrl` + `/ws-unity`（`?room_id=` 付与）で組み立て。
- ヘルスチェックは `backendHttpBaseUrl` + `/` を利用。

### 変更ファイル
- `Streamerio_unity/Assets/Resources/ServerConfig.json`（新規）
- `Streamerio_unity/Assets/0_Scripts/0_Common/Websocket/WebsocketManager.cs`

### 意図・設計上の判断
- 高凝集: URL組み立て責務を `WebsocketManager` に集約しつつ、可変要素（ベースURL）は外部設定に切り出し。
- 低結合: 他スクリプトからURLを直接参照しない。将来は `AppConfig` などの共通ローダーへ移譲可能な構造。
- フェイルセーフ: 設定ファイルが欠落/破損しても、既存の運用URLで動作継続。

### 使い方/運用
- 環境ごとに `Assets/Resources/ServerConfig.json` の内容を切替（例: CI で上書き、またはAddressables/ビルドパイプラインで差し替え）。
- 形式:
  ```json
  {
    "backendHttpBaseUrl": "https://example.com",
    "backendWsBaseUrl": "wss://example.com",
    "frontendUrlFormat": "https://front.example.com/?streamer_id={0}"
  }
  ```


## 2025-10-17 Unity: URL設定をScriptableObjectへ移行

### 目的
- `Assets/Resources/ServerConfig.json` を廃止し、URL設定を ScriptableObject 化。
- 環境切替（dev/stg/prod）をインスペクタで安全・容易に行えるようにする。
- ビルド時の差し替えや CI 注入を簡素化し、保守性を向上。

### 実装概要
- `ApiConfigSO` を新規追加（CreateAssetMenu: `SO/Common/ApiConfigSO`）。
- フィールド:
  - `frontendUrlFormat`（例: `https://streamerio.vercel.app/?streamer_id={0}`）
  - `backendWsUrl`（例: `wss://.../ws-unity`）
  - `backendHttpUrl`（例: `https://.../`）
- `WebsocketManager` に `ApiConfigSO` を `[SerializeField]` で参照し、`Awake` で読込む実装に統一。
- 旧 `Resources.Load<TextAsset>("ServerConfig")` ベースの読込は廃止。
- `Assets/Resources/ServerConfig.json` を削除。

### 変更ファイル
- `Streamerio_unity/Assets/0_Scripts/0_Common/Websocket/ApiConfigSO.cs`（新規）
- `Streamerio_unity/Assets/0_Scripts/0_Common/Websocket/WebsocketManager.cs`（SO 読込に対応）
- `Streamerio_unity/Assets/Resources/ServerConfig.json`（削除）

### 意図・設計上の判断
- 高凝集: URL 組み立ては `WebsocketManager` に集約し、可変値は `ApiConfigSO` に分離。
- 低結合: 他スクリプトは URL 文字列に直接依存せず、`WebsocketManager` の API を利用。
- 運用性: インスペクタで環境値を切替。アセットのデフォルト値は本番相当を設定。
- フェイルセーフ: `ApiConfigSO` 未割当時はログで検知（誤った既定値で稼働しない）。

### 使い方/運用
- メニュー `SO/Common/ApiConfigSO` からアセットを作成。
- シーンの `WebsocketManager`（`_apiConfigSO`）に作成したアセットを割当。
- 環境ごとにアセットを分けるか、CI/ビルドパイプラインで値を上書き。
- フロント URL 生成/WS 接続/HTTP ヘルスチェックはアセット値から自動で組み立て。

### 移行メモ
- 以前の `ServerConfig.json` は不要（Git からも削除済み）。
- `ApiConfigSO` の未割当時は URL が空になり得るため、必ず参照を設定すること。

## 2025-01-27 イベントカウンターの閾値超過分保持機能実装

### 目的
- イベントの閾値到達時にカウントをリセット（0に戻す）するのではなく、閾値を超過した分を保持するように変更
- 視聴者のアクションが無駄にならないよう、超過分を次回の閾値計算に活用する
- より公平で継続的なゲーム体験を提供する

### 実装概要
- `Counter` インターフェースに `SetExcess` メソッドを追加
- Redisカウンターとメモリカウンターの両方に `SetExcess` メソッドを実装
- `EventService.ProcessEvent` で閾値到達時に `Reset` の代わりに `SetExcess` を使用
- 超過分の計算: `excess = current - threshold`

### 変更ファイル
- `pkg/counter/interface.go` - `SetExcess` メソッドをインターフェースに追加
- `pkg/counter/redis.go` - Redis実装に `SetExcess` メソッドを追加
- `pkg/counter/memory.go` - メモリ実装に `SetExcess` メソッドを追加
- `internal/service/event.go` - 閾値到達時の処理を `Reset` から `SetExcess` に変更

### 意図・設計上の判断
- 高凝集: カウンターの責務を `Counter` インターフェース内に集約し、超過分設定も含める
- 低結合: 既存の `Reset` メソッドは残し、新しい `SetExcess` メソッドを追加することで後方互換性を維持
- 公平性: 視聴者のアクションが無駄にならないよう、超過分を次回の閾値計算に活用
- ログ: エラー発生時は詳細なログを出力し、デバッグを容易にする

### 動作例
- 閾値が10で、現在のカウントが15の場合
- 従来: カウントを0にリセット → 次回は0から開始
- 新実装: カウントを5（15-10）に設定 → 次回は5から開始

### 今後の課題
- 超過分が非常に大きくなった場合の上限設定を検討
- 長時間プレイ時のバランス調整の必要性を検討

## 2025-11-05 WebSocket サーバー切り出し前段対応

### 目的
- REST API と Unity 向け WebSocket を別プロセスでスケールさせられるよう起動系を分離
- Pub/Sub を介した配信を前提に、API サーバーから WebSocket 依存を取り除き役割を明確化

### 実装概要
- `cmd/server/main.go` から WebSocket ハンドラ初期化と購読処理を削除し、REST API 専用の起動構成に変更
- 新エントリポイント `cmd/unityws/main.go` を追加し、Redis Pub/Sub 購読と `/ws-unity` エンドポイントを担当させた
- 設定値に `UNITY_WS_PORT` を追加して WebSocket サーバーの待受ポートを独立管理
- API サーバー側の `GameSessionService` には WebSocket 送信者を注入せず、終了集計と REST レスポンスに専念させた
- Cloud Run 用のイメージ切り分け準備として `Dockerfile.unityws` を新設し、WebSocket サーバー専用バイナリをビルドできるようにした

### 意図・設計上の判断
- **高凝集**: API サーバーは REST, WebSocket サーバーは Unity 通信という単一責務になるよう依存を整理
- **低結合**: 共通インフラ（DB, Redis, Pub/Sub）は共有しつつ、起動単位で切り替え可能にするためインタフェース実装（`WebSocketSender`）はそのまま利用
- **スケーリング性**: 負荷に応じて WebSocket サーバーだけを水平スケールしやすくするため、設定とブートストラップを分離
- **互換性**: 既存の Pub/Sub チャネルやハンドラ構成は維持し、Unity 側の接続先はポート変更のみで動作するよう配慮

### 今後の課題
- Docker やデプロイ定義で新しい WebSocket サーバーの起動フローを明記（別コンテナ化）
- 監視・ヘルスチェックエンドポイントを整備し、両サーバーの死活監視を行えるようにする

## 2025-11-10 Cloudflare Worker 中継サーバ検討

### 目的
- GCP ログエクスプローラで集約しているログに Unity/フロントエンドのクライアントログも安全に取り込み、鍵の配布を防ぐ。
- Cloudflare Workers + Hono で軽量な中継サーバを構築できるかを仕様として整理し、今後の PoC を円滑にする。

### 検討内容概要
- クライアント -> Cloudflare Worker -> GCP Logging という三層構成を整理し、入力スキーマ/認証/信頼境界を定義。
- Workers 側でサービスアカウント鍵を `Secret` 管理し、短命アクセストークンを発行して Cloud Logging Ingestion API を叩く方針を確認。
- クライアント認証は backend 署名の `client_log_token` を用いた MAC 認証方式とし、Frontend/Unity から鍵が漏れないようにする案をまとめた。
- バースト時のバックプレッシャとログ欠損防止のため、Cloudflare Queues + R2 を併用するフォールトトレランス案を提示。

### 意図・設計上の判断
- 高凝集: ログ加工・転送ロジックを Workers(Hono) 単体に集約し、各クライアントからは同一 API で扱えるようにする。
- 低結合: 既存 Go backend とはトークン発行と設定共有のみに限定し、本番稼働前でも既存システムへの影響を最小限に抑える。
- セキュリティ: GCP 資格情報を Workers Secret に閉じ込め、クライアントとは署名付きペイロードのみで連携することで鍵拡散を防止。

### 今後の TODO
- PoC 実装で Cloudflare Worker から Cloud Logging Write API への実送信を確認。
- Backend にトークン発行 API を追加し、Frontend/Unity SDK の利用方法をドキュメント化。
- 運用観点（レート制御、DLQ 監視、メトリクス連携）の詳細設計を詰める。

## 2025-11-10 Cloudflare Worker 実装着手

### 目的
- 設計メモで固めた要件をコード化し、Cloudflare Workers 上で動作する Hono アプリを `log_system/` 配下に構築する。
- Unity/Frontend からのログ受信、Queue 経由によるバッファ、Cloud Logging への直接書き込み、DLQ 再送の責務を高凝集にまとめる。

### 実装概要
- `log_system/` に Hono + TypeScript の骨格と `wrangler.toml` を作成。Queue/R2 バインディングや閾値設定を `vars` で管理。
- ルーティングは `src/routes` に分離し、`/v1/ingest`, `/v1/replay`, `/healthz` を追加。共通前処理で requestId を生成し、Logging/監査用に利用。
- `security/token.ts` で HMAC ベアラートークン検証を実装。秘密鍵は Cloudflare Secret から取得し、`log:write` / `log:replay` scope を判定。
- `services/log-normalizer.ts` で input スキーマ整形、`log-router.ts` で Queue or Cloud Logging or DLQ への振り分けを担当。GCP 書き込みは `services/gcp.ts` に集約し、Service Account JWT→Access Token キャッシュを内包。
- 失敗時は `services/dead-letter.ts` 経由で R2 へ JSON バッチを保存し、`/v1/replay` からキー指定で再送できるようにした。
- `npm run build` で wrangler の dry-run デプロイを実行できるよう `package.json` にビルドコマンドを追加し、CI でビルドエラー検知しやすくした。

### 意図・設計上の判断
- **高凝集**: トークン検証・正規化・ルーティング・GCP 書き込みをフォルダ単位で分離し、1 コンポーネント=1責務を徹底。
- **低結合**: `LogRouter` は Queue/Writer/DeadLetter を内部注入に留め、外部は `dispatch` のみ呼べば良い API に整理。
- **拡張性**: GCP サービスアカウント情報を JSON 文字列のまま env 受取にし、Workload Identity など別方式に差し替えやすいよう設計。

### 追記 (Queue コンシューマ)
- Cloudflare Queues からのログバッチを処理する `queue.batch` を同 Worker 内に実装。Cloud Logging への書き込み失敗時は R2 へ DLQ 退避し、DLQ 未設定時のみリトライさせる。
- `wrangler.toml` に `queues.consumers` を追加し、最大 50 件・5 秒バッチで処理するように設定。Miniflare や本番で一貫した挙動となるよう README も更新。

## 2025-11-10 Log Token API 実装

### 目的
- Cloudflare Worker で利用する `client_log_token` を Go backend で払い出し、Unity/Frontend から安全に取得できるようにする。

### 実装概要
- `internal/service/log_token.go` を追加し、HMAC-SHA256 署名・Base64URL エンコードで `payload.signature` 形式のトークンを生成。TTL, allow/default scopes を環境変数で制御可能にした。
- `internal/config/config.go` に `LOG_RELAY_*` 系の設定（シークレット・TTL・スコープ）を追加し、`cmd/server/main.go` でサービスを初期化。
- `POST /api/log-token` を新設し、ルーム存在チェック後に `client_id`/`viewer_id`/`platform` を含めたトークンを返却。レスポンスには `expires_at`, `ttl_seconds`, `scopes` を含めクライアントが再発行タイミングを判断できるようにした。
- 基本スコープは `log:write`、将来的な運用用に `LOG_RELAY_ALLOWED_SCOPES` で `log:replay` などを許可できるようにした。

### テスト
- `internal/service/log_token_test.go` を追加し、トークン生成時の署名形式・TTL・scope フィルタリングを検証。

## 2025-11-10 Queue 依存撤廃 (無料プラン対応)

### 背景
- Cloudflare Workers の無料プランでは Queues が利用できず、`wrangler` デプロイが 100129 エラーで失敗するため、Queue 経路を一旦外す。

### 変更
- `wrangler.toml` から Queue バインディング/コンシューマ設定を削除。Worker エントリ (`src/index.ts`) も `fetch` のみ export。
- `services/log-router.ts` をシンプル化し、Cloud Logging 直接書き込み＋失敗時の R2 (DLQ) 退避のみを実施する実装へ変更。`routes/health.ts` から `queueBound` を削除。
- README を更新し、無料枠では Queue を使わず運用する点・`DLQ_BUCKET` のみ必須である点を明記。

### 今後
- Queue の有無で挙動を切り替えられるよう feature flag 化する案を検討（有料プラン移行時に再導入）。

## 2025-11-10 Log Worker 利用手順ドキュメント化

- 外部クライアント向けに `log_system/README.md` に利用手順を追記。`/api/log-token` → `/v1/ingest` → `/v1/replay` の呼び方とリトライ推奨方針を明文化し、Unity/Frontend 実装者が自力で組み込めるようにした。

## 2025-11-10 Backend ログ構造化対応

- `pkg/logger` に Cloud Logging 互換の `slog` ハンドラを実装し、JSON で `severity`/`timestamp`/`message`/`fields` を出力するよう変更。サービス名などのメタも出せるよう `Config.Service` 等を追加し、デフォルトの `LOG_LEVEL` は `error` に落として INFO ログを抑制。
- API サーバ起動時 (`cmd/server/main.go`) に新ロガーを利用し、Echo のアクセスログを `middlewarex.StructuredLogger` に置き換え。Cloud Run → Logging でもレベルフィルタ可能になった。
- REST ハンドラ (`internal/handler/api.go`) に `*slog.Logger` を注入し、500 系など致命的ケースのみ `logger.Error` を吐くよう整理（4xx はロギングせず）。これでエラー原因がログに残りつつ、不要なレベルは出力しない。
- 付随して `pkg/logger/logger_test.go` を追加し、JSON フォーマットと severity マッピングを検証。

### 今後の TODO
- Backend のトークン発行 API と SDK 実装を接続し、エンドツーエンドで疎通確認を行う。
- Miniflare を用いた自動テスト、Queue コンシューマ Worker の雛形追加、Metrics エクスポートを整備。
