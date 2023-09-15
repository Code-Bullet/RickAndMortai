using UnityEngine;

namespace DialogueAI
{
    // this stores info for the fake you tts api call. 
    public class Dialogue
    {
        public AudioClip audioClip;
        public AudioClip clip;
        public string uuid { get; set; }
        public string character { get; set; }
        public string text { get; set; }
        public string model { get; set; }
        public string cookie { get; set; }

        public int clipNumber { get; set; }

        public bool failed { get; set; }
    }

    public class SpeakResponse
    {
        public bool success { get; set; }
        public string inference_job_token { get; set; }
    }

    //taken from https://cdn.discordapp.com/attachments/947792035040088104/993046615239692288/FakeYou.cs
    public class GetResponse
    {
        public bool success { get; set; }

        public GetResponseState state { get; set; }
    }

    //also taken from the same place
    public class GetResponseState
    {
        public string job_token { get; set; }
        public string status { get; set; }
        public string maybe_extra_status_description { get; set; }
        public int attempt_count { get; set; }
        public string maybe_result_token { get; set; }
        public string maybe_public_bucket_wav_audio_path { get; set; }
        public string model_token { get; set; }
        public string tts_model_type { get; set; }
        public string title { get; set; }
        public string raw_inference_text { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}