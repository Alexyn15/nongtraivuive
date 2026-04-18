using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SceneTrigger : MonoBehaviour
{
    public string targetSceneName;
    public string playerTag = "Player";
    public bool saveBeforeLoad = true;

    private static bool isSceneLoading;
    private static float blockUntilTime;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneLoading = false;
        blockUntilTime = Time.unscaledTime + 0.75f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.unscaledTime < blockUntilTime) return;
        if (isSceneLoading) return;
        if (!other.CompareTag(playerTag)) return;
        if (string.IsNullOrEmpty(targetSceneName)) return;

        if (saveBeforeLoad)
        {
            SaveController saveController = FindObjectOfType<SaveController>();
            if (saveController != null)
            {
                saveController.SaveGame(targetSceneName);
            }
        }

        PauseController.SetPause(false);
        isSceneLoading = true;
        SceneManager.LoadScene(targetSceneName);
    }
}