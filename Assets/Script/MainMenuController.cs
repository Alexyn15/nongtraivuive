using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject creditPanel;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    // ===== START GAME =====
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // ===== SETTING =====
    public void OpenSetting()
    {
        if (settingPanel != null)
            settingPanel.SetActive(true);
    }

    public void CloseSetting()
    {
        if (settingPanel != null)
            settingPanel.SetActive(false);
    }

    // ===== CREDIT =====
    public void OpenCredit()
    {
        if (creditPanel != null)
            creditPanel.SetActive(true);
    }

    public void CloseCredit()
    {
        if (creditPanel != null)
            creditPanel.SetActive(false);
    }

    // (Optional) Quit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
