using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreNode : MonoBehaviour
{
    [System.Serializable]
    public class OreVariant
    {
        public string oreName;
        public Sprite sprite;
        public int dropItemID;
        public int minDrop = 1;
        public int maxDrop = 2;
    }

    [Header("Ore Config")]
    public List<OreVariant> oreVariants = new List<OreVariant>();
    public float respawnSeconds = 60f;

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Collider2D oreCollider;

    private bool isAvailable = true;
    private OreVariant currentVariant;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (oreCollider == null) oreCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        RollRandomVariant();
        SetAvailable(true);
    }

    public bool CanMine()
    {
        return isAvailable && currentVariant != null;
    }

    public bool TryMine(InventoryController inventory, ItemDictionary itemDictionary)
    {
        if (!CanMine() || inventory == null || itemDictionary == null) return false;

        GameObject itemPrefab = itemDictionary.GetItemPrefab(currentVariant.dropItemID);
        if (itemPrefab == null)
        {
            Debug.LogWarning($"OreNode: Không tìm thấy item prefab với ID={currentVariant.dropItemID}");
            return false;
        }

        int dropAmount = Random.Range(currentVariant.minDrop, currentVariant.maxDrop + 1);
        int addedCount = 0;

        for (int i = 0; i < dropAmount; i++)
        {
            if (inventory.AddItem(itemPrefab))
            {
                addedCount++;
            }
            else
            {
                break; // inventory full
            }
        }

        if (addedCount <= 0)
        {
            Debug.Log("Túi đầy, không đào được quặng.");
            return false;
        }

        SetAvailable(false);
        StartCoroutine(RespawnRoutine());

        Debug.Log($"Đã đào {currentVariant.oreName}, nhận {addedCount} item.");
        return true;
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnSeconds);
        RollRandomVariant();
        SetAvailable(true);
    }

    private void RollRandomVariant()
    {
        if (oreVariants == null || oreVariants.Count == 0)
        {
            currentVariant = null;
            return;
        }

        currentVariant = oreVariants[Random.Range(0, oreVariants.Count)];

        if (spriteRenderer != null && currentVariant.sprite != null)
        {
            spriteRenderer.sprite = currentVariant.sprite;
        }
    }

    private void SetAvailable(bool available)
    {
        isAvailable = available;

        if (spriteRenderer != null) spriteRenderer.enabled = available;
        if (oreCollider != null) oreCollider.enabled = available;
    }
}