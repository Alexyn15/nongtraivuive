using TMPro;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Health playerHealth;

    private void Awake()
    {
        if (healthText == null)
            healthText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                playerHealth = p.GetComponent<Health>();
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
            playerHealth.OnDeath += OnPlayerDeath;

            // sync ban đầu từ giá trị hiện tại
            OnHealthChanged(playerHealth.currentHealth, playerHealth.maxHealth);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
            playerHealth.OnDeath -= OnPlayerDeath;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (healthText == null) return;
        healthText.text = $"{current}/{max}";
    }

    private void OnPlayerDeath()
    {
        // tuỳ bạn hiển thị thêm
    }
}
