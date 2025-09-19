using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor.PackageManager;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;
    const string BASE_URL = "https://trail-api-y0t9.onrender.com";
    public ErrorMessage errorMessage;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator StartGame(System.Action<StartResponse> onSuccess)
    {
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(BASE_URL + "/start", ""))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                StartResponse response = JsonConvert.DeserializeObject<StartResponse>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
                Debug.Log(www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Start API failed: " + www.error);
            }
        }
    }

    public IEnumerator SubmitConnections(List<string[]> connections, System.Action<SubmitResponse> onSuccess)
    {
        SubmitRequest requestData = new SubmitRequest { connections = connections };
        string json = JsonConvert.SerializeObject(requestData);
        Debug.Log(json);
        using (UnityWebRequest www = new UnityWebRequest(BASE_URL + "/submit", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                SubmitResponse response = JsonConvert.DeserializeObject<SubmitResponse>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError("Submit API failed: " + www.error);
                errorMessage.ActivateObject(5f, www.error);
            }
        }
    }

    public IEnumerator ClearLastConnection(System.Action<ClearResponse> onSuccess)
    {
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(BASE_URL + "/clear_last", ""))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                ClearResponse response = JsonConvert.DeserializeObject<ClearResponse>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError("Clear Last API failed: " + www.error);
                errorMessage.ActivateObject(5f, www.error);
            }
        }
    }

    public IEnumerator ClearAllConnections(System.Action<ClearResponse> onSuccess)
    {
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(BASE_URL + "/clear_all", ""))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                ClearResponse response = JsonConvert.DeserializeObject<ClearResponse>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError("Clear All API failed: " + www.error);
                errorMessage.ActivateObject(5f, www.error);
            }
        }
    }

    // Classes to deserialize JSON
    [System.Serializable]
    public class StartResponse
    {
        public string level;
        public Dictionary<string, Position> task;
    }

    [System.Serializable]
    public class Position
    {
        public float x;
        public float y;
    }

    [System.Serializable]
    public class SubmitRequest
    {
        public List<string[]> connections;
    }

    [System.Serializable]
    public class SubmitResponse
    {
        public string status;
        public bool success;
        public string level;
        public Dictionary<string, Position> task;
    }

    [System.Serializable]
    public class ClearResponse
    {
        public List<string[]> connections;
    }
}
