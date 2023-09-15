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


// this script is used to get responses from chatgpt.
public class OpenAIController : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;

    public string outputString;
    public string[] outputLines;

    public SceneDirector director;

    private string systemMessage = "";


    public List<TextAsset> examples;

    public TextAsset sceneSystemMessage;

    public bool useChatGPT4 = false;
    void Start()
    {
        // this is the system message. its probably shit but it kinda works

        systemMessage += "You are in charge of writing a rick and morty episode, you can only give very basic stage directions,";
        systemMessage += " here is the list of stage directions that you can use:";
        systemMessage += " Rick walks to Morty, Rick walks to Workbench, Morty walks to Rick, Morty walks to Workbench, Rick walks to Center Stage, Morty walks to Center Stage,";
        systemMessage += " Rick and Morty enter the portal to the Yard, Rick and Morty enter the portal to the Garage.";
        systemMessage += " Dont use any other stage directions, they MUST be exactly the same as these.";
        systemMessage += " When you include stage directions, have them in square brackets.";
        systemMessage += " The whole script takes place in Ricks Garage and In the front Yard. The script always starts in the Garage.";
        systemMessage += " Rick and Morty change locations by creating a portal and entering it and exiting in the new location.";
        systemMessage += " If you wish to change location to the Yard this is done by using the stage direction [Rick and Morty enter the portal to the Yard].";
        systemMessage += " also if rick and morty are currently in the Yard and you wish to change location to the Garage then use the stage direction [Rick and Morty enter the portal to the Garage].";
        systemMessage += " Also note that only the garage has the workbench.";
        systemMessage += " In the yard there is Jerry who is mortys farther, you can control him like you control rick and morty with stage directions";
        systemMessage += " and dialog. Jerry cannot leave the yard, only rick and morty can go through portals.";
        // systemMessage += " Another location you can go to via portal is BikiniBottom from spongebob.";
        // systemMessage += " Use the direction [Rick and Morty enter the portal to BikiniBottom] to go to bikiniBottom";
        // systemMessage += " Spongebob squidward and patrick are all characters in bikiniBottom you can control them like rick and morty";
        // systemMessage += " With stage directions and dialog. They can only be used in the bikiniBottom dimension and cannot use portals";
        systemMessage += " Another location you can go to via portal is the loungeroom of the Simpsons House from the simpsons.";
        systemMessage += " Use the direction [Rick and Morty enter the portal to SimpsonsHouse] to go to the loungeroom of the Simpsons House";
        systemMessage += " Homer Bart and Marge are all characters in the simpsons house dimension and you can control them like rick and morty";
        systemMessage += " With stage directions and dialog. They can only be used in the simpsons house dimension and cannot enter portals";
        systemMessage += " when you enter a dimension try to incude all the characters in that dimension in the story.";
        systemMessage += " Another location you can go to via portal is Shreks Swamp from Shrek.";
        systemMessage += " Use the direction [Rick and Morty enter the portal to ShreksSwamp] to go to shreks swamp";
        systemMessage += " Shrek and Donkey are characters in shreks swamp dimension and you can control them like rick and morty";
        systemMessage += " With stage directions and dialog. They can only be used in Shreks Swamp dimension and cannot enter portals";
        systemMessage += " when you enter a dimension try to incude all the characters in that dimension in the story.";
        systemMessage += " Another location you can go to via portal is the Cantina from starwars";
        systemMessage += " Use the direction [Rick and Morty enter the portal to Starwars Cantina] to go to the starwars cantina";
        systemMessage += " Yoda and Jar Jar Binks are characters in the cantina and you can control them like rick and morty";
        systemMessage += " With stage directions and dialog. They can only be used in the starwars dimension and cannot enter portals";
        systemMessage += " when you enter a dimension try to incude all the characters in that dimension in the story.";
        systemMessage += " Another location you can go to via portal is a dodgy back alley";
        systemMessage += " Use the direction [Rick and Morty enter the portal to Back Alley] to go to the back alley";
        systemMessage += " there are no other characters in the back alley";
        systemMessage += " when you enter a dimension try to incude all the characters in that dimension in the story.";

        systemMessage += " Please structure the script by Stating the Person that is speaking then : separate speakers by new lines.";
        systemMessage += " If something cannot be expressed with the limited stage directions or through dialog then you can use the narrator";
        systemMessage += " to explain what is happening, do this in the same way you use other character, e.g. Narrator: Rick punches morty in the face";
        systemMessage += " Include some classic catchphrases like \"aww jeez rick\" for morty and \"wubba lubba dub dub\" for rick.";
        
        systemMessage += " Occasionally Include some stuttering for Morty while talking such as \"uh uh\" or \"re re realy\"";
        systemMessage += " Its important to use light profanity like frick and crap.";
        systemMessage += " Do not combine stage directions and dialog in a single line make sure they are seperated.";
        systemMessage += " Make sure to add some funny scifi jokes.";
        systemMessage += " Do NOT add any other directions to define tone or anything, only dialog and the specific stage directions I gave you";
        systemMessage += " Try not to go to too many dimensions in a single script, 2 - 3 max unless specifically asked by the prompt. Doing a tour of all the dimensions just becomes boring";


        systemMessage += " Here are some example scripts: \n";


        // there are example scripts defined in the inspector, and we add these to the system message.
        string nextLine = "";

        foreach (TextAsset example in examples)
        {
            systemMessage += nextLine;
            systemMessage += example.text;
            nextLine = "\nHere is another example: \n";
        }

        // This line gets your API key (and could be slightly different on Mac/Linux)
        api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User));

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
    public string[] ProcessOutputIntoStringArray(string chatgptOutputMessage)
    {
        // outputString = messages[messages.Count - 1].Content;
        outputString = chatgptOutputMessage;
        Debug.Log(outputString);
        // textField.text = outputString;

        outputString = outputString.Replace("frick", "fuck");
        outputString = outputString.Replace("Frick", "Fuck");
        outputString = outputString.Replace("Freakin", "Fuckin");
        outputString = outputString.Replace("freakin", "fuckin");
        outputString = outputString.Replace("crap", "shit");
        outputString = outputString.Replace("Crap", "Shit");
        outputString = outputString.Replace("shoot", "shit");
        outputString = outputString.Replace("Shoot", "Shit");
        outputString = outputString.Replace("Nigger", "nope");
        outputString = outputString.Replace("Nigga", "nope");
        outputString = outputString.Replace("nigga", "nope");
        outputString = outputString.Replace("Niger", "nope");
        outputString = outputString.Replace("nigger", "nope");
        outputString = outputString.Replace("niger", "nope");
        outputString = outputString.Replace("negro", "nope");
        outputString = outputString.Replace("Negro", "nope");
        outputString = outputString.Replace("migger", "mope");
        outputString = outputString.Replace("migga", "mope");
        outputString = outputString.Replace("migga", "mope");
        outputString = outputString.Replace("miger", "mope");
        outputString = outputString.Replace("migger", "mope");
        outputString = outputString.Replace("miger", "mope");
        outputString = outputString.Replace("megro", "mope");
        outputString = outputString.Replace("megro", "mope");
        outputString = outputString.Replace("faggot", "fnope");
        outputString = outputString.Replace("Faggot", "fnope");
        outputString = outputString.Replace("feggot", "fnope");
        outputString = outputString.Replace("Feggot", "fnope");
        outputString = outputString.Replace("fagot", "fnope");
        outputString = outputString.Replace("Fagot", "fnope");
        outputString = outputString.Replace("Fogot", "fnope");
        outputString = outputString.Replace("fogot", "fnope");
        outputString = outputString.Replace("panigerism", "nope");
        outputString = outputString.Replace("Nick G", "nope");
        outputString = outputString.Replace("nick g", "nope");
        outputString = outputString.Replace("Nick g", "nope");
        outputString = outputString.Replace("nick g", "nope");
        string[] outputLinesProcessed = outputString.Split(Environment.NewLine,
                    StringSplitOptions.RemoveEmptyEntries);


        char[] delims = new[] { '\r', '\n' };
        outputLinesProcessed = outputString.Split(delims, StringSplitOptions.RemoveEmptyEntries);
        return outputLinesProcessed;
    }

    // ok this is the actual interaction with chatgpt.
    public async Task<string> EnterPromptAndGetResponse(string inputPrompt)
    {
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