using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel; // panel chứa nút Replay

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDeath -= OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
        // Pause game
        PauseController.SetPause(true);

        // Hiện UI game over nếu đã gán
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // Gắn vào nút Replay trong UI
    public void OnReplayButtonPressed()
    {
        // Bỏ pause
        PauseController.SetPause(false);

        // Reload lại scene hiện tại
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    // (tuỳ chọn) nút Quit về main menu
    public void OnQuitToMenuPressed()
    {
        PauseController.SetPause(false);
        SceneManager.LoadScene("MainMenu"); // sửa tên scene main menu nếu khác
    }
}