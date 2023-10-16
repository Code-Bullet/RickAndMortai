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
using Unity.VisualScripting;


// this script is used to get responses from chatgpt.
public class OpenAISlurDetector : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;

    private string systemMessage = "";

    private bool isEnabled = true;

    void Start()
    {
        // this is the system message. its probably shit but it kinda works
        systemMessage += "I am making ai generated Rick and morty episodes, where the topics that rick and morty are talking about are chosen by a live youtube chat. ";
        systemMessage += "A different chatgpt agent takes this topic and generates a script. However some of these scripts contain slurs that i dont want,";
        systemMessage += "mainly the N word and the F slur not Fuck the other one for gay people. Sometimes youtube chat is sneaky and will ask for things";
        systemMessage += " like \"Rick and Morty talk about their friend Nick Ger\" which when said outloud sounds like the N word. Some other examples of things";
        systemMessage += " that should be replaced are: \"niger\", \"nigeria\", \"nidder\", saying \"ger ni\" multiple times, saying \"gar nick\" multiple times, \"niga\", \"N I G G E R\", \"N-I-G-G-E-R\". ";
        // systemMessage += " I will give you the script and can you replace every occurance of a potenial slur with the word nope. Dont change any of the formatting ";
        // systemMessage += " of the input. if there is no slur just reply with \"no slurs detected\".";      
        systemMessage += " Swear words like fuck, Frick, Crap and shit are fine, only the 2  slurs should change.";
        systemMessage += " I will give you the script and if you dont detect any slurs respond with: [no slurs detected] if slurs are ";
        systemMessage += " detected repsond with the word that should be replaced. e.g. respond: [niger]. if multiple different slurs are present respond with each one ";
        systemMessage += " in square brakets e.g. [niger][N I D D E R][gar ni].";


        // systemMessage += "you create terminal commands to satisfy a user's query for doing engineering/programming ";
        Debug.Log("system message: \n" + systemMessage);


        // This line gets your API key (and could be slightly different on Mac/Linux)

        string key = WholeThingManager.Singleton.config.OPENAI_API_KEY;
        if (string.IsNullOrEmpty(key))
        {
            isEnabled = false;
            Debug.LogError("OPEN AI KEY NOT FOUND");
            return;
        }

        api = new OpenAIAPI(key);

        // add the system message to the messages history.
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, systemMessage)
        };

    }

    // because we give chatgpt the entire history of messages it is important to clear it because there is a max message length.
    // atm this is cleared after every prompt
    public void ClearMessages()
    {

        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, systemMessage)
        };
    }

    // takes the chatgpt output and seperates it into a string array, each string in the array is a new dialog line or an action.
    // this also is where we remove a bunch of nono
    // public string[] ProcessOutputIntoStringArray(string chatgptOutputMessage)
    // {
    //     // outputString = messages[messages.Count - 1].Content;
    //     outputString = chatgptOutputMessage;
    //     Debug.Log(outputString);
    //     // textField.text = outputString;

    //     outputString = outputString.Replace("frick", "fuck");
    //     outputString = outputString.Replace("Frick", "Fuck");
    //     outputString = outputString.Replace("Freakin", "Fuckin");
    //     outputString = outputString.Replace("freakin", "fuckin");
    //     outputString = outputString.Replace("crap", "shit");
    //     outputString = outputString.Replace("Crap", "Shit");
    //     outputString = outputString.Replace("shoot", "shit");
    //     outputString = outputString.Replace("Shoot", "Shit");

    //     string[] outputLinesProcessed = outputString.Split(Environment.NewLine,
    //                 StringSplitOptions.RemoveEmptyEntries);

    //     char[] delims = new[] { '\r', '\n' };
    //     outputLinesProcessed = outputString.Split(delims, StringSplitOptions.RemoveEmptyEntries);
    //     return outputLinesProcessed;
    // }

    public string RemoveDirectSlurs(string chatgptOutputString)
    {
        chatgptOutputString = chatgptOutputString.Replace("Nigger", "nope");
        chatgptOutputString = chatgptOutputString.Replace("Nigga", "nope");
        chatgptOutputString = chatgptOutputString.Replace("nigga", "nope");
        chatgptOutputString = chatgptOutputString.Replace("Niger", "nope");
        chatgptOutputString = chatgptOutputString.Replace("nigger", "nope");
        chatgptOutputString = chatgptOutputString.Replace("niger", "nope");
        chatgptOutputString = chatgptOutputString.Replace("negro", "nope");
        chatgptOutputString = chatgptOutputString.Replace("Negro", "nope");
        chatgptOutputString = chatgptOutputString.Replace("migger", "mope");
        chatgptOutputString = chatgptOutputString.Replace("migga", "mope");
        chatgptOutputString = chatgptOutputString.Replace("migga", "mope");
        chatgptOutputString = chatgptOutputString.Replace("miger", "mope");
        chatgptOutputString = chatgptOutputString.Replace("migger", "mope");
        chatgptOutputString = chatgptOutputString.Replace("miger", "mope");
        chatgptOutputString = chatgptOutputString.Replace("megro", "mope");
        chatgptOutputString = chatgptOutputString.Replace("megro", "mope");
        chatgptOutputString = chatgptOutputString.Replace("faggot", "fnope");
        chatgptOutputString = chatgptOutputString.Replace("Faggot", "fnope");
        chatgptOutputString = chatgptOutputString.Replace("feggot", "fnope");
        chatgptOutputString = chatgptOutputString.Replace("Feggot", "fnope");
        chatgptOutputString = chatgptOutputString.Replace("fagot", "fnope");
        chatgptOutputString = chatgptOutputString.Replace("Fagot", "fnope");
        chatgptOutputString = chatgptOutputString.Replace("Fogot", "fnope");
        chatgptOutputString = chatgptOutputString.Replace("fogot", "fnope");
        chatgptOutputString = chatgptOutputString.Replace("panigerism", "nope");
        chatgptOutputString = chatgptOutputString.Replace("Nick G", "nope");
        chatgptOutputString = chatgptOutputString.Replace("nick g", "nope");
        chatgptOutputString = chatgptOutputString.Replace("Nick g", "nope");
        chatgptOutputString = chatgptOutputString.Replace("nick g", "nope");
        return chatgptOutputString;
    }

    // ok this is the actual interaction with chatgpt.
    public async Task<string> EnterPromptAndGetResponse(string inputPrompt)
    {
        if (!isEnabled)
        {
            return string.Empty;
        }

        ClearMessages();
        // Don't submit empty messages
        if (inputPrompt.Length < 1)
        {
            Debug.Log("message is empty");
            return null;
        }


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

                    // Model = useChatGPT4 ? Model.ChatGPT4_8k : Model.ChatGPTTurbo16k,
                    // Model = Model.ChatGPTTurbo16k,
                    Model = Model.ChatGPT4_8k,
                    // Model = Model.ChatGPT4_8k_functions,
                    // Functions = GetFunctionList(),
                    // Function_Call = "auto",
                    Temperature = 1,
                    MaxTokens = 3000,
                    Messages = messages
                });

                // Get the response message and store it in a response message variable 
                ChatMessage responseMessage = new ChatMessage();
                responseMessage.Role = chatResult.Choices[0].Message.Role;
                responseMessage.Content = chatResult.Choices[0].Message.Content;
                Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));
                // Debug.Log(chatResult.Choices[0].Message.Function_Call.ToString());
                // Debug.Log(chatResult.Choices[0].Message.Function_Call.Arguments.ToString());
                // Debug.Log(chatResult.Choices[0].Message.Function_Call.Arguments);
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
        // Debug.Log("hello");

        //return the message
        return messages[messages.Count - 1].Content;

    }

    // this is just me testing shit done worry
    public static object[] GetFunctionList()
    {
        List<object> functionList = new List<object>();

        // Define the 'get_current_weather' function
        var getCurrentWeather = new
        {
            name = "get_commands",
            description = "Get a list of bash commands on an Ubuntu machine to run",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    commands = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "string",
                            description = "A terminal command string"
                        },
                        description = "List of terminal command strings to be executed"
                    },
                    example = new
                    {
                        type = "string",
                        description = "an example of using the word in a sentence"
                    }

                },
                required = new string[] { "commands"}
            }
        };

        // Add 'get_current_weather' to the function list
        functionList.Add(getCurrentWeather);

        // Return as an object array
        return functionList.ToArray();
    }
}

