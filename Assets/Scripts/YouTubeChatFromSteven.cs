using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.IO;

// this is from steven4547466 on discord, hes a legend.

// this script reads shit from chat, well it recieves messages from a python script that is reading chat
// it then populates a topic suggestions and  vote suggestions list 
public class YouTubeChatFromSteven : MonoBehaviour
{

    public class ChatMessage
    {
        public string author { get; set; }
        public string text { get; set; }

        public ChatMessage(string author, string text)
        {
            this.author = author;
            this.text = text;
        }

        public ChatMessage() { }

        public override string ToString()
        {
            return $"Author: {author} | Message: {text}";
        }
    }

    private int maxListSize = 1000;

    public List<string> topicSuggestions = new();

    public List<string> voteSuggestions = new();

    List<string> alreadyTakenTopics = new();

    List<string> wordBlacklist = new() { "faggot", "fagot", "nigga", "niga", "niger", "nigger", "nick g", "nick c", "meth", "911", "9/11", "9 11", "nine eleven", "Homophobic", "Isis", "Muslim", "semitic", "Rape", "Retard", "Pedophile", "Pedophilia" };  // List of predefined words that topics cannot contain

    private bool connected = false;

    public HttpListener listener;
    public int port = 9999;
    public bool usingYoutubeChatStuff = true;
    void Start()
    {
        if (usingYoutubeChatStuff)
        {
            Server();
        }
    }

    public static byte[] responseBuffer = Encoding.UTF8.GetBytes("OK");
    public void Server()
    {
        listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        Task.Run(() =>
        {
            while (listener.IsListening)
            {
                HttpListenerContext ctx = listener.GetContext();

                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                List<ChatMessage> messages = JsonConvert.DeserializeObject<List<ChatMessage>>(new StreamReader(req.InputStream).ReadToEnd());

                foreach (ChatMessage chatMessage in messages)
                {
                    string message = chatMessage.text;
                    string author = chatMessage.author;

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        continue;
                    }

                    Debug.Log("recieved: " + message );

                    if (message.ToLower().StartsWith("topic:"))
                    {

                        string topicMessage = message.Substring("topic:".Length).Trim();

                        // Check if the topicMessage contains any word from the wordBlacklist
                        bool containsBlacklistedWord = wordBlacklist.Any(blackWord => topicMessage.ToLower().Contains(blackWord.ToLower()));
                        bool haveAlreadyDoneTopic = alreadyTakenTopics.Contains(topicMessage);
                        // Check if the message after "topic:" is not empty or only spaces
                        if (!string.IsNullOrWhiteSpace(topicMessage) && !haveAlreadyDoneTopic && !containsBlacklistedWord)
                        {

                            // Add new message text to message texts
                            topicSuggestions.Add(topicMessage + "\n" + author);

                            Debug.Log(topicMessage + "\n" + author);

                            // Limit the size of messageTexts
                            if (topicSuggestions.Count > maxListSize)
                            {
                                // Remove oldest message text
                                topicSuggestions.RemoveAt(0);
                            }
                        }
                    }
                    else if (message.ToLower().StartsWith("vote:"))
                    {

                        string voteMessage = message.Substring("vote:".Length).Trim();

                        // Check if the message after "topic:" is not empty or only spaces
                        if (!string.IsNullOrWhiteSpace(voteMessage))
                        {
                            // Add new message text to message texts
                            voteSuggestions.Add(voteMessage);
                        }
                    }
                }

                resp.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
                resp.Close();
            }

            listener.Stop();
        });
    }

    // adds a topic to the already taken topics list
    public void AddToBlacklist(string topic)
    {
        if (!string.IsNullOrWhiteSpace(topic) && !alreadyTakenTopics.Contains(topic))
        {
            alreadyTakenTopics.Add(topic);
        }
    }

    //read it bitch
    public void ClearVotes()
    {
        voteSuggestions = new List<string>();
    }

    // counts votes and retuns a int array, which will look like [5,12,123] this means 5 votes for topic 1 ect.
    public int[] CountVotes()
    {
        int[] counts = new int[3];  // Array to hold the counts for "1", "2", "3"

        foreach (string vote in voteSuggestions)
        {
            string cleanedVote = vote.Trim();  // Remove leading and trailing white spaces

            if (cleanedVote == "1")
            {
                counts[0]++;
            }
            else if (cleanedVote == "2")
            {
                counts[1]++;
            }
            else if (cleanedVote == "3")
            {
                counts[2]++;
            }
        }

        return counts;
    }

    //returns a list of 3 random topics
    public List<string> GetRandomTopics()
    {
        int n = 3;
        List<string> randomTopics = new();

        if (topicSuggestions.Count < n)
        {
            Debug.LogError("Not enough topics to select from.");
            return null;  // Return empty list
        }

        for (int i = 0; i < n; i++)
        {

            int randomIndex = UnityEngine.Random.Range(0, topicSuggestions.Count);
            string selectedTopic = topicSuggestions[randomIndex];

            // Ensure that the topic has not already been chosen
            if (!randomTopics.Contains(selectedTopic))
            {
                randomTopics.Add(selectedTopic);
                alreadyTakenTopics.Add(selectedTopic.Split("\n")[0]);// add just the message to the already taken topics
                topicSuggestions.RemoveAt(randomIndex);
            }
            else
            {
                // Since the topic was already selected, decrement the loop counter to re-try the random selection
                topicSuggestions.RemoveAt(randomIndex);
                i--;
            }

            // if no topics are left then fuck me i guess. 
            if (topicSuggestions.Count <= 0)
            {
                return null;
            }
        }

        return randomTopics;
    }
}

