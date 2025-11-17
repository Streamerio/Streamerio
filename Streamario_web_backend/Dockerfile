# Goの公式イメージを使用
FROM golang:1.25

# 作業ディレクトリを設定
WORKDIR /app

# Goアプリケーションのソースコードをコピー（リポジトリ直下をコンテキストにしたビルドに対応）
COPY Streamario_web_backend/ .

# 依存関係を解決
RUN go mod tidy

# アプリケーションをビルド
RUN go build -o main cmd/server/main.go 

# コンテナが起動時に実行するコマンド
CMD ["./main"]

# ポート8888を開放
EXPOSE 8888