package main

import (
	"bytes"
	"encoding/json"
	"flag"
	"fmt"
	"log"
	"net/http"
	"sync"
	"time"
)

func main() {
	roomID := flag.String("room", "demo-room-1", "Room ID to join")
	count := flag.Int("count", 10, "Number of viewers to simulate")
	duration := flag.Duration("duration", 60*time.Second, "Duration to keep viewers active")
	baseURL := flag.String("url", "http://localhost:8888", "Backend URL")
	flag.Parse()

	log.Printf("Starting simulation: %d viewers in room %s for %v", *count, *roomID, *duration)

	var wg sync.WaitGroup
	for i := 0; i < *count; i++ {
		wg.Add(1)
		go func(id int) {
			defer wg.Done()
			viewerID := fmt.Sprintf("sim-user-%d", id)
			simulateViewer(*baseURL, *roomID, viewerID, *duration)
		}(i)
	}
	wg.Wait()
	log.Println("Simulation finished")
}

func simulateViewer(baseURL, roomID, viewerID string, duration time.Duration) {
	client := &http.Client{Timeout: 5 * time.Second}

	// Join immediately
	if err := joinRoom(client, baseURL, roomID, viewerID); err != nil {
		log.Printf("[%s] Join failed: %v", viewerID, err)
	} else {
		// log.Printf("[%s] Joined", viewerID) // Reduce noise
	}

	// Keep active by re-joining every 10 seconds
	// (Since we only update activity on Join or Button Push, and we don't want to mess up game stats,
	// calling Join repeatedly is a safe way to stay "active" without incrementing button counts)
	ticker := time.NewTicker(10 * time.Second)
	defer ticker.Stop()

	timeout := time.After(duration)

	for {
		select {
		case <-timeout:
			return
		case <-ticker.C:
			if err := joinRoom(client, baseURL, roomID, viewerID); err != nil {
				log.Printf("[%s] Keep-alive failed: %v", viewerID, err)
			}
		}
	}
}

func joinRoom(client *http.Client, baseURL, roomID, viewerID string) error {
	url := fmt.Sprintf("%s/api/rooms/%s/join", baseURL, roomID)
	body := map[string]string{"viewer_id": viewerID}
	jsonBody, _ := json.Marshal(body)

	resp, err := client.Post(url, "application/json", bytes.NewBuffer(jsonBody))
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return fmt.Errorf("status %d", resp.StatusCode)
	}
	return nil
}
