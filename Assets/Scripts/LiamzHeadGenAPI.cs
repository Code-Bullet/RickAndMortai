using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Text;

public class LiamzHeadGenAPI : MonoBehaviour
{
    public static async Task<AICharacter> GenerateAICharacter(string characterName)
    {
        CharacterHeadsGenerateResponse res = await GenerateHead(characterName);

        AICharacter aiCharacter = new AICharacter();
        aiCharacter.characterName = characterName;
        aiCharacter.head3d = new AIHead3D();
        aiCharacter.head3d.generationIds = res.generation_ids;
        aiCharacter.head3d.characterKey = characterName;
        //aiCharacter.prompt = prompt;
        //aiCharacter.texture = fullTexture;

        return aiCharacter;
    }


    public static async Task<CharacterHeadsGenerateResponse> GenerateHead(string character)
    {
        using HttpClient client = new HttpClient();
        string apiUrl = "http://127.0.0.1:10001/v1/character-heads/generate";
        var args = new
        {
            character = character
        };
        client.Timeout = TimeSpan.FromSeconds(60 * 8); // 8min timeout

        HttpResponseMessage res = await client.PostAsync(
            apiUrl,
            new StringContent(
                JsonConvert.SerializeObject(args),
                Encoding.UTF8,
                "application/json"
            )
        );

        var body = await res.Content.ReadAsStringAsync();
        var retval = JsonConvert.DeserializeObject<CharacterHeadsGenerateResponse>(body);

        if (retval.error != null)
        {
            throw new Exception($"Error generating 3d head: {retval.error}");
        }

        return retval;
    }

    public static async Task<CharacterHeadsGenerateResponse> GenerateHead_mock(string character)
    {
        // wait 2s
        System.Threading.Thread.Sleep(2000);
        CharacterHeadsGenerateResponse res = new CharacterHeadsGenerateResponse
        {
            generation_ids = new string[] { "6jpkjglb773m5qy5jne2h6e72y" },
            error = ""
        };
        return res;

    }
}
