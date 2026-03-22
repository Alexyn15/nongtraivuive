using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SceneTrigger : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("Tên scene muốn load (phải có trong Build Settings)")]
    public string targetSceneName;

    [Header("Options")]
    [Tooltip("Chỉ cho phép Player kích hoạt")]
    public string playerTag = "Player";

    [Tooltip("Tự động save trước khi chuyển (nếu có SaveController trong scene)")]
    public bool saveBeforeLoad = true;

    private void Reset()
    {
        // đảm bảo collider là trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("SceneTrigger: targetSceneName is empty");
            return;
        }

        if (saveBeforeLoad)
        {
            var saveController = FindObjectOfType<SaveController>();
            if (saveController != null)
            {
                saveController.SaveGame();
            }
        }

        // bỏ pause nếu đang pause
        PauseController.SetPause(false);

        SceneManager.LoadScene(targetSceneName);
    }
}