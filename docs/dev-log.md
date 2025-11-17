# 開発ログ (game-end-handling)

- 2024-09-21 02:00 作業ブランチ `game-end-handling` を作成し、既存仕様と `docs/game_end_plan.md` を再確認。終了イベントと集計要件を把握し、命名統一済み (`skill*/enemy*`) を前提に進める方針を明文化。
- 2024-09-21 02:35 バックエンドにゲーム終了処理の骨格を実装。`GameSessionService` を追加して終了集計・Unity へのサマリー送信・ルーム状態管理を集約し、WebSocket ハンドラで `game_end` メッセージを捕捉するよう調整。終了後リクエストに応答するため、API へ viewer サマリー返却と結果取得エンドポイントを追加。
- 2024-09-21 03:05 フロントエンドに終了検知とリザルトページを追加。イベント送信後に `game_over` 応答を受け取った場合は viewer サマリーをセッション保存し、`/result/[roomId]` へ遷移して集計 API の結果と組み合わせて表示するようにした。
- 2024-09-21 03:25 視聴者端末識別用の `/get_viewer_id` エンドポイントを追加。ULID を払い出して `viewers` テーブルに保存し、Cookie に設定することで既存フロントの `fetchViewerId` が動作するようにした。
- 2024-09-21 03:45 視聴者名を管理する `viewers` テーブル拡張と `/api/viewers/set_name` を追加。フロントのヘッダーに表示名編集フォームを用意し、サーバ側で24文字までに正規化して永続化するよう対応。
- 2025-09-20 12:10 CORS/クッキー運用の見直し。Cloud Run(backend) と Vercel(frontend) 間で `credentials: 'include'` を使うため、Echo の CORS を allowlist（`FRONTEND_URL`）+ `AllowCredentials=true` に変更。`/get_viewer_id` の Cookie を `SameSite=None; Secure` に更新してクロスサイト送受信を許可。ローカルデフォルトの `FRONTEND_URL="*"` の場合は `AllowCredentials=false` とし、意図せずワイルドカード+資格情報が混在しないようガード。
- 2024-09-21 04:10 終了サマリーに視聴者名を含めるようバックエンドを拡張し、Unity への `game_end_summary` と REST レスポンス双方で表示名を送出。リザルト画面は名前を優先表示し、未設定の場合は ULID をフォールバックとして表示するよう更新。

 
<<<<<<< HEAD
=======
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

## 2025-10-27 イベントカウンターの閾値超過分保持機能実装

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

## 2025-11-27 WebSocket接続状況のReactiveProperty公開（R3自動監視版）

### 目的
- WebSocketの接続状況をReactivePropertyとして公開し、他のコンポーネントからリアクティブに監視できるようにする
- UIコンポーネントなどが接続状態の変化に応じて自動的に更新される仕組みを提供
- R3の`Observable.EveryUpdate()`を使って`_websocket.State`を自動監視し、手動更新を不要にする

### 実装概要
- `WebsocketManager`に`ReactiveProperty<bool> _isConnectedProp`を追加
- `ReadOnlyReactiveProperty<bool> IsConnectedProp`として公開（外部からの書き込みを防止）
- `Start()`メソッドでR3の`Observable.EveryUpdate()`を使用して`_websocket.State`を監視
- `_websocket.State == WebSocketState.Open`なら`true`、それ以外なら`false`をReactivePropertyに設定
- `DistinctUntilChanged()`で値が変化した時のみ更新（パフォーマンス最適化）
- 手動でのReactiveProperty更新処理を削除し、自動監視に一本化

### 変更ファイル
- `Streamerio_unity/Assets/0_Scripts/0_Common/Websocket/WebsocketManager.cs`
  - `ReactiveProperty<bool> _isConnectedProp`を追加
  - `ReadOnlyReactiveProperty<bool> IsConnectedProp`プロパティを追加
  - `Start()`メソッドで`Observable.EveryUpdate()`による自動監視を実装
  - 手動更新処理（`OnOpen`、`OnError`、`OnClose`、`DisconnectWebSocket`内）を削除

### 意図・設計上の判断
- 高凝集: WebSocket接続状態の管理を`WebsocketManager`内に集約し、ReactivePropertyも同一クラスで管理
- 低結合: `ReadOnlyReactiveProperty`として公開することで、外部からの直接的な変更を防止し、インターフェースを明確化
- リアクティブ: R3ライブラリの`Observable.EveryUpdate()`を使用して、接続状態を自動的に監視・更新
- 簡潔性: 手動更新処理を削除し、状態監視を一元化することでコードを簡潔に
- パフォーマンス: `DistinctUntilChanged()`により、値が変化した時のみ更新処理を実行

### 使用方法
```csharp
// 他のコンポーネントから使用例
WebsocketManager.Instance.IsConnectedProp.Subscribe(isConnected =>
{
    Debug.Log($"WebSocket接続状態: {isConnected}");
    // UI更新など
});
```

### 実装の詳細
- `Observable.EveryUpdate()`で毎フレーム`_websocket.State`をチェック
- `_websocket`が`null`の場合は`false`を返す
- `_websocket.State == WebSocketState.Open`の場合のみ`true`、それ以外は`false`
- `DistinctUntilChanged()`で前回の値と異なる場合のみ購読者に通知
- `.AddTo(this)`でGameObject破棄時に自動的に購読を解除

### 今後の課題
- 接続状態の変化をUIに反映するコンポーネントの実装
- 接続状態に応じた自動リトライ機能の実装（必要に応じて）

## 2025-12-XX WebSocketManagerにIDisposable実装とDisposeパターンの適用

### 目的
- `OnApplicationQuit`の処理を`IDisposable`の`Dispose`メソッドに移動し、リソース管理を標準的なパターンに統一
- アプリケーション終了時だけでなく、オブジェクト破棄時にも確実にリソースを解放できるようにする
- .NET標準のリソース管理パターンに準拠し、保守性を向上させる

### 実装概要
- `WebSocketManager`クラスに`IDisposable`インターフェースを実装
- `Dispose()`メソッドを追加し、`OnApplicationQuit`の処理（WebSocket切断）を移動
- `OnApplicationQuit`から`Dispose()`を呼び出すように変更
- `Dispose()`内では`DisconnectWebSocket().Forget()`を使用して非同期処理を実行

### 変更ファイル
- `Streamerio_unity/Assets/0_Scripts/0_Common/Websocket/WebsocketManager.cs`
  - `IDisposable`インターフェースを実装
  - `Dispose()`メソッドを追加
  - `OnApplicationQuit()`を`Dispose()`を呼び出すように変更

### 意図・設計上の判断
- 標準パターン: .NET標準の`IDisposable`パターンに準拠し、リソース管理を明確化
- 高凝集: WebSocket切断処理を`Dispose()`メソッドに集約し、リソース解放の責務を明確化
- 低結合: `OnApplicationQuit`は`Dispose()`を呼び出すだけの薄いラッパーにし、実装の詳細を`Dispose()`に集約
- 非同期処理: `Dispose()`は同期的なメソッドだが、`Forget()`を使用して非同期処理を実行（Unityの`OnApplicationQuit`では同期的に待つことが推奨されていないため）

### 実装の詳細
- `Dispose()`メソッド内で`DisconnectWebSocket().Forget()`を呼び出し、WebSocket切断を非同期で実行
- エラーハンドリングを`try-catch`で実装し、エラー発生時もアプリケーション終了を妨げない
- `OnApplicationQuit`は`async void`から`void`に変更し、`Dispose()`を同期的に呼び出す

### 今後の課題
- `OnDestroy`でも`Dispose()`を呼び出すようにすることで、オブジェクト破棄時にも確実にリソースを解放する（必要に応じて）

## 2025-12-XX baseMessage.typeをEnum化

### 目的
- `baseMessage.type`を文字列からEnumに変更し、型安全性を向上させる
- 文字列比較によるタイプミスを防ぎ、IDEの補完機能を活用できるようにする
- メッセージタイプの管理を一元化し、保守性を向上させる

### 実装概要
- `MessageType` Enumを新規作成（`room_created`, `game_event`, `game_end_summary`）
- `BaseMessage`, `RoomCreatedNotification`, `GameEventNotification`の`type`フィールドは文字列のまま維持（UnityのJsonUtilityの制約のため）
- JSONから文字列として読み取った後、`Enum.Parse`で`MessageType`に変換
- 文字列比較（`baseMessage.type == "room_created"`）をEnum比較（`messageType == MessageType.room_created`）に変更

### 変更ファイル
- `Streamerio_unity/Assets/0_Scripts/0_Common/Websocket/WebsocketManager.cs`
  - `MessageType` Enumを追加
  - `ReceiveWebSocketMessage`メソッドで文字列からEnumへの変換処理を追加
  - 文字列比較をEnum比較に変更

### 意図・設計上の判断
- 型安全性: Enumを使用することで、コンパイル時にタイプミスを検出可能
- 保守性: メッセージタイプの追加・変更時にEnumを更新するだけで、使用箇所をIDEで一括検索可能
- 互換性: UnityのJsonUtilityはEnumを直接文字列として扱えないため、文字列として読み取った後でEnumに変換する方式を採用
- エラーハンドリング: `Enum.Parse`で変換失敗時に適切なエラーログを出力し、デバッグを容易にする

### 実装の詳細
- JSONから文字列として`baseMessage.type`を読み取り
- `System.Enum.Parse(typeof(MessageType), baseMessage.type, true)`でEnumに変換（大文字小文字を無視）
- 変換に失敗した場合はエラーログを出力して処理を中断
- 変換後の`MessageType`を使用して分岐処理を実行

### 今後の課題
- 新しいメッセージタイプが追加された場合は、`MessageType` Enumにも追加が必要
- UnityのJsonUtilityの代わりに、より柔軟なJSONライブラリ（例: Newtonsoft.Json）の導入を検討

## 2025-12-XX SkillRandomActivatorへの依存性注入設定

### 目的
- `SkillRandomActivator`に`GlobalLifetimeScope`で登録された`IWebSocketManager`のインスタンスを注入できるようにする
- VContainerの依存性注入システムを活用し、コンポーネント間の結合を緩和する
- `[Inject]`アトリビュートを使用した依存性注入を正しく動作させる

### 実装概要
- `InGameLifetimeScope`の`Configure`メソッドに`SkillRandomActivator`の登録を追加
- `RegisterComponentInHierarchy<SkillRandomActivator>()`を使用して、シーン内の`SkillRandomActivator`コンポーネントをVContainerに登録
- `InGameLifetimeScope`は`base.Configure(builder)`を呼び出しているため、親スコープ（`GlobalLifetimeScope`）で登録された`IWebSocketManager`が利用可能

### 変更ファイル
- `Streamerio_unity/Assets/0_Scripts/1_InGame/InGameLifetimeScope.cs`
  - `RegisterComponentInHierarchy<SkillRandomActivator>()`を追加

### 意図・設計上の判断
- 高凝集: 依存性注入の設定を`LifetimeScope`に集約し、コンポーネントの登録を一元管理
- 低結合: `SkillRandomActivator`は`IWebSocketManager`インターフェースに依存し、具体的な実装クラス（`WebSocketManager`）に直接依存しない
- 依存性注入: VContainerの標準的なパターン（`RegisterComponentInHierarchy`）を使用し、`[Inject]`アトリビュートによる自動注入を実現
- スコープ継承: `InGameLifetimeScope`が`GlobalLifetimeScope`の子スコープとして動作し、親スコープで登録されたシングルトンインスタンスを利用可能

### 実装の詳細
- `SkillRandomActivator`は`[Inject]`アトリビュートが付いた`Construct`メソッドを持っている
- `RegisterComponentInHierarchy`により、シーン内の`SkillRandomActivator`コンポーネントがVContainerに登録される
- VContainerが自動的に`Construct`メソッドを呼び出し、`GlobalLifetimeScope`で登録された`IWebSocketManager`のインスタンスを注入する
- `GlobalLifetimeScope`で`IWebSocketManager`は`Lifetime.Singleton`として登録されているため、同じインスタンスが注入される

### 使用方法
- `SkillRandomActivator`コンポーネントをシーンに配置する
- `InGameLifetimeScope`が存在するシーンで、VContainerが自動的に依存性を注入する
- `SkillRandomActivator`の`Construct`メソッドが呼び出され、`_webSocketManager`フィールドに`IWebSocketManager`のインスタンスが設定される

### 今後の課題
- 他の`MonoBehaviour`コンポーネントでも同様の依存性注入が必要な場合は、適切な`LifetimeScope`で登録する必要がある
- `EnemyRandomActivator`など、同様のパターンを持つコンポーネントも確認し、必要に応じて登録を追加する
>>>>>>> game-di
