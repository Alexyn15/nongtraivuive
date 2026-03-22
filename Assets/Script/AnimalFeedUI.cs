using TMPro;
using UnityEngine;

public class AnimalFeedUI : MonoBehaviour
{
    [SerializeField] private Animal animal;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 0.8f, 0);

    private Camera cam;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        if (animal == null)
            animal = GetComponentInParent<Animal>();

        if (statusText == null)
            statusText = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        UpdateStatus();
    }

    private void LateUpdate()
    {
        if (animal == null) return;
        if (cam == null) cam = Camera.main;

        transform.position = animal.transform.position + worldOffset;
        transform.rotation = Quaternion.identity;

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (animal == null || statusText == null) return;

        if (animal.hasProduct)
        {
            statusText.text = "Ready!";
        }
        else
        {
            statusText.text = $"{animal.currentFoodCount}/{animal.foodPerProduct}";
        }
    }
}
