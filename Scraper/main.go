package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"strings"
	"time"

	"github.com/go-rod/rod"
)

var channelID string = "@ChannelHere" // Livestream can be automatically pulled using this.
var streamID string = ""              // Alt, you can feed the streamID directly here.
var chatUpdateRate int = 5            // Howmany times per second the chat should be read.
var chatUpdateMax int = 100           // Maximum messages should be sent to server for processing at once.
var port int = 9999                   // Port to send chat messages via http.

type Message struct { // Struct to store message data before JSON marshalling.
	Author string `json:"author"`
	Text   string `json:"text"`
}

func messageBuilder(element *rod.Element) Message { // Constructs message structs.
	message := Message{} // Creates new message struct
	message.Author = element.MustElement("#author-name").MustText()
	message.Text = element.MustElement("#message").MustText()
	return message
}

func main() { // The program, lmao.
	browser := rod.New().MustConnect() // Spins up the browser.
	if streamID == "" {                // If stream not directly provided, will attempt to grab it.
		fmt.Println("[INFO] No stream provided, attempting grab from channel")
		fmt.Println("[INFO] Waiting for ToS")
		page := browser.MustPage(fmt.Sprintf("https://www.youtube.com/%s/streams", channelID)) // Navigates to live page of yt channel ID provided.
		page.MustElement("#yDmH0d > c-wiz > div > div > div > div.NIoIEf > div.G4njw > div.qqtRac > div.VtwTSb > form:nth-child(3) > div > div > button").
			MustClick() // Agrees to yts TOS.
		fmt.Println("[INFO] Searching for livestream")
		page.MustElement("ytd-thumbnail-overlay-time-status-renderer.ytd-thumbnail > div:nth-child(2) > span:nth-child(2)").
			MustClick() // Opens the first active live.
		fmt.Println("[INFO] Getting stream ID")
		streamID = strings.Split(page.MustInfo().URL, "v=")[1] //Gets ID from URL.
		page.Close()
	}
	page := browser.MustPage(fmt.Sprintf("https://www.youtube.com/live_chat?is_popout=1&v=%s", streamID))                            // Opens the livechat.
	chat := page.MustElement("#chat > #item-list > #live-chat-item-list-panel > #contents > #item-scroller > #item-offset > #items") // Grabs actual chat element.
	fmt.Println("[INFO] Chat window obtained")

	var lastElement *rod.Element                                          // Creates var to store elements via rod.
	ticker := time.NewTicker(time.Second / time.Duration(chatUpdateRate)) // Creates ticker to pulse for duration specified.
	for range ticker.C {                                                  // Basically just goes when the ticker does.
		toSend := []Message{}   // Slice of message structs to send.
		if lastElement == nil { //If there's no prev element, it'll grab the first one and add it.
			lastElement = chat.MustElement("yt-live-chat-text-message-renderer")
			if lastElement == nil {
				return
			}
			message := messageBuilder(lastElement)
			toSend = append(toSend, message)
		}

		for i := 0; i < chatUpdateMax-len(toSend); i++ { // Will run while there are messages to send in limit.
			nextElement, err := lastElement.Next() // Selects next element.
			if err != nil {                        // Error checking needed, trust.
				break
			}
			if nextElement.MustProperty("tagName").String() == "YT-LIVE-CHAT-TEXT-MESSAGE-RENDERER" { // Checks if element is a chat message.
				message := messageBuilder(nextElement) // Builds and adds it.
				toSend = append(toSend, message)
			}
			lastElement = nextElement // Swapsies. :3
		}

		if len(toSend) > 0 { // If there are messages to send..
			toSendJSON, err := json.Marshal(toSend) // Turns struct into JSON bytes.
			if err != nil {
				panic(err) // This would be bad.
			}

			req, err := http.NewRequest( // Creates POST req, sends JSON as bytes.
				http.MethodPost,
				fmt.Sprintf("http://localhost:%v", port),
				bytes.NewBuffer(toSendJSON))
			if err != nil {
				panic(err) // This would be bad.
			}
			req.Header.Set("Content-Type", "application/json") // Declares itself.

			client := &http.Client{}   // New http client.
			res, err := client.Do(req) // Sends it.
			if err != nil {
				panic(err)
			}
			defer res.Body.Close()
			fmt.Printf("[EVENT] Sent %v chat messages\n", len(toSend))
		}
	}
}
