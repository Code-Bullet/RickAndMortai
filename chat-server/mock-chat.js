const LIVESTREAM_ID = "" // If you want to supply a specific id, that works, too. Otherwise leave this empty and let the automated detection work
const UPDATE_RATE = 5 // How many times to read new chat per second
const MAX_PER_UPDATE = 100 // The max it will send in one patch
const PORT = 9999 // ...


class MessageGenerator {
    constructor() {
        this.i = 0
        this.usernames = [
            "satoshi",
            "terrydavis",
            "liamzebedee",
            "codebullet",
            "llama69"
        ]
        this.messageTemplates = {
            topicVote: [
                "vote:1",
                "vote:2",
                "vote:3",
                "vote:4"
            ],
            charVote: [
                "char:1",
                "char:2",
                "char:3",
                "char:4"
            ],
            rating: [
                "pog",
                "boo"
            ],
            random: [
                "lol",
                "lmao"
            ]
        }
    }

    generate(typ) {
        let i = Math.floor(Math.random() * 100)
        let author = this.usernames[i % this.usernames.length]
        let text = this.messageTemplates[typ][i % this.messageTemplates[typ].length]
        return { author, text }
    }
}

// Main entry point
(async () => {
    let msgGen = new MessageGenerator()

    // Main loop
    setInterval(async () => {
        // Test: random reaction.
        let msgs = Array(3).fill(0).map(() => msgGen.generate("rating"))
        
        // Test: 100% based reaction.
        // const msgs = Array(3).fill(0).map(() => ({
        //     author: "satoshi",
        //     text: "pog"
        // }))

        console.log(msgs)

        fetch(`http://localhost:${PORT}`, {
            method: "POST",
            body: JSON.stringify(msgs),
            headers: {
                "Content-Type": "application/json"
            }
        }).catch(console.error)

    }, 1000)
})()