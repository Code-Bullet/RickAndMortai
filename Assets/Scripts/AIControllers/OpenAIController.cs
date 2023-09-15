using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using System.Threading.Tasks;
using System.Net.Http;
using Assets.Scripts.AIControllers;


// this script is used to get responses from chatgpt.
public class OpenAIController : AIController
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;

    [SerializeField]
    private string outputString;
    public override string OutputString
    {
        get
        {
            return outputString;
        }
        set
        {
            outputString = value;
        }
    }

    [SerializeField]
    private string[] outputLines;
    public override string[] OutputLines
    {
        get
        {
            return outputLines;
        }
        set
        {
            outputLines = value;
        }
    }

    [SerializeField]
    private SceneDirector director;
    public override SceneDirector Director
    {
        get
        {
            return director;
        }
        set
        {
            director = value;
        }
    }

    [SerializeField]
    private List<TextAsset> examples;
    public List<TextAsset> Examples
    {
        get
        {
            return examples;
        }
        set
        {
            examples = value;
        }
    }

    [SerializeField]
    private TextAsset systemMessage;
    public override TextAsset SystemMessage
    {
        get
        {
            return systemMessage;
        }
        set
        {
            systemMessage = value;
        }
    }

    public bool useChatGPT4 = false;
    public override void Init()
    {
        // this is the system message. its probably shit but it kinda works


        // This line gets your API key (and could be slightly different on Mac/Linux)
        api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User));

        string text = systemMessage.text;

        string nextLine = " Here are some example scripts:\n";

        foreach (TextAsset example in examples)
        {
            text += nextLine;
            text += example.text;
            nextLine = "\nHere is another example: \n";
        }

        // add the system message to the messages history.
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, text)
        };
    }

    // because we give chatgpt the entire history of messages it is important to clear it because there is a max message length.
    // atm this is cleared after every prompt
    public override void Clear()
    {
        string text = systemMessage.text;

        string nextLine = " Here are some example scripts:\n";

        foreach (TextAsset example in examples)
        {
            text += nextLine;
            text += example.text;
            nextLine = "\nHere is another example: \n";
        }

        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, text)
        };
    }

    // ok this is the actual interaction with chatgpt.
    public override async Task<string> EnterPromptAndGetResponse(string inputPrompt)
    {
        // Don't submit empty messages
        if (inputPrompt.Length < 1)
        {
            Debug.Log("message is empty");
            return null;
        }

        inputPrompt += ". Make sure to use light profanity like frick, shoot and crap. Scripts should have at least 30 lines of dialog.";

        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = inputPrompt;

        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        // Add the message to the list
        messages.Add(userMessage);

        // Retry logic
        int retryCount = 0;
        const int maxRetryCount = 3;
        while (retryCount < maxRetryCount)
        {
            try
            {
                // Send the entire chat to OpenAI to get the next message
                var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
                {

                    Model = useChatGPT4 ? Model.ChatGPT4_8k : Model.ChatGPTTurbo16k,
                    // Model = Model.ChatGPT4_8k,

                    Temperature = 0.6,
                    MaxTokens = 2000,
                    Messages = messages
                });

                // Get the response message and store it in a response message variable 
                ChatMessage responseMessage = new ChatMessage();
                responseMessage.Role = chatResult.Choices[0].Message.Role;
                responseMessage.Content = chatResult.Choices[0].Message.Content;
                Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));

                // Add the response to the list of messages
                messages.Add(responseMessage);

                // Exit the retry loop if the request is successful
                break;
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("429"))
                {
                    // Model overloaded, retry after a delay
                    retryCount++;
                    Debug.LogWarning("TooManyRequests error. Retrying in 1 second...");
                    await Task.Delay(1000); // Wait for 1 second before retrying
                }
                else
                {
                    // Other HTTP request error occurred, log the exception
                    Debug.LogError($"HTTP request error: {ex}");
                    Debug.Log("Retrying in 1 second...");
                    retryCount++;
                    await Task.Delay(1000);
                    // break;
                }
            }
            catch (Exception ex)
            {
                // Other exceptions occurred, log the exception
                Debug.LogError($"Error: {ex}");
                break;
            }



        }

        //return the message
        return messages[messages.Count - 1].Content;

    }

}