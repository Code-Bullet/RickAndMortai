﻿using OpenAI_API;
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
using OpenAI_API.Completions;


// this script is used to get responses from chatgpt, but using the text competion gpt-3.5-turbo-instruct model instead of chat one
public class OpenAIInstructController : AIController 
{
    private OpenAIAPI api;

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

    public bool useDavinci = false;
    public int maxTokens = 2000;
    public double temperature = 0.6;

    private string loadedSystemMessageWithExamples;

    public override void Init()
    {
        // This line gets your API key (and could be slightly different on Mac/Linux)
        string key = Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("OPEN AI KEY NOT FOUND");
            return;
        }
        api = new OpenAIAPI(key);

        string text = systemMessage.text;

        string nextLine = " Here are some example scripts:\n";

        foreach (TextAsset example in examples)
        {
            text += nextLine;
            text += example.text;
            nextLine = "\nHere is another example: \n";
        }

        // im putting this one for async function to get this text instead of recreating it every fucking time
        loadedSystemMessageWithExamples = text;
    }


    public override void Clear()
    {
        // this should not be required as you don't necessarily need to clear completion model
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

        string fullPrompt = loadedSystemMessageWithExamples + "\nScript topic: " + inputPrompt + "\n\n[Start of script]\n";

        CompletionRequest completionRequest = new CompletionRequest(
            prompt: fullPrompt,
            model: useDavinci ? Model.DavinciText002 : Model.GPTTurboInstruct,
            max_tokens: maxTokens,
            temperature: temperature
            ) ;

        Debug.Log($"GPT Instruct model used: {completionRequest.Model}, temp: {temperature}");

        // Retry logic
        int retryCount = 0;
        const int maxRetryCount = 3;

        string response = "";

        while (retryCount < maxRetryCount)
        {
            try
            {
                // Send the request to OpenAI
                var chatResult = await api.Completions.CreateCompletionAsync(completionRequest);


                // store response in a variable to return
                response = chatResult.ToString();

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
        return response;
    }
}