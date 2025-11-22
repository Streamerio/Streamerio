# 処理フロー（配信者サイド / 視聴者側）

本ドキュメントは、配信者サイドと視聴者側の処理の流れを、Mermaid のシーケンス図で示します。実装は `AGENTS.md` と `docs/game_end_plan.md` の方針に沿い、高凝集・低結合を意識した役割分離（REST/API と WebSocket、DB、Redis、Pub/Sub）を前提としています。

---

## 配信者サイド

```mermaid
sequenceDiagram
	autonumber
	actor Streamer as 配信者ゲーム(Unity)
	participant WS as WebSocketサーバ
	participant API as HTTPサーバ(API)
	participant DB as PostgresDB
	participant Redis as Redis
	participant PubSub as Pub/Sub

	%% 1-4: ゲーム開始とルーム確立
	Streamer->>WS: 接続（Handshake）
	WS-->>Streamer: roomId払い出し
	Note over Streamer: 画面にQR表示（roomId を含むURL）
	Streamer->>Streamer: ゲーム開始

	%% 5: 進行中のイベント反映（概要）
	rect rgb(245,245,245)
	Note over WS,Streamer: ゲーム進行中（視聴者イベント反映）
	Note over API: 視聴者Webが1.2秒ごとに<br/>集計済み回数を送信
	loop 集計済み回数の受信ごと
		API->>Redis: イベント数を加算（INCRBY）
		API->>DB: イベントを記録（viewerId/種別/回数）
		API->>API: 閾値判定（受信毎・超過分保持）
		alt 閾値超過
			API->>PubSub: publish(roomId, event)
			PubSub-->>WS: push(event)
			WS-->>Streamer: event（敵出現/スキル攻撃）
		else 閾値未満
			API-->>API: 継続
		end
	end
	end

	%% 6-9: クリア～終了サマリ表示
	alt クリア（ゲーム終了）
		Streamer->>WS: 終了通知（roomId）
		WS->>API: 終了を伝達（roomId）
		API->>DB: サマリ集計を取得
		API-->>Streamer: サマリ（最多押下者など）
		Streamer->>Streamer: 結果表示
	end
```

---

## 視聴者側

```mermaid
sequenceDiagram
	autonumber
	participant Viewer as 視聴者ブラウザ
	participant API as HTTPサーバ(API)
	participant Redis as Redis
	participant DB as PostgresDB
	participant PubSub as Pub/Sub
	participant WS as WebSocketサーバ
	participant Streamer as 配信者ゲーム(Unity)

	%% 1-2: 参加とID払い出し
	Note over Viewer: 配信画面のQRを読み取り（roomId付きURLを開く）
	Viewer->>API: GET /get_viewer_id（viewerId発行依頼）
	API-->>Viewer: viewerId を払い出し（Cookie/レスポンス）

	%% 3-7: イベント送信～しきい値判定～ゲーム状態監視
	loop 1.2秒ごとの集計サイクル（クライアント内で集計→送信）
		Viewer->>Viewer: ボタン連打をクライアント内でカウント
		Viewer->>API: POST /events {viewerId, roomId, 種別, 回数}
		API->>Redis: INCRBY（イベント数を加算）
		API->>DB: イベントを記録（viewerIdとともに永続化）
		API->>API: 閾値判定（受信毎・超過分保持）
		alt しきい値超過
			API->>PubSub: publish(roomId, event)
			PubSub-->>WS: push(event)
			WS-->>Streamer: event（敵出現/スキル攻撃）
		end
		API->>DB: ゲーム状態を取得（roomId）
		alt ゲーム終了
			API->>DB: サマリ集計（個人/全体）
			API-->>Viewer: 終了通知 + サマリ返却
			Note over Viewer,API: 以降の送信は停止（ループ終了を表現）
		else 継続
			API-->>Viewer: 200 OK（受付）
		end
	end

	Viewer->>Viewer: 終了
```

---

### 説明と前提
- 配信者ゲーム（Unity）は WebSocket サーバへ接続して `roomId` を取得し、その `roomId` を含むURLのQRを表示します。
- 視聴者は QR から Web サイトへ遷移し、`/get_viewer_id` で `viewerId` を払い出し。以降は 1.2 秒ごとに連打数を API に送信します。
- API は Redis にイベント数を蓄積し、しきい値超過時に Pub/Sub を通じて WS サーバへ通知。WS サーバは Unity へイベントを送出します。
- イベントは PostgresDB にも永続化され、終了時には API が DB からサマリを集計して配信者（Unity）と視聴者へ返します。
- 実装上は REST(API) と WebSocket サーバを別プロセス（別デプロイ）に分離可能とし、負荷に応じたスケールを容易にしています。

