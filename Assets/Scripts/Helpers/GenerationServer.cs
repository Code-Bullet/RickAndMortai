using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System;

public class GenerationServer
{
    public class GenerateCall
    {
        public string author;
        public string script;

        public GenerateCall(string author, string script)
        {
            this.author = author;
            this.script = script;
        }

        public override string ToString()
        {
            return $"Author: {author}\nScript: \n{script}";
        }
    }

    public HttpListener listener;
    public int port = 8512;
    private WholeThingManager mgr;

    public GenerationServer(WholeThingManager mgr, int port)
    {
        this.port = port;
        this.mgr = mgr;
    }

    public async Task Serve()
    {
        listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{this.port}/");
        listener.Start();

        while (listener.IsListening)
        {
            // Handle request.
            HttpListenerContext ctx = await listener.GetContextAsync();
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;

            resp.AppendHeader("Access-Control-Allow-Origin", "*");
            resp.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
            resp.AddHeader("Access-Control-Allow-Methods", "GET, POST");
            resp.AddHeader("Access-Control-Max-Age", "1728000");

            if (req.HttpMethod == "OPTIONS")
            {
                resp.Close();
                continue;
            }

            try
            {
                // Parse.
                string path = req.Url.AbsolutePath.ToLower();

                if (path == "/scenes/generate")
                {
                    GenerateCall call = JsonConvert.DeserializeObject<GenerateCall>(new StreamReader(req.InputStream).ReadToEnd());
                    Debug.Log("generating script");
                    Debug.Log(call.ToString());
                    var scene = await mgr.CreateSceneFromScript(call.script, call.author, true, true);
                    //await mgr.RunScene(scene);
                    await mgr.RunAndRecord(scene, $"{scene.id}.mp4");


                    // Respond.
                    var jsonResponse = new { sceneId = scene.id };
                    string jsonResponseString = JsonConvert.SerializeObject(jsonResponse);

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonResponseString);
                    resp.ContentType = "application/json"; // Set content type as JSON
                    resp.ContentLength64 = buffer.Length;
                    Stream output = resp.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
                resp.StatusCode = 500;
                resp.StatusDescription = $"Internal server error: {ex.ToString()}";
            }
            finally
            {
                resp.Close();
            }
            
        }

        listener.Stop();
    }
}

