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
