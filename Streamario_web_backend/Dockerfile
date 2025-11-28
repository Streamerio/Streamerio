# ビルドステージ
FROM golang:alpine AS builder

# 作業ディレクトリを設定
WORKDIR /app

# 依存関係のみ先にコピーしてキャッシュを活用
COPY go.mod go.sum ./
RUN go mod download

# ソースコードをコピー
COPY . .

# アプリケーションをビルド(静的リンク)
RUN CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -o main cmd/server/main.go 

# 実行ステージ
FROM gcr.io/distroless/base-debian12

WORKDIR /root/

# ビルドステージからバイナリをコピー
COPY --from=builder /app/main .

# ポート8888を開放
EXPOSE 8888

CMD ["./main"]