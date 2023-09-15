using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;

public class ZeroMQExample : MonoBehaviour
{
    private void Start()
    {
        // Initialize NetMQ (required)
        AsyncIO.ForceDotNet.Force();
        StartCoroutine(PollChat());
    }

    private IEnumerator PollChat()
    {
        while (true)
        {
            using (RequestSocket client = new RequestSocket())
            {
                // Connect to the Python server
                client.Connect("tcp://127.0.0.1:5555");

                // Send a message to Python to request chat
                client.SendFrame("get_chat");

                // Receive chat messages from Python
                string chatMessages = client.ReceiveFrameString();
                string[] messages = chatMessages.Split('\n');
                string[] messagesFiltered = System.Array.FindAll(messages, s => !string.IsNullOrEmpty(s));

                foreach (string message in messagesFiltered)
                {
                    Debug.Log(message);
                }

                // Debug.Log($"Received chat: {chatMessages}");
            }

            // Wait for a short moment before the next poll
            yield return new WaitForSeconds(2.0f);
        }
    }

    private void OnApplicationQuit()
    {
        // Terminate NetMQ (required)
        NetMQConfig.Cleanup();
    }
}
