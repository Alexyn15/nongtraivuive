using UnityEngine;

public class Chest : MonoBehaviour, Interactable
{
    [SerializeField] private string chestID;
    [SerializeField] private bool isOpen;

    public string ChestID => chestID;
    public bool IsOpen => isOpen;

    public GameObject itemPrefab;
    public Sprite openedSprite;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(chestID))
        {
            chestID = GlobalHelper.GenerateUniqueID(gameObject);
        }
    }

    public bool CanInteract() => !isOpen;

    public void Interact()
    {
        if (!CanInteract()) return;
        OpenChest();
    }

    private void OpenChest()
    {
        SetOpened(true);

        if (itemPrefab)
        {
            Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
        }
    }

    public void SetOpened(bool opened)
    {
        isOpen = opened;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (isOpen && openedSprite != null)
        {
            spriteRenderer.sprite = openedSprite;
        }
    }
}
