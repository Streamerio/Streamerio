// 古いコード

package handler

import (
	"fmt"
	"io"
	"net/http"

	"github.com/labstack/echo/v4"
	"golang.org/x/net/websocket"
)

func HandleWebSocket(c echo.Context) error {
	s := websocket.Server{
		Handshake: func(cfg *websocket.Config, r *http.Request) error {
			// 全オリジン許可（必要ならここで厳密にチェック）
			return nil
		},
		Handler: func(ws *websocket.Conn) {
			defer ws.Close()

			// 接続登録
			id := manager.Register(ws)
			defer manager.Unregister(id)

			// 接続直後に必ずログを出す
			c.Logger().Infof("Client connected: %s id=%s", c.Request().RemoteAddr, id)

			// 初回のメッセージを送信（IDを通知）
			if err := websocket.Message.Send(ws, fmt.Sprintf("your id=%s", id)); err != nil {
				c.Logger().Errorf("initial send failed: %v", err)
				return
			} else {
				c.Logger().Info("Greeting sent")
			}

			for {
				// Client からのメッセージを読み込む
				msg := ""
				err := websocket.Message.Receive(ws, &msg)
				if err != nil {
					if err == io.EOF {
						c.Logger().Infof("Client disconnected id=%s", id)
					} else {
						c.Logger().Errorf("receive failed: %v", err)
					}
					return
				}

				// 受け取ったメッセージをログ出力
				c.Logger().Infof("Received from client: %s", msg)

				// Client からのメッセージを元に返すメッセージを作成し送信する
				err = sendMessageToClient(ws, fmt.Sprintf("Server: id=%s \"%s\" received!", id, msg))
				if err != nil {
					c.Logger().Errorf("send failed: %v", err)
					return
				}
			}
		},
	}

	s.ServeHTTP(c.Response(), c.Request())
	return nil
}

func sendMessageToClient(ws *websocket.Conn, message string) error {
	return websocket.Message.Send(ws, message)
}

func SendAttackMessageToClient(c echo.Context) error {
	to := c.QueryParam("action")
	msg := "1"
	if err := manager.SendTo(to, msg); err != nil {
		return c.String(http.StatusNotFound, fmt.Sprintf("send failed: %v", err))
	}
	return c.String(http.StatusOK, "ATTACK OK")
}

func SendDefendMessageToClient(c echo.Context) error {
	to := c.QueryParam("action")
	msg := "DEFEND"
	if err := manager.SendTo(to, msg); err != nil {
		return c.String(http.StatusNotFound, fmt.Sprintf("send failed: %v", err))
	}
	return c.String(http.StatusOK, "DEFEND OK")
}

func ListClients(c echo.Context) error {
	ids := manager.ListIDs()
	return c.JSON(http.StatusOK, map[string]interface{}{"clients": ids})
}
