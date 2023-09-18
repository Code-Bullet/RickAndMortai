// UCgzg_f5HC6EnY-9q5Px1Q-g - Code Bullet's Day Off
// dG3Ul7OwF_s - Example stream, Alan Becker's 24/7 Livestream. Has constant messages.

const CHANNEL_ID = "UCgzg_f5HC6EnY-9q5Px1Q-g" // The channel id with the livestream for automated detection.
const LIVESTREAM_ID = "" // Specific livestream ID (youtube.com/watch?v=id)
const DEBUG = false // Log messages that are being sent
const PORT = 9999 // Port to send messages to

const { LiveChat } = require("youtube-chat");

// Gets the author and content from a node
async function getAuthorAndContents(message) {
  if (!message) return null
  let author = message.author.name || "User"
  let text = message.message[0].text || "Not Found"
  return { author, text }
}

// Main entry point
(async () => {
  // Initialize LiveChat (youtube-chat)
  
  let liveChat;
  if(CHANNEL_ID){ // If a channel ID is specified, start it with the channel ID
    liveChat = new LiveChat({
      channelId: CHANNEL_ID
    })
  }else{ // If not, fallback to livestream ID.
    liveChat = new LiveChat({
      liveId: LIVESTREAM_ID
    })
  }

  // The above code may seem messy, but youtube-chat won't accept undefined or null.

  liveChat.on('start', (liveId) => { // Signal that it has connected to the stream
    console.log(`Started with livestream ID: ${liveId}`)
  })

  liveChat.on('end', (reason) => { // No idea why this might happen, it's in their documentation.
    console.error(`Ended unexpectedly: ${reason}`)
  })

  liveChat.on('chat', async (comment) => { // When a chat message is recieved
    try{
      let authorAndContents = await getAuthorAndContents(comment) // Get the author and contents in the format that the stream expects

      if(DEBUG) console.log(authorAndContents)
      
      fetch(`http://localhost:${PORT}`, { // Send it off to localhost:PORT
        method: "POST",
        body: JSON.stringify([authorAndContents]), // Stringify it in an array, so that the stream has the correct format.
        headers: {
          "Content-Type": "application/json"
        }
      }).catch((error)=>{ // Helpful error message, sort of.
        console.log(`!!! Make sure to have port ${PORT} open !!!`)
        console.error(error)
      })
    }catch(error){
      console.error(error)
    }
  })

  liveChat.on('error', console.error) // Most likely stream not found

  await liveChat.start() // Start the chat listener!
})()
