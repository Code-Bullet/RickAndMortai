using OpenAI_API.Chat;
using OpenAI_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.Http;
using Newtonsoft.Json;

namespace Assets.Scripts.AIControllers
{
    public enum AuthorRole
    {
        Unknown = -1,
        System,
        User,
        Assistant
    }

    public class Message
    {
        public AuthorRole AuthorRole { get; set; }
        public string Text { get; set; }

        public Message(AuthorRole authorRole, string text)
        {
            AuthorRole = authorRole;
            Text = text;
        }

        public Message() { }
    }

    // An AI controller designed specifically to follow the protocol for this project.
    public class ProtocolAIController : AIController
    {
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

        [SerializeField]
        private int port = 9998;
        public int Port
        {
            get 
            { 
                return port; 
            }
            set
            {
                port = value;
            }
        }

        List<Message> History { get; set; }

        public override void Init()
        {
            Debug.Log("Init called");
            // this is the system message. its probably shit but it kinda works


            //systemMessage += "";

            History = new List<Message>() { new Message(AuthorRole.System, systemMessage.text) };
        }

        public override void Clear()
        {
            History.Clear();
            History.Add(new Message(AuthorRole.System, systemMessage.text));
        }

        public override async Task<string> EnterPromptAndGetResponse(string inputPrompt)
        {
            History.Add(new Message(AuthorRole.User, inputPrompt));
            using HttpClient client = new HttpClient();
            HttpResponseMessage m = await client.PostAsync($"http://localhost:{Port}", new StringContent(JsonConvert.SerializeObject(History), Encoding.UTF8, "application/json"));
            return await m.Content.ReadAsStringAsync();
        }
    }
}
