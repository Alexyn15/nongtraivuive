using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;

    private void Start()
    {
        if (goldText == null)
            goldText = GetComponent<TMP_Text>();

        // set giá trị ban đầu
        if (CurrencyController.Instance != null)
        {
            UpdateGoldText(CurrencyController.Instance.GetGold());
            CurrencyController.Instance.OnGlodChanged += UpdateGoldText;
        }
    }

    private void OnDestroy()
    {
        if (CurrencyController.Instance != null)
            CurrencyController.Instance.OnGlodChanged -= UpdateGoldText;
    }

    private void UpdateGoldText(int amount)
    {
        if (goldText != null)
            goldText.text = amount.ToString();
    }
}