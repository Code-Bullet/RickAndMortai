using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.AIControllers
{
    [Serializable]
    public class GenerationParameters
    {
        [SerializeField]
        [Tooltip("IF THIS IS SET TO ANYTHING OTHER THAN NONE, IT WILL USE THE PRESET in presets/preset-name.yaml")]
        public string Preset = "None";

        [SerializeField]
        public bool DoSample = true;

        [SerializeField]
        public int MaxNewTokens = 512;

        [SerializeField]
        [Range(0, 1.999f)]
        public float Temperature = 0.7f;

        [SerializeField]
        [Range(0, 1)]
        public float TopP = 0.1f;

        [SerializeField]
        [Range(0, 1)]
        public float TypicalP = 1f;

        [SerializeField]
        [Tooltip("In units of 1e-4")]
        [Range(0, 9)]
        public float EpsilionCutoff = 0;

        [SerializeField]
        [Tooltip("In units of 1e-4")]
        [Range(0, 20)]
        public float EtaCutoff = 0;

        [SerializeField]
        [Range(0, 1)]
        public float Tfs = 1;

        [SerializeField]
        [Range(0, 1)]
        public float TopA = 1;

        [SerializeField]
        public float RepetitionPenalty = 1;

        [SerializeField]
        public float RepetitionPenaltyRange = 0;

        [SerializeField]
        [Range(0, 200)]
        public float TopK = 40;

        [SerializeField]
        [Range(0, 2000)]
        public float MinLength = 0;

        [SerializeField]
        [Range(0, 20)]
        public int NoRepeatNgramSize = 0;

        [SerializeField]
        [Range(1, 20)]
        public int NumBeams = 0;

        [SerializeField]
        [Range(0, 5)]
        public float PenaltyAlpha = 0;

        [SerializeField]
        [Range(-5, 5)]
        public float LengthPenalty = 1;

        [SerializeField]
        public bool EarlyStopping = false;

        [SerializeField]
        [Range(0, 2)]
        [Tooltip("1 = llama.cpp only")]
        public int MirostatMode = 0;

        [SerializeField]
        [Range(0, 10)]
        public float MirostatTau = 5;

        [SerializeField]
        [Range(0, 1)]
        public float MirostatEta = 0.1f;

        [SerializeField]
        [Range(-0.5f, 2.5f)]
        public float GuidanceScale = 1f;

        [SerializeField]
        public int Seed = -1;

        [SerializeField]
        public List<string> StoppingStrings = new List<string>() { "User:" };

        [SerializeField]
        public string Prompt = string.Empty;
    }

    [Serializable]
    public class ModelParameters
    {
        [SerializeField]
        public int Threads = 0;

        [SerializeField]
        public int NBatch = 512;

        [SerializeField]
        public int NGpuLayers = 0;

        [SerializeField]
        public int NContext = 2048;
    }

    public class BoogaResponse
    {
        public class BoogaResult
        {
            public string text;

            public BoogaResult(string text) { this.text = text; }
            public BoogaResult() { }
        }

        public List<BoogaResult> results;

        public BoogaResponse(List<BoogaResult> results) { this.results = results; }
        public BoogaResponse() { }
    }

    public class BoogaAIController : AIController
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
        private int port = 5000;
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

        [SerializeField]
        private string model;
        public string Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
            }
        }

        [SerializeField]
        private string prefix;
        public string Prefix
        {
            get
            {
                return prefix;
            }
            set
            {
                prefix = value;
            }
        }

        [SerializeField]
        private string suffix;
        public string Suffix
        {
            get
            {
                return suffix;
            }
            set
            {
                suffix = value;
            }
        }

        [SerializeField]
        private ModelParameters modelParameters;
        public ModelParameters ModelParameters
        {
            get
            {
                return modelParameters;
            }
            set
            {
                modelParameters = value;
            }
        }

        [SerializeField]
        private GenerationParameters generationParameters;
        public GenerationParameters GenerationParameters
        {
            get
            {
                return generationParameters;
            }
            set
            {
                generationParameters = value;
            }
        }

        private JsonSerializerSettings jsonSerializerSettings;

        private bool ready = false;

        public override void Clear()
        {
            
        }

        public override async Task<string> EnterPromptAndGetResponse(string inputPrompt)
        {
            while (!ready)
            {
                await Task.Delay(500);
            }

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            GenerationParameters.Prompt = systemMessage.text + (!string.IsNullOrEmpty(Prefix) ? $"\n{Prefix}" : "") + inputPrompt + (!string.IsNullOrEmpty(Suffix) ? $"\n{Suffix}" : "");
            //Debug.Log(JsonConvert.SerializeObject(GenerationParameters, jsonSerializerSettings));
            HttpResponseMessage m = await client.PostAsync($"http://localhost:{Port}/api/v1/generate", new StringContent(JsonConvert.SerializeObject(GenerationParameters, jsonSerializerSettings), Encoding.UTF8, "application/json"));
            //Debug.Log("Posted, getting content");
            string content = await m.Content.ReadAsStringAsync();
            //Debug.Log(content);
            BoogaResponse response = JsonConvert.DeserializeObject<BoogaResponse>(content);
            return response.results[0].text;
        }

        public async override void Init()
        {
            jsonSerializerSettings = new JsonSerializerSettings();
            DefaultContractResolver contractResolver = new DefaultContractResolver();
            contractResolver.NamingStrategy = new SnakeCaseNamingStrategy();
            jsonSerializerSettings.ContractResolver = contractResolver;


            //Debug.Log($"{{\"action\":\"load\",\"model_name\":\"{Model}\",\"args\":{JsonConvert.SerializeObject(ModelParameters, jsonSerializerSettings)}}}");
            using HttpClient client = new HttpClient();
            HttpResponseMessage m =  await client.PostAsync($"http://localhost:{Port}/api/v1/model", new StringContent($"{{\"action\":\"load\",\"model_name\":\"{Model}\",\"args\":{JsonConvert.SerializeObject(ModelParameters, jsonSerializerSettings)}}}", Encoding.UTF8, "application/json"));
            await m.Content.ReadAsStringAsync();
            ready = true;
        }
    }
}
