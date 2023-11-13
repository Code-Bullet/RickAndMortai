using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class GlobalConfigData
{
    public string OPENAI_API_KEY = "";
    public string FAKE_YOU_USERNAME_OR_EMAIL = "";
    public string FAKE_YOU_PASSWORD = "";

    public static GlobalConfigData CreateFromJSON(string str)
    {
        return JsonUtility.FromJson<GlobalConfigData>(str);
    }

    public static GlobalConfigData CreateFromEnvVars()
    {
        GlobalConfigData data = new GlobalConfigData();
        data.OPENAI_API_KEY = System.Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        data.FAKE_YOU_USERNAME_OR_EMAIL = System.Environment.GetEnvironmentVariable("FAKE_YOU_USERNAME_OR_EMAIL");
        data.FAKE_YOU_PASSWORD = System.Environment.GetEnvironmentVariable("FAKE_YOU_PASSWORD");
        return data;
    }
}