using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButton : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Tên scene sẽ load khi bấm New Game (phải có trong Build Settings)")]
    [SerializeField] private string targetSceneName = "MainScene";

    [Header("Save File")]
    [Tooltip("Tên file save, phải khớp với SaveController")]
    [SerializeField] private string saveFileName = "saveData.json";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnNewGameClicked()
    {
        // Xoá file save nếu có
        try
        {
            string saveLocation = Path.Combine(Application.persistentDataPath, saveFileName);
            if (File.Exists(saveLocation))
            {
                File.Delete(saveLocation);
                Debug.Log("NewGameButton: Save file deleted: " + saveLocation);
            }
            else
            {
                Debug.Log("NewGameButton: No save file to delete at: " + saveLocation);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("NewGameButton: Failed to delete save file: " + e.Message);
        }

        // Bỏ pause nếu đang pause
        PauseController.SetPause(false);

        // Load scene mới
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("NewGameButton: targetSceneName is empty");
        }
    }
}
