using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using RuneRealms.Data;

namespace RuneRealms.Core
{
    /// <summary>
    /// Communication bridge between Unity and the Devvit server.
    /// Attach to a persistent GameObject named "DevvitBridge" in the scene.
    /// JS calls SendMessage('DevvitBridge', 'OnInitData', jsonString) after load.
    /// </summary>
    public class DevvitBridge : MonoBehaviour
    {
        public static DevvitBridge Instance { get; private set; }

        public event Action<InitResponse> OnInitDataReceived;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Called from JavaScript via SendMessage
        public void OnInitData(string json)
        {
            Debug.Log("[DevvitBridge] Received init data from Devvit");
            try
            {
                var data = JsonConvert.DeserializeObject<InitResponse>(json);
                if (data != null)
                {
                    OnInitDataReceived?.Invoke(data);
                }
                else
                {
                    Debug.LogError("[DevvitBridge] Failed to parse init data: result was null");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DevvitBridge] Failed to parse init data: {e.Message}");
            }
        }

        // --- HTTP Helpers ---

        public void Get<T>(string endpoint, Action<T> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(GetCoroutine(endpoint, onSuccess, onError));
        }

        public void Post<TReq, TRes>(string endpoint, TReq body, Action<TRes> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(PostCoroutine(endpoint, body, onSuccess, onError));
        }

        private IEnumerator GetCoroutine<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
        {
            using var www = UnityWebRequest.Get(endpoint);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                var errorMsg = $"GET {endpoint} failed: {www.error}";
                Debug.LogWarning($"[DevvitBridge] {errorMsg}");
                onError?.Invoke(errorMsg);
                yield break;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<T>(www.downloadHandler.text);
                onSuccess?.Invoke(data);
            }
            catch (Exception e)
            {
                var errorMsg = $"Failed to parse response from {endpoint}: {e.Message}";
                Debug.LogError($"[DevvitBridge] {errorMsg}");
                onError?.Invoke(errorMsg);
            }
        }

        private IEnumerator PostCoroutine<TReq, TRes>(string endpoint, TReq body, Action<TRes> onSuccess, Action<string> onError)
        {
            var json = JsonConvert.SerializeObject(body);
            var bodyBytes = Encoding.UTF8.GetBytes(json);

            using var www = new UnityWebRequest(endpoint, "POST");
            www.uploadHandler = new UploadHandlerRaw(bodyBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                var errorMsg = $"POST {endpoint} failed: {www.error}";
                Debug.LogWarning($"[DevvitBridge] {errorMsg}");
                onError?.Invoke(errorMsg);
                yield break;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<TRes>(www.downloadHandler.text);
                onSuccess?.Invoke(data);
            }
            catch (Exception e)
            {
                var errorMsg = $"Failed to parse response from {endpoint}: {e.Message}";
                Debug.LogError($"[DevvitBridge] {errorMsg}");
                onError?.Invoke(errorMsg);
            }
        }

        // --- Convenience Methods ---

        public void SaveSkills(PlayerSkills skills, Action<SaveSkillsResponse> onComplete = null)
        {
            var request = new SaveSkillsRequest { skills = skills };
            Post<SaveSkillsRequest, SaveSkillsResponse>("/api/save-skills", request,
                response =>
                {
                    Debug.Log($"[DevvitBridge] Skills saved: {response.success}");
                    onComplete?.Invoke(response);
                },
                error =>
                {
                    Debug.LogWarning($"[DevvitBridge] Failed to save skills: {error}");
                    onComplete?.Invoke(new SaveSkillsResponse { success = false, message = error });
                });
        }

        public void SaveArenaResult(ArenaResult result, Action<SaveArenaResultResponse> onComplete = null)
        {
            var request = new SaveArenaResultRequest { result = result };
            Post<SaveArenaResultRequest, SaveArenaResultResponse>("/api/save-arena-result", request,
                response =>
                {
                    Debug.Log($"[DevvitBridge] Arena result saved. Total kills: {response.totalKills}");
                    onComplete?.Invoke(response);
                },
                error =>
                {
                    Debug.LogWarning($"[DevvitBridge] Failed to save arena result: {error}");
                });
        }

        public void FetchLeaderboard(string type, int limit, Action<GetLeaderboardResponse> onComplete)
        {
            Get<GetLeaderboardResponse>($"/api/leaderboard?type={type}&limit={limit}", onComplete);
        }

        public void SaveTutorial(TutorialProgress tutorial, Action<SaveTutorialResponse> onComplete = null)
        {
            var request = new SaveTutorialRequest { tutorial = tutorial };
            Post<SaveTutorialRequest, SaveTutorialResponse>("/api/save-tutorial", request,
                response =>
                {
                    Debug.Log($"[DevvitBridge] Tutorial saved: {response.success}");
                    onComplete?.Invoke(response);
                },
                error =>
                {
                    Debug.LogWarning($"[DevvitBridge] Failed to save tutorial: {error}");
                    onComplete?.Invoke(new SaveTutorialResponse { success = false });
                });
        }

        public void UseItem(string itemId, Action<UseItemResponse> onComplete = null)
        {
            var request = new UseItemRequest { itemId = itemId };
            Post<UseItemRequest, UseItemResponse>("/api/inventory/use", request,
                response =>
                {
                    Debug.Log($"[DevvitBridge] Item used: {response.message}");
                    onComplete?.Invoke(response);
                },
                error =>
                {
                    Debug.LogWarning($"[DevvitBridge] Failed to use item: {error}");
                });
        }
    }
}
