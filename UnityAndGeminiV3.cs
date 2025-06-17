using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using GoogleTextToSpeech.Scripts.Data;
using GoogleTextToSpeech.Scripts;

[System.Serializable]
public class UnityAndGeminiKey
{
    public string key;
}

[System.Serializable]
public class Response
{
    public Candidate[] candidates;
}

public class ChatRequest
{
    public Content[] contents;
}

[System.Serializable]
public class Candidate
{
    public Content content;
}

[System.Serializable]
public class Content
{
    public string role; 
    public Part[] parts;
}

[System.Serializable]
public class Part
{
    public string text;
}

public class UnityAndGeminiV3 : MonoBehaviour
{
    [Header("Gemini API Password")]
    public string apiKey; 
    private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";
    private string proxyEndpoint = "http://localhost:3000/proxy/gemini"; // Update to your proxy server URL

    [Header("NPC Function")]
    [SerializeField] private TextToSpeechManager googleServices;
    private Content[] chatHistory;

    private string knowledgeBaseText;

    void Start()
    {
        chatHistory = new Content[] { };
        TextAsset knowledgeBaseAsset = Resources.Load<TextAsset>("KnowledgeBase");
        if (knowledgeBaseAsset != null)
        {
            knowledgeBaseText = knowledgeBaseAsset.text;
            Debug.Log("Knowledge base loaded: " + knowledgeBaseText);
        }
        else
        {
            Debug.LogError("Failed to load KnowledgeBase.txt from Resources!");
            knowledgeBaseText = "";
        }
    }

    private IEnumerator SendPromptRequestToGemini(string promptText)
    {
        string url = $"{apiEndpoint}?key={apiKey}";
        string fullPrompt = $"Here is the knowledge base: {knowledgeBaseText}\n\nAnswer the following question only if the answer is in the knowledge base, otherwise say 'I don’t have the answer to this question.': Respond in 25 words or fewer for concise speech output. {promptText}";
        string jsonData = "{\"contents\": [{\"parts\": [{\"text\": \"" + fullPrompt + "\"}]}]}";

        // Send request via proxy
        // string proxyJsonData = JsonUtility.ToJson(new { url = url, data = JsonUtility.FromJson<object>(jsonData) });
        // byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(proxyJsonData);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        // using (UnityWebRequest www = new UnityWebRequest(proxyEndpoint, "POST"))
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Request complete!");
                Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                {
                    string text = response.candidates[0].content.parts[0].text;
                    Debug.Log(text);
                }
                else
                {
                    Debug.Log("No text found.");
                }
            }
        }
    }

    public void SendChat(string userMessage)
    {
        StartCoroutine(SendChatRequestToGemini(userMessage));
    }

    private IEnumerator SendChatRequestToGemini(string newMessage)
    {
        string url = $"{apiEndpoint}?key={apiKey}";
        string fullMessage = $"Here is the knowledge base: {knowledgeBaseText}\n\nAnswer the following question only if the answer is in the knowledge base, otherwise say 'I don’t have the answer to this question.': Respond in 25 words or fewer for concise speech output. {newMessage}";

        Content userContent = new Content
        {
            role = "user",
            parts = new Part[]
            {
                new Part { text = fullMessage }
            }
        };

        List<Content> contentsList = new List<Content>(chatHistory);
        contentsList.Add(userContent);
        chatHistory = contentsList.ToArray(); 

        ChatRequest chatRequest = new ChatRequest { contents = chatHistory };
        string jsonData = JsonUtility.ToJson(chatRequest);

        // Send request via proxy
        // string proxyJsonData = JsonUtility.ToJson(new { url = url, data = JsonUtility.FromJson<object>(jsonData) });
        // byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(proxyJsonData);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        // using (UnityWebRequest www = new UnityWebRequest(proxyEndpoint, "POST"))
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Request complete!");
                Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                {
                    string reply = response.candidates[0].content.parts[0].text;
                    string[] words = reply.Split(' ');
                    if (words.Length > 25)
                    {
                        reply = string.Join(" ", words, 0, 25) + "...";
                    }

                    Content botContent = new Content
                    {
                        role = "model",
                        parts = new Part[]
                        {
                            new Part { text = reply }
                        }
                    };

                    Debug.Log(reply);
                    googleServices.SendTextToGoogle(reply);

                    contentsList.Add(botContent);
                    chatHistory = contentsList.ToArray();
                }
                else
                {
                    Debug.Log("No text found.");
                }
            }
        }  
    }
}




















// using System.Collections;
// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using System;
// using GoogleTextToSpeech.Scripts.Data;
// using GoogleTextToSpeech.Scripts;

// [System.Serializable]
// public class UnityAndGeminiKey
// {
//     public string key;
// }

// [System.Serializable]
// public class Response
// {
//     public Candidate[] candidates;
// }

// public class ChatRequest
// {
//     public Content[] contents;
// }

// [System.Serializable]
// public class Candidate
// {
//     public Content content;
// }

// [System.Serializable]
// public class Content
// {
//     public string role; 
//     public Part[] parts;
// }

// [System.Serializable]
// public class Part
// {
//     public string text;
// }

// public class UnityAndGeminiV3 : MonoBehaviour
// {
//     [Header("Gemini API Password")]
//     public string apiKey; 
//     private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";

//     [Header("NPC Function")]
//     [SerializeField] private TextToSpeechManager googleServices;
//     private Content[] chatHistory;

//     // Knowledge Base
//     private string knowledgeBaseText;

//     void Start()
//     {
//         chatHistory = new Content[] { };
//         // Load the knowledge base from Resources
//         TextAsset knowledgeBaseAsset = Resources.Load<TextAsset>("KnowledgeBase");
//         if (knowledgeBaseAsset != null)
//         {
//             knowledgeBaseText = knowledgeBaseAsset.text;
//             Debug.Log("Knowledge base loaded: " + knowledgeBaseText);
//         }
//         else
//         {
//             Debug.LogError("Failed to load KnowledgeBase.txt from Resources!");
//             knowledgeBaseText = "";
//         }
//     }

//     private IEnumerator SendPromptRequestToGemini(string promptText)
//     {
//         string url = $"{apiEndpoint}?key={apiKey}";
     
//         // Include the knowledge base and instruction in the prompt
//         string fullPrompt = $"Here is the knowledge base: {knowledgeBaseText}\n\nAnswer the following question only if the answer is in the knowledge base, otherwise say 'I don’t have the answer to this question.': Respond in 25 words or fewer for concise speech output. {promptText}";
//         string jsonData = "{\"contents\": [{\"parts\": [{\"text\": \"" + fullPrompt + "\"}]}]}";

//         byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

//         using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
//         {
//             www.uploadHandler = new UploadHandlerRaw(jsonToSend);
//             www.downloadHandler = new DownloadHandlerBuffer();
//             www.SetRequestHeader("Content-Type", "application/json");

//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError(www.error);
//             }
//             else
//             {
//                 Debug.Log("Request complete!");
//                 Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
//                 if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
//                 {
//                     string text = response.candidates[0].content.parts[0].text;
//                     Debug.Log(text);
//                 }
//                 else
//                 {
//                     Debug.Log("No text found.");
//                 }
//             }
//         }
//     }

//     public void SendChat(string userMessage)
//     {
//         StartCoroutine(SendChatRequestToGemini(userMessage));
//     }

//     private IEnumerator SendChatRequestToGemini(string newMessage)
//     {
//         string url = $"{apiEndpoint}?key={apiKey}";
     
//         // Include the knowledge base and instruction in the user's message
//         string fullMessage = $"Here is the knowledge base: {knowledgeBaseText}\n\nAnswer the following question only if the answer is in the knowledge base, otherwise say 'I don’t have the answer to this question.': Respond in 25 words or fewer for concise speech output. {newMessage}";

//         Content userContent = new Content
//         {
//             role = "user",
//             parts = new Part[]
//             {
//                 new Part { text = fullMessage }
//             }
//         };

//         List<Content> contentsList = new List<Content>(chatHistory);
//         contentsList.Add(userContent);
//         chatHistory = contentsList.ToArray(); 

//         ChatRequest chatRequest = new ChatRequest { contents = chatHistory };

//         string jsonData = JsonUtility.ToJson(chatRequest);

//         byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

//         using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
//         {
//             www.uploadHandler = new UploadHandlerRaw(jsonToSend);
//             www.downloadHandler = new DownloadHandlerBuffer();
//             www.SetRequestHeader("Content-Type", "application/json");

//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError(www.error);
//             }
//             else
//             {
//                 Debug.Log("Request complete!");
//                 Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
//                 if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
//                 {
//                     string reply = response.candidates[0].content.parts[0].text;
//                     string[] words = reply.Split(' ');
//                     if (words.Length > 25)
//                     {
//                         reply = string.Join(" ", words, 0, 25) + "...";
//                     }

//                     Content botContent = new Content
//                     {
//                         role = "model",
//                         parts = new Part[]
//                         {
//                             new Part { text = reply }
//                         }
//                     };

//                     Debug.Log(reply);
//                     googleServices.SendTextToGoogle(reply);

//                     contentsList.Add(botContent);
//                     chatHistory = contentsList.ToArray();
//                 }
//                 else
//                 {
//                     Debug.Log("No text found.");
//                 }
//             }
//         }  
//     }
// }




























// using System.Collections;
// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using System;
// using GoogleTextToSpeech.Scripts.Data;
// using GoogleTextToSpeech.Scripts;

// [System.Serializable]
// public class UnityAndGeminiKey
// {
//     public string key;
// }

// [System.Serializable]
// public class Response
// {
//     public Candidate[] candidates;
// }

// public class ChatRequest
// {
//     public Content[] contents;
// }

// [System.Serializable]
// public class Candidate
// {
//     public Content content;
// }

// [System.Serializable]
// public class Content
// {
//     public string role; 
//     public Part[] parts;
// }

// [System.Serializable]
// public class Part
// {
//     public string text;
// }

// public class UnityAndGeminiV3 : MonoBehaviour
// {
//     [Header("Gemini API Password")]
//     public string apiKey; 
//     private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";

//     [Header("NPC Function")]
//     [SerializeField] private TextToSpeechManager googleServices;
//     private Content[] chatHistory;

//     void Start()
//     {
//         chatHistory = new Content[] { };
//     }

//     private IEnumerator SendPromptRequestToGemini(string promptText)
//     {
//         string url = $"{apiEndpoint}?key={apiKey}";
     
//         string concisePrompt = $"Your name is Forlan. Respond in 25 words or fewer for concise speech output. When speaking the output should not be greater than 10 seconds and in extreme cases 15 seconds.   {promptText}";
//         string jsonData = "{\"contents\": [{\"parts\": [{\"text\": \"" + concisePrompt + "\"}]}]}";

//         byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

//         using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
//         {
//             www.uploadHandler = new UploadHandlerRaw(jsonToSend);
//             www.downloadHandler = new DownloadHandlerBuffer();
//             www.SetRequestHeader("Content-Type", "application/json");

//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError(www.error);
//             }
//             else
//             {
//                 Debug.Log("Request complete!");
//                 Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
//                 if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
//                 {
//                     string text = response.candidates[0].content.parts[0].text;
//                     Debug.Log(text);
//                 }
//                 else
//                 {
//                     Debug.Log("No text found.");
//                 }
//             }
//         }
//     }

//     public void SendChat(string userMessage)
//     {
//         StartCoroutine(SendChatRequestToGemini(userMessage));
//     }

//     private IEnumerator SendChatRequestToGemini(string newMessage)
//     {
//         string url = $"{apiEndpoint}?key={apiKey}";
     
//         Content userContent = new Content
//         {
//             role = "user",
//             parts = new Part[]
//             {
//                 new Part { text = $"Respond in 25 words or fewer for concise speech output. {newMessage}" }
//             }
//         };

//         List<Content> contentsList = new List<Content>(chatHistory);
//         contentsList.Add(userContent);
//         chatHistory = contentsList.ToArray(); 

//         ChatRequest chatRequest = new ChatRequest { contents = chatHistory };

//         string jsonData = JsonUtility.ToJson(chatRequest);

//         byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

//         using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
//         {
//             www.uploadHandler = new UploadHandlerRaw(jsonToSend);
//             www.downloadHandler = new DownloadHandlerBuffer();
//             www.SetRequestHeader("Content-Type", "application/json");

//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError(www.error);
//             }
//             else
//             {
//                 Debug.Log("Request complete!");
//                 Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
//                 if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
//                 {
//                     string reply = response.candidates[0].content.parts[0].text;
//                     // Truncate to ~25 words (10 seconds at 2.5 words/sec)
//                     string[] words = reply.Split(' ');
//                     if (words.Length > 25)
//                     {
//                         reply = string.Join(" ", words, 0, 25) + "...";
//                     }

//                     Content botContent = new Content
//                     {
//                         role = "model",
//                         parts = new Part[]
//                         {
//                             new Part { text = reply }
//                         }
//                     };

//                     Debug.Log(reply);
//                     googleServices.SendTextToGoogle(reply);

//                     contentsList.Add(botContent);
//                     chatHistory = contentsList.ToArray();
//                 }
//                 else
//                 {
//                     Debug.Log("No text found.");
//                 }
//             }
//         }  
//     }
// }
























// using System.Collections;
// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using System;
// using GoogleTextToSpeech.Scripts.Data;
// using GoogleTextToSpeech.Scripts;


// [System.Serializable]
// public class UnityAndGeminiKey
// {
//     public string key;
// }

// [System.Serializable]
// public class Response
// {
//     public Candidate[] candidates;
// }

// public class ChatRequest
// {
//     public Content[] contents;
// }

// [System.Serializable]
// public class Candidate
// {
//     public Content content;
// }

// [System.Serializable]
// public class Content
// {
//     public string role; 
//     public Part[] parts;
// }

// [System.Serializable]
// public class Part
// {
//     public string text;
// }


// public class UnityAndGeminiV3: MonoBehaviour
// {
//     [Header("Gemini API Password")]
//     public string apiKey; 
//     private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent"; // Edit it and choose your prefer model


//     [Header("NPC Function")]
//     [SerializeField] private TextToSpeechManager googleServices;
//     private Content[] chatHistory;

//     void Start()
//     {
//         chatHistory = new Content[] { };
//     }

//     // Functions for sending a new prompt, or a chat to Gemini
//     private IEnumerator SendPromptRequestToGemini(string promptText)
//     {
//         string url = $"{apiEndpoint}?key={apiKey}";
     
//         string jsonData = "{\"contents\": [{\"parts\": [{\"text\": \"{" + promptText + "}\"}]}]}";

//         byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

//         // Create a UnityWebRequest with the JSON data
//         using (UnityWebRequest www = new UnityWebRequest(url, "POST")){
//             www.uploadHandler = new UploadHandlerRaw(jsonToSend);
//             www.downloadHandler = new DownloadHandlerBuffer();
//             www.SetRequestHeader("Content-Type", "application/json");

//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success) {
//                 Debug.LogError(www.error);
//             } else {
//                 Debug.Log("Request complete!");
//                 Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
//                 if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
//                     {
//                         //This is the response to your request
//                         string text = response.candidates[0].content.parts[0].text;
//                         Debug.Log(text);
//                     }
//                 else
//                 {
//                     Debug.Log("No text found.");
//                 }
//             }
//         }
//     }

//     public void SendChat(string userMessage)
//     {
//         // string userMessage = inputField.text;
//         StartCoroutine( SendChatRequestToGemini(userMessage));
//     }

//     private IEnumerator SendChatRequestToGemini(string newMessage)
//     {

//         string url = $"{apiEndpoint}?key={apiKey}";
     
//         Content userContent = new Content
//         {
//             role = "user",
//             parts = new Part[]
//             {
//                 new Part { text = newMessage }
//             }
//         };

//         List<Content> contentsList = new List<Content>(chatHistory);
//         contentsList.Add(userContent);
//         chatHistory = contentsList.ToArray(); 

//         ChatRequest chatRequest = new ChatRequest { contents = chatHistory };

//         string jsonData = JsonUtility.ToJson(chatRequest);

//         byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

//         // Create a UnityWebRequest with the JSON data
//         using (UnityWebRequest www = new UnityWebRequest(url, "POST")){
//             www.uploadHandler = new UploadHandlerRaw(jsonToSend);
//             www.downloadHandler = new DownloadHandlerBuffer();
//             www.SetRequestHeader("Content-Type", "application/json");

//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success) {
//                 Debug.LogError(www.error);
//             } else {
//                 Debug.Log("Request complete!");
//                 Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
//                 if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
//                     {
//                         //This is the response to your request
//                         string reply = response.candidates[0].content.parts[0].text;
//                         Content botContent = new Content
//                         {
//                             role = "model",
//                             parts = new Part[]
//                             {
//                                 new Part { text = reply }
//                             }
//                         };

//                         Debug.Log(reply);
//                         googleServices.SendTextToGoogle(reply);


//                         //This part shows the text in the Canvas
//                         // uiText.text = reply;
//                         //This part adds the response to the chat history, for your next message
//                         contentsList.Add(botContent);
//                         chatHistory = contentsList.ToArray();
//                     }
//                 else
//                 {
//                     Debug.Log("No text found.");
//                 }
//              }
//         }  
//     }


// }


