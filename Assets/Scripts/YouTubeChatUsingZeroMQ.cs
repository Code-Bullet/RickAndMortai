using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

// this script reads shit from chat, well it recieves messages from a python script that is reading chat
// it then populates a topic suggestions and  vote suggestions list 
public class YouTubeChatUsingZeroMQ : MonoBehaviour
{

    private int maxListSize = 1000;

    public List<string> topicSuggestions = new();

    public List<string> voteSuggestions = new();

    List<string> alreadyTakenTopics = new();

    List<string> wordBlacklist = new() { "knee", "nick", "faggot", "fagot", "nigga", "niga", "niger", "nigger", "nick g", "nick c", "meth", "911", "9/11", "9 11", "nine eleven", "september", "Homophobic", "Isis", "Muslim", "semitic", "Rape", "Retard", "Pedophile", "Pedophilia" };  // List of predefined words that topics cannot contain

    private bool connected = false;

    public bool usingYoutubeChatStuff = true;

    int timeSinceMessage = 0;

    void Start()
    {

        if (usingYoutubeChatStuff)
        {

            // Initialize NetMQ (required)
            AsyncIO.ForceDotNet.Force();
            PollChat();

            // CallPythonSocketCommunicationRepeatedly();
        }
    }
    private async Task PollChat()
    {
        while (true)
        {
            using (RequestSocket client = new())
            {
                // Connect to the Python server
                client.Connect("tcp://127.0.0.1:5555");

                // Send a message to Python to request chat
                client.SendFrame("get_chat");

                // Receive chat messages from Python
                string chatMessages = client.ReceiveFrameString();
                string[] messages = chatMessages.Split('\n');
                string[] messagesFiltered = Array.FindAll(messages, s => !string.IsNullOrEmpty(s));

                foreach (string message in messagesFiltered)
                {
                    Debug.Log(message);
                }

                for (int i = 0; i < messagesFiltered.Length - 1; i += 2)
                {

                    // messages are sent like: message content \n author name \n
                    // so get the author and message
                    string message = messages[i];
                    string author = messages[i + 1];

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        continue;
                    }

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
            }
            // Wait for a short moment before the next poll
            await Task.Delay(3000);
            // yield return new WaitForSeconds(2.0f);
        }
    }
    private void OnApplicationQuit()
    {
        // Terminate NetMQ (required)
        NetMQConfig.Cleanup();
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

