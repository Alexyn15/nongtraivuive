using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Dùng để test gọi AI model trong Editor, không dùng khi build
public class AITestCaller : MonoBehaviour
{
    [Header("AI Server")]
    [SerializeField] private string apiUrl = "http://localhost:5000/test-model";

    [TextArea]
    public string testInput = "Hello from Unity";

    [ContextMenu("Call AI Model (Editor Only)")]
    public void CallModelOnce()
    {
#if UNITY_EDITOR
        StartCoroutine(CallModelCoroutine());
#else
        Debug.LogWarning("AITestCaller: chỉ dùng trong Editor.");
#endif
    }

    private IEnumerator CallModelCoroutine()
    {
        var payload = JsonUtility.ToJson(new TestRequest { input = testInput });

        using (var req = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("AITestCaller: sending request to " + apiUrl);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AITestCaller: AI request failed: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            Debug.Log("AITestCaller: raw response = " + json);

            TestResponse resp = JsonUtility.FromJson<TestResponse>(json);
            if (resp != null)
            {
                Debug.Log("AITestCaller: model output = " + resp.output);
            }
        }
    }

    [System.Serializable]
    private class TestRequest
    {
        public string input;
    }

    [System.Serializable]
    private class TestResponse
    {
        public string output;
    }
}