using UnityEngine;

public class FarmTile : MonoBehaviour
{
    public bool isHoed;
    public bool hasPlant;

    [Header("Visual")]
    public Sprite normalSprite;
    public Sprite hoedSprite;
    private SpriteRenderer sr;

    [Header("Harvest")]
    public GameObject harvestItemPrefab;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        sr.sprite = normalSprite;
    }

    // F
    public void Hoe()
    {
        if (isHoed) return;

        isHoed = true;
        hasPlant = true; // GIẢ LẬP: cuốc xong là có cây ngay
        sr.sprite = hoedSprite;
    }

    // E
    public void Harvest()
    {
        if (!isHoed || !hasPlant) return;

        if (harvestItemPrefab == null) return;

        bool added = InventoryController.Instance.AddItem(harvestItemPrefab);
        if (!added) return;

        hasPlant = false;
        isHoed = false;
        sr.sprite = normalSprite;
    }
}
