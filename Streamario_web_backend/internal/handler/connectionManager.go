// 古いコード

package handler

import (
	"errors"
	"sort"
	"sync"
	"strconv"

	"golang.org/x/net/websocket"
)


type ClientConn struct {
	id      string
	ws      *websocket.Conn
	writeMu sync.Mutex
}

func (c *ClientConn) send(message string) error {
	c.writeMu.Lock()
	defer c.writeMu.Unlock()
	return websocket.Message.Send(c.ws, message)
}



type ConnectionManager struct {
	mu       sync.RWMutex
	Id   int
	idToConn map[string]*ClientConn
}

func NewConnectionManager() *ConnectionManager {
	return &ConnectionManager{
		idToConn: make(map[string]*ClientConn),
	}
}

func (m *ConnectionManager) Register(ws *websocket.Conn) string {
	idNum := m.Id + 1
	id := "client-" + strconv.Itoa(idNum)
	client := &ClientConn{id: id, ws: ws}
	m.mu.Lock()
	m.idToConn[id] = client
	m.mu.Unlock()
	return id
}

func (m *ConnectionManager) Unregister(id string) {
	m.mu.Lock()
	delete(m.idToConn, id)
	m.mu.Unlock()
}

func (m *ConnectionManager) SendTo(id string, message string) error {
	m.mu.RLock()
	client := m.idToConn[id]
	m.mu.RUnlock()
	if client == nil {
		return errors.New("connection not found")
	}
	return client.send(message)
}

func (m *ConnectionManager) ListIDs() []string {
	m.mu.RLock()
	ids := make([]string, 0, len(m.idToConn))
	for id := range m.idToConn {
		ids = append(ids, id)
	}
	m.mu.RUnlock()
	sort.Strings(ids)
	return ids
}


var manager = NewConnectionManager()
