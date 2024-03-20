// const CHANNEL_ID = "UCGtZoW-xDId5E7OtnF0WgBA" // llama69's channel
const CHANNEL_ID = "UCgzg_f5HC6EnY-9q5Px1Q-g" // This is code bullets day off channel
// const CHANNEL_ID = "UCq6VFHwMzcMXbuKyG7SQYIg" // The channel id with the livestream for automated detection
const LIVESTREAM_ID = "" // If you want to supply a specific id, that works, too. Otherwise leave this empty and let the automated detection work
const UPDATE_RATE = 5 // How many times to read new chat per second
const MAX_PER_UPDATE = 100 // The max it will send in one patch
const PORT = 9999 // ...

const puppeteer = require("puppeteer")

// Goes up x parent nodes from node
async function up(node, x) {
  if (x <= 0)
    return node

  return up(await node.getProperty("parentElement"), x - 1)
}

// Gets the author and content from a node
async function getAuthorAndContents(message) {
  if (!message) return null
  let author = await (await message.waitForSelector("#author-name")).evaluate(e => e.textContent)
  let text = await (await message.waitForSelector("#message")).evaluate(e => e.textContent)
  return { author: author.trim().replace("\n", ""), text: text.trim().replace("\n", "") }
}

// Main entry point
(async () => {
  // Initialize puppeteer
  const browser = await puppeteer.launch({ headless: "new" })
  const page = await browser.newPage()
  let streamId = LIVESTREAM_ID

  if (streamId == "") {
    // Get the first live video on the provided channel id
    console.log(`Navigating to channel: https://youtube.com/channel/${CHANNEL_ID}/streams`)
    await page.goto(`https://youtube.com/channel/${CHANNEL_ID}/streams`)
    streamId = (await (await (await up(await page.waitForSelector('[aria-label="LIVE"]'), 4)).getProperty("href")).jsonValue()).split("v=")[1]
    console.log(`Found stream id: ${streamId}`)
    console.log("Popping out chat")
  } else {
    console.log(`Using input stream id: ${streamId}`)
  }

  // Open the chat in popout format
  await page.goto(`https://www.youtube.com/live_chat?is_popout=1&v=${streamId}`)
  
  // Get the chat window where all messages are
  const chatWindow = await page.waitForSelector("#chat > #item-list > #live-chat-item-list-panel > #contents > #item-scroller > #item-offset > #items")
  console.log("Chat window obtained. Ready...")

  let lastElement = null

  // Main loop
  setInterval(async () => {
    try {
      let toSend = []
      if (!lastElement) {
        // Get the first chat message we can find
        lastElement = await chatWindow.$("yt-live-chat-text-message-renderer")
        if (!lastElement) { // If there are no chat messages yet, just ignore...
          return
        }

        let authorAndContents = await getAuthorAndContents(lastElement)

        if (authorAndContents)
          toSend.push(authorAndContents)
      }

      for (let i = 0; i < MAX_PER_UPDATE - toSend.length; i++) {
        // From last element, get next sibling, essentially going forward one message
        let nextElement = await lastElement.getProperty("nextElementSibling")
        if (nextElement == null || nextElement.handle == null) break
        lastElement = nextElement

        // Ensure the element is a message before adding it to the array
        if (await (await nextElement.getProperty("tagName")).jsonValue() != "YT-LIVE-CHAT-TEXT-MESSAGE-RENDERER")
          continue
        
        let authorAndContents = await getAuthorAndContents(lastElement)

        if (authorAndContents)
          toSend.push(authorAndContents)
      }

      toSend.map(e => console.log(`${e.author}: ${e.text}`))

      // If we have anything to send, then send it on the specified port
      if (toSend.length > 0) {
        fetch(`http://localhost:${PORT}`, {
          method: "POST",
          body: JSON.stringify(toSend),
          headers: {
            "Content-Type": "application/json"
          }
        }).catch(console.error)
      }
    } catch (e) {
      console.error(e)
    }

  }, 1000 / UPDATE_RATE)
})()