using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;

public class ReplicateAPI : MonoBehaviour
{
    private const string API_URL = "https://api.replicate.com/v1/predictions";
    private const string API_TOKEN = "r8_Pd1EcAQlG6TNpHhQUuzrwqTUOM0Nk571AALK2";
    public string promptToPredict; // This variable can be set in the Inspector
    public Texture2D downloadedImage; // The image will be stored here

    private bool doneDownloadingImage = false;

    // public bool doneGettingImage;
    private void Start()
    {
        // if (!string.IsNullOrEmpty(promptToPredict))
        // {
        //     GetTextureFromPrompt("2 close up images of The Joker face, 1 profile image and 1 looking front on, plain background", 512, 256);
        // }
        // else
        // {
        //     Debug.LogWarning("Prompt is empty. Please set it in the inspector.");
        // }
    }


    public async Task<AIArtStuff> DoAllTheAiArtStuffForAScene(string nameOfCharacter, string nameOfDimension)
    {
        Debug.Log("doing all the ai art stuff");
        Debug.Log(nameOfCharacter);
        Debug.Log(nameOfDimension);

        AIArtStuff artStuff = new AIArtStuff();

        if (nameOfCharacter != null)
        {
            string prompt = $"2 close up images of {nameOfCharacter}'s face, 1 profile image and 1 looking front on, plain background";
            Texture2D fullTexture = await GetTextureFromPrompt(prompt, 512, 256, 0.92f, "https://i.imgur.com/3fm0Z0o.png");

            AICharacter aiCharacter = new AICharacter();
            aiCharacter.characterName = nameOfCharacter;
            aiCharacter.prompt = prompt;
            aiCharacter.texture = fullTexture;

            artStuff.character = aiCharacter;
        }

        if (nameOfDimension != null)
        {
            string prompt = $"An environment photo of {nameOfDimension}";
            Texture2D dimensionTexture = await GetTextureFromPrompt(prompt, 1280, 720, 0.8f, null);

            AIDimension aiDimension = new AIDimension();
            aiDimension.dimensionName = nameOfDimension;
            aiDimension.prompt = prompt;
            aiDimension.texture = dimensionTexture;

            artStuff.dimension = aiDimension;
        }

        Debug.Log("done the ai stuff");
        return artStuff;
    }

    public IEnumerator StartPrediction(string prompt, int width, int height, float promptStrength, string imageUrl)
    {
        Debug.Log("Starting prediction with prompt: " + prompt);


        var requestPayload = "{\"version\": \"8beff3369e81422112d93b89ca01426147de542cd4684c244b673b105188fe5f\", ";
        requestPayload += "\"input\": {\"prompt\": \"" + prompt + "\", ";
        requestPayload += "\"width\": " + width + ", \"height\": " + height + ", \"prompt_strength\": " + promptStrength;

        if (imageUrl != null)
        {
            requestPayload += ", \"image\": \"" + imageUrl + "\"";
        }

        requestPayload += "}}";

        Debug.Log("this is the shit we sending: " + requestPayload);

        var predictionRequest = new UnityWebRequest(API_URL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestPayload);
        predictionRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        predictionRequest.downloadHandler = new DownloadHandlerBuffer();
        predictionRequest.SetRequestHeader("Authorization", "Token " + API_TOKEN);
        predictionRequest.SetRequestHeader("Content-Type", "application/json");

        yield return predictionRequest.SendWebRequest();

        if (predictionRequest.isNetworkError || predictionRequest.isHttpError)
        {
            Debug.LogError("Error initiating prediction: " + predictionRequest.error);
            doneDownloadingImage = true;

            yield break;
        }

        string jsonResponse = predictionRequest.downloadHandler.text;
        Debug.Log("Received prediction initiation response: " + jsonResponse);

        PredictionResponse response = JsonUtility.FromJson<PredictionResponse>(jsonResponse);

        if (response.status == "starting")
        {
            Debug.Log("Prediction is starting. Beginning polling...");
            StartCoroutine(PollForCompletion(response.id));
        }
        else
        {
            doneDownloadingImage = true;
        }
    }


    public async Task<Texture2D> GetTextureFromPrompt(string prompt, int width, int height, float promptStrength, string imageUrl)
    {
        downloadedImage = null;
        int maxAttempts = 2;
        int currentAttemptNo = 0;
        // if the attempt fucks up we try it agian until max attempts is reached.
        while (downloadedImage == null && currentAttemptNo < maxAttempts)
        {
            doneDownloadingImage = false;
            currentAttemptNo += 1;
            StartCoroutine(StartPrediction(prompt, width, height, promptStrength, imageUrl));
            while (!doneDownloadingImage)
            {
                await Task.Delay(1000);
            }
        }

        Debug.Log("ok its done");


        return downloadedImage;
    }

    // public IEnumerator StartPrediction(string prompt, int width, int height)
    // {
    //     Debug.Log("Starting prediction with prompt: " + prompt);

    //     var requestPayload = "{\"version\": \"8beff3369e81422112d93b89ca01426147de542cd4684c244b673b105188fe5f\", \"input\": {\"prompt\": \"" + prompt + "\", \"width\": " + width + ", \"height\": " + height + ", \"image\": " add image data here" }}";
    //     Debug.Log(requestPayload);
    //     var predictionRequest = new UnityWebRequest(API_URL, "POST");
    //     byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestPayload);
    //     predictionRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //     predictionRequest.downloadHandler = new DownloadHandlerBuffer();
    //     predictionRequest.SetRequestHeader("Authorization", "Token " + API_TOKEN);
    //     predictionRequest.SetRequestHeader("Content-Type", "application/json");

    //     yield return predictionRequest.SendWebRequest();

    //     if (predictionRequest.isNetworkError || predictionRequest.isHttpError)
    //     {
    //         Debug.LogError("Error initiating prediction: " + predictionRequest.error);
    //         doneDownloadingImage = true;

    //         yield break;
    //     }

    //     string jsonResponse = predictionRequest.downloadHandler.text;
    //     Debug.Log("Received prediction initiation response: " + jsonResponse);

    //     PredictionResponse response = JsonUtility.FromJson<PredictionResponse>(jsonResponse);

    //     if (response.status == "starting")
    //     {
    //         Debug.Log("Prediction is starting. Beginning polling...");
    //         StartCoroutine(PollForCompletion(response.id));
    //     }
    //     else
    //     {
    //         doneDownloadingImage = true;

    //     }
    // }


    // public IEnumerator StartPrediction(string prompt, int width, int height)
    // {




    //     Debug.Log("Starting prediction with prompt: " + prompt);

    //     // UnityWebRequest predictionRequest = UnityWebRequest.Post(API_URL, "POST");

    //     List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

    //     // Adding the image file to the form data
    //     if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
    //     {
    //         byte[] imageBytes = File.ReadAllBytes(imagePath);
    //         formData.Add(new MultipartFormFileSection("image", imageBytes, Path.GetFileName(imagePath), "image/jpeg")); // assuming jpeg, adjust as needed
    //     }

    //     // Adding the JSON payload to the form data
    //     var jsonString = "{\"version\": \"8beff3369e81422112d93b89ca01426147de542cd4684c244b673b105188fe5f\", \"input\": {\"prompt\": \"" + prompt + "\", \"width\": " + width + ", \"height\": " + height + "}}";
    //     formData.Add(new MultipartFormDataSection("jsonPayload", jsonString));



    //     UnityWebRequest predictionRequest = UnityWebRequest.Post(API_URL, formData);

    //     predictionRequest.downloadHandler = new DownloadHandlerBuffer();
    //     predictionRequest.SetRequestHeader("Authorization", "Token " + API_TOKEN);
    //     // The content type will be set automatically to 'multipart/form-data' when using UnityWebRequest.Post with formData

    //     yield return predictionRequest.SendWebRequest();

    //     // predictionRequest.uploadHandler = new UploadHandlerRaw(formData);
    //     // predictionRequest.downloadHandler = new DownloadHandlerBuffer();
    //     // predictionRequest.SetRequestHeader("Authorization", "Token " + API_TOKEN);
    //     // predictionRequest.SetRequestHeader("Content-Type", "multipart/form-data");

    //     // yield return predictionRequest.SendWebRequest();

    //     if (predictionRequest.isNetworkError || predictionRequest.isHttpError)
    //     {
    //         Debug.LogError("Error initiating prediction: " + predictionRequest.error);
    //         doneDownloadingImage = true;

    //         yield break;
    //     }

    //     string jsonResponse = predictionRequest.downloadHandler.text;
    //     Debug.Log("Received prediction initiation response: " + jsonResponse);

    //     PredictionResponse response = JsonUtility.FromJson<PredictionResponse>(jsonResponse);

    //     if (response.status == "starting")
    //     {
    //         Debug.Log("Prediction is starting. Beginning polling...");
    //         StartCoroutine(PollForCompletion(response.id));
    //     }
    //     else
    //     {
    //         doneDownloadingImage = true;
    //     }
    // }

    // // // ...
    // public IEnumerator StartPrediction(string prompt, int width, int height)
    // {
    //     Debug.Log("Starting prediction with prompt: " + prompt);

    //     // Convert image to base64 string
    //     string base64Image = null;
    //     if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
    //     {
    //         byte[] imageBytes = File.ReadAllBytes(imagePath);
    //         base64Image = Convert.ToBase64String(imageBytes);
    //     }

    //     var requestPayload = "{\"version\": \"8beff3369e81422112d93b89ca01426147de542cd4684c244b673b105188fe5f\", \"input\": {\"prompt\": \"" + prompt + "\", \"width\": " + width + ", \"height\": " + height;
    //     requestPayload += ", \"prompt_strength\": 0.92";
    //     if (base64Image != null)
    //     {
    //         requestPayload += ", \"image\": \"" + base64Image + "\"";
    //     }

    //     requestPayload += "}}";

    //     var predictionRequest = new UnityWebRequest(API_URL, "POST");
    //     byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestPayload);
    //     predictionRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //     predictionRequest.downloadHandler = new DownloadHandlerBuffer();
    //     predictionRequest.SetRequestHeader("Authorization", "Token " + API_TOKEN);
    //     predictionRequest.SetRequestHeader("Content-Type", "application/json");

    //     yield return predictionRequest.SendWebRequest();

    //     if (predictionRequest.isNetworkError || predictionRequest.isHttpError)
    //     {
    //         Debug.LogError("Error initiating prediction: " + predictionRequest.error);
    //         doneDownloadingImage = true;

    //         yield break;
    //     }

    //     string jsonResponse = predictionRequest.downloadHandler.text;
    //     Debug.Log("Received prediction initiation response: " + jsonResponse);

    //     PredictionResponse response = JsonUtility.FromJson<PredictionResponse>(jsonResponse);

    //     if (response.status == "starting")
    //     {
    //         Debug.Log("Prediction is starting. Beginning polling...");
    //         StartCoroutine(PollForCompletion(response.id));
    //     }
    //     else
    //     {
    //         doneDownloadingImage = true;
    //     }


    // }

    public IEnumerator PollForCompletion(string predictionId)
    {
        while (true)
        {
            yield return new WaitForSeconds(5); // Poll every 5 seconds. Adjust as necessary.

            Debug.Log("Polling prediction with ID: " + predictionId);

            UnityWebRequest predictionStatusRequest = UnityWebRequest.Get(API_URL + "/" + predictionId);
            predictionStatusRequest.SetRequestHeader("Authorization", "Token " + API_TOKEN);

            yield return predictionStatusRequest.SendWebRequest();

            if (predictionStatusRequest.isNetworkError || predictionStatusRequest.isHttpError)
            {
                Debug.LogError("Error polling prediction: " + predictionStatusRequest.error);
                doneDownloadingImage = true;
                yield break;
            }

            string jsonResponse = predictionStatusRequest.downloadHandler.text;
            Debug.Log("Received polling response: " + jsonResponse);

            PredictionResponse response = JsonUtility.FromJson<PredictionResponse>(jsonResponse);

            if (response.status == "succeeded")
            {
                if (response.output != null && response.output.Length > 0)
                {
                    Debug.Log("Prediction completed! Starting image download from URL: " + response.output[0]);

                    StartCoroutine(DownloadImage("" + response.output[0]));
                }
                else
                {
                    Debug.LogError("No output image URL found.");
                    doneDownloadingImage = true;
                }

                break;
            }
            else if (response.status == "failed")
            {


                Debug.LogError("shit failed," + response.error);
                doneDownloadingImage = true;
                break;

            }
        }
    }

    public Renderer planeRenderer1;
    public Renderer planeRenderer2;
    public Renderer planeRenderer3;
    public Renderer planeRenderer4;

    public IEnumerator DownloadImage(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Error downloading image: " + request.error);
        }
        else
        {
            // Get the downloaded texture
            downloadedImage = ((DownloadHandlerTexture)request.downloadHandler).texture;
            doneDownloadingImage = true;


            // // Assign the texture to the plane's material

        }

        doneDownloadingImage = true;
    }
}


[System.Serializable]
public class PredictionResponse
{
    public string id;
    public InputData input;
    public string[] output;
    public string status;
    public string error;
}

[System.Serializable]
public class InputData
{
    public string prompt;
}
